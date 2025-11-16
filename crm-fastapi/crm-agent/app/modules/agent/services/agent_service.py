"""Service layer for agent operations."""
from abc import ABC, abstractmethod
from typing import List
from sqlalchemy.ext.asyncio import AsyncSession

from app.agent.prompts import AgentType
from app.modules.agent.dto.agent_dto import AgentRunResponse, AgentListResponse
from app.core.exceptions import AgentExecutionError, BusinessProfileNotFoundError


class IAgentService(ABC):
    """Interface for agent service."""
    
    @abstractmethod
    async def run_agent(
        self,
        db: AsyncSession,
        user_id: int,
        agent_type: AgentType
    ) -> AgentRunResponse:
        """Run an agent and return the generated message."""
        pass
    
    @abstractmethod
    async def list_agents(self) -> AgentListResponse:
        """List all available agent types."""
        pass


class AgentService(IAgentService):
    """Service implementation for agent operations."""
    
    def __init__(self):
        # Import will be done in the method to avoid circular imports
        pass
    
    async def _run_agent_impl(self, db, user_id, agent_type):
        """Internal method to run agent."""
        from app.agent.orchestrator import run_agent
        return await run_agent(db, user_id, agent_type)
    
    async def run_agent(
        self,
        db: AsyncSession,
        user_id: int,
        agent_type: AgentType
    ) -> AgentRunResponse:
        """Run an agent and return the generated message."""
        try:
            message = await self._run_agent_impl(db, user_id, agent_type)
            
            return AgentRunResponse(
                status="ok",
                agent_type=agent_type if isinstance(agent_type, str) else str(agent_type),
                user_id=user_id,
                message=message
            )
        except ValueError as e:
            raise AgentExecutionError(str(agent_type), str(e))
        except Exception as e:
            raise AgentExecutionError(str(agent_type), f"Unexpected error: {str(e)}")
    
    async def list_agents(self) -> AgentListResponse:
        """List all available agent types."""
        return AgentListResponse(
            agents=["REMINDER", "FOLLOW_UP", "CLOSURE", "NURTURE", "UPSELL"]
        )

