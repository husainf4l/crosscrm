"""Service layer for chat operations."""
from abc import ABC, abstractmethod
from typing import List
from sqlalchemy.ext.asyncio import AsyncSession

from app.modules.chat.dto.chat_dto import (
    ChatMessageRequest,
    ChatMessageResponse,
    ChatHistoryResponse,
    ChatMessageItem
)
from app.core.exceptions import DatabaseError
from fastapi import HTTPException, status
from app.db import crud


class IChatService(ABC):
    """Interface for chat service."""
    
    @abstractmethod
    async def send_message(
        self,
        db: AsyncSession,
        request: ChatMessageRequest
    ) -> ChatMessageResponse:
        """Send a message and get agent response."""
        pass
    
    @abstractmethod
    async def get_history(
        self,
        db: AsyncSession,
        user_id: int,
        limit: int = 50
    ) -> ChatHistoryResponse:
        """Get chat history for a user."""
        pass


class ChatService(IChatService):
    """Service implementation for chat operations."""
    
    def __init__(self):
        from app.agent.orchestrator import AgentOrchestrator
        self._orchestrator = AgentOrchestrator()
    
    async def send_message(
        self,
        db: AsyncSession,
        request: ChatMessageRequest
    ) -> ChatMessageResponse:
        """Send a message and get agent response."""
        try:
            # Process user message with LLM
            response_text = await self._orchestrator.process_user_message(
                db=db,
                user_id=request.user_id,
                message=request.message
            )
            
            # Log the user message and agent response (if database is available)
            if db is not None:
                try:
                    await crud.log_agent_run(
                        db=db,
                        user_id=request.user_id,
                        agent_type="USER_MESSAGE",
                        message=request.message
                    )
                    
                    await crud.log_agent_run(
                        db=db,
                        user_id=request.user_id,
                        agent_type="AGENT_RESPONSE",
                        message=response_text
                    )
                except Exception as e:
                    # Log error but don't fail the request
                    print(f"Warning: Failed to log messages: {e}")
            
            return ChatMessageResponse(
                user_id=request.user_id,
                message=response_text,
                agent_type="AGENT_RESPONSE"
            )
        except Exception as e:
            # Log the error for debugging
            print(f"Error in send_message: {e}")
            import traceback
            traceback.print_exc()
            # Return error message instead of raising exception
            return ChatMessageResponse(
                user_id=request.user_id,
                message=f"I'm sorry, but I encountered an error: {str(e)[:200]}",
                agent_type="AGENT_RESPONSE"
            )
    
    async def get_history(
        self,
        db: AsyncSession,
        user_id: int,
        limit: int = 50
    ) -> ChatHistoryResponse:
        """Get chat history for a user."""
        if user_id <= 0:
            raise ValueError("user_id must be greater than 0")
        
        if limit <= 0 or limit > 100:
            limit = 50
        
        try:
            # Check if database is available
            if db is None:
                # Return empty history if database is not available
                return ChatHistoryResponse(
                    user_id=user_id,
                    messages=[]
                )
            
            recent_runs = await crud.get_recent_agent_runs(db, user_id, limit=limit)
            
            messages = [
                ChatMessageItem(
                    id=run.id,
                    agent_type=run.agent_type,
                    message=run.message,
                    created_at=run.created_at.isoformat()
                )
                for run in recent_runs
            ]
            
            return ChatHistoryResponse(
                user_id=user_id,
                messages=messages
            )
        except ValueError:
            # Re-raise ValueError as-is (validation errors)
            raise
        except RuntimeError as e:
            # Database not configured - return empty history
            print(f"Warning: Database not available: {e}")
            return ChatHistoryResponse(
                user_id=user_id,
                messages=[]
            )
        except Exception as e:
            # Log the actual error for debugging
            print(f"Error getting chat history: {e}")
            import traceback
            traceback.print_exc()
            raise DatabaseError("get_history", str(e))

