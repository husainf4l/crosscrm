"""API routes for chat, agents, and business profiles."""
from typing import Optional

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession
from pydantic import BaseModel

from app.db.database import get_db
from app.db import crud
from app.agent.orchestrator import AgentOrchestrator, run_agent
from app.agent.prompts import AgentType


# Request Models
class BusinessProfileRequest(BaseModel):
    """Business profile request model."""
    business_type: str
    products: Optional[list] = None
    tone: Optional[str] = None
    daily_goal: Optional[str] = None
    keywords: Optional[list] = None


class AgentRunRequest(BaseModel):
    """Agent run request model."""
    user_id: int
    agent_type: AgentType


class ChatMessageRequest(BaseModel):
    """Chat message request model."""
    user_id: int
    message: str


class ChatResponse(BaseModel):
    """Chat response model."""
    user_id: int
    message: str
    agent_type: Optional[str] = None


# Initialize router
router = APIRouter(prefix="/api", tags=["api"])


# Chat Endpoints
@router.post("/chat/message", response_model=ChatResponse)
async def send_chat_message(
    body: ChatMessageRequest,
    db: AsyncSession = Depends(get_db)
):
    """Send a message to the CRM agent and get a response."""
    try:
        orchestrator = AgentOrchestrator()
        
        # Process user message with LLM
        response_text = await orchestrator.process_user_message(
            db=db,
            user_id=body.user_id,
            message=body.message
        )
        
        # Log the user message and agent response
        await crud.log_agent_run(
            db=db,
            user_id=body.user_id,
            agent_type="USER_MESSAGE",
            message=body.message
        )
        
        await crud.log_agent_run(
            db=db,
            user_id=body.user_id,
            agent_type="AGENT_RESPONSE",
            message=response_text
        )
        
        return ChatResponse(
            user_id=body.user_id,
            message=response_text,
            agent_type="AGENT_RESPONSE"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/chat/history/{user_id}")
async def get_chat_history(
    user_id: int,
    limit: int = 50,
    db: AsyncSession = Depends(get_db)
):
    """Get chat history for a user."""
    try:
        recent_runs = await crud.get_recent_agent_runs(db, user_id, limit=limit)
        return {
            "user_id": user_id,
            "messages": [
                {
                    "id": run.id,
                    "agent_type": run.agent_type,
                    "message": run.message,
                    "created_at": run.created_at.isoformat()
                }
                for run in recent_runs
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# Business Profile Endpoints
@router.post("/business-profile/{user_id}")
async def create_business_profile(
    user_id: int,
    profile: BusinessProfileRequest,
    db: AsyncSession = Depends(get_db)
):
    """Create or update business profile."""
    business_profile = await crud.create_or_update_business_profile(
        db=db,
        user_id=user_id,
        business_type=profile.business_type,
        products=profile.products,
        tone=profile.tone,
        daily_goal=profile.daily_goal,
        keywords=profile.keywords
    )
    return {"status": "success", "profile_id": business_profile.id}


@router.get("/business-profile/{user_id}")
async def get_business_profile_endpoint(
    user_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get business profile."""
    profile = await crud.get_business_profile(db, user_id)
    if not profile:
        raise HTTPException(status_code=404, detail="Business profile not found")
    
    return {
        "id": profile.id,
        "user_id": profile.user_id,
        "business_type": profile.business_type,
        "products": profile.products,
        "tone": profile.tone,
        "daily_goal": profile.daily_goal,
        "keywords": profile.keywords
    }


# Task Endpoints
@router.get("/tasks/today/{user_id}")
async def get_today_tasks_endpoint(
    user_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get today's tasks."""
    tasks = await crud.get_today_tasks(db, user_id)
    return {
        "user_id": user_id,
        "tasks": [
            {
                "id": task.id,
                "title": task.title,
                "status": task.status,
                "due_date": str(task.due_date)
            }
            for task in tasks
        ]
    }


# Progress Endpoints
@router.get("/progress/{user_id}")
async def get_progress_endpoint(
    user_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get user progress."""
    progress = await crud.get_progress(db, user_id)
    return {"user_id": user_id, "progress": progress}


# Agent Control Endpoints
@router.post("/agents/run")
async def run_agent_endpoint(
    body: AgentRunRequest,
    db: AsyncSession = Depends(get_db)
):
    """Manually trigger an agent to run and return the generated message."""
    try:
        message = await run_agent(db, body.user_id, body.agent_type)
        return {
            "status": "ok",
            "agent_type": body.agent_type,
            "user_id": body.user_id,
            "message": message
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/agents/list")
def list_agents():
    """List all available agent types."""
    return {
        "agents": [
            "REMINDER",
            "FOLLOW_UP",
            "CLOSURE",
            "NURTURE",
            "UPSELL"
        ]
    }
