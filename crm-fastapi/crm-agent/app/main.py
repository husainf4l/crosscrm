"""FastAPI main application - CRM Agent."""
from contextlib import asynccontextmanager
from typing import Optional
import os

from fastapi import FastAPI, Depends, HTTPException
from fastapi.responses import JSONResponse, HTMLResponse
from fastapi.staticfiles import StaticFiles
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.ext.asyncio import AsyncSession
from pydantic import BaseModel

from app.config.settings import settings
from app.db.database import get_db, init_db
from app.db import crud
from app.agent.scheduler import start_scheduler, shutdown_scheduler
from app.core.dependencies import get_agent_service, get_chat_service
from app.core.middleware import LoggingMiddleware, ErrorHandlingMiddleware
from app.core.exceptions import CRMException
from app.modules.agent.services.agent_service import IAgentService
from app.modules.chat.services.chat_service import IChatService
from app.modules.agent.dto.agent_dto import AgentRunRequest, AgentRunResponse, AgentListResponse
from app.modules.chat.dto.chat_dto import ChatMessageRequest, ChatMessageResponse, ChatHistoryResponse


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan: startup and shutdown."""
    # Startup
    await init_db()
    start_scheduler()
    yield
    # Shutdown
    shutdown_scheduler()


app = FastAPI(
    title="CRM Agent API",
    description="LLM-powered CRM agent with chat interface - Following crm-backend best practices",
    version=settings.API_VERSION,
    debug=settings.DEBUG,
    lifespan=lifespan
)

# Add custom middleware
app.add_middleware(LoggingMiddleware)
app.add_middleware(ErrorHandlingMiddleware)

# CORS middleware for frontend access
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify your frontend domain
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Serve static files (frontend)
static_dir = os.path.join(os.path.dirname(os.path.dirname(__file__)), "static")
if os.path.exists(static_dir):
    app.mount("/static", StaticFiles(directory=static_dir), name="static")


# Global exception handler
@app.exception_handler(CRMException)
async def crm_exception_handler(request, exc: CRMException):
    """Handle CRM exceptions."""
    return JSONResponse(
        status_code=exc.status_code,
        content={"detail": exc.detail}
    )


# Request Models (for backward compatibility - will be migrated to DTOs)
class BusinessProfileRequest(BaseModel):
    """Business profile request model."""
    business_type: str
    products: Optional[list] = None
    tone: Optional[str] = None
    daily_goal: Optional[str] = None
    keywords: Optional[list] = None


# Health Check
@app.get("/")
async def root():
    """Health check endpoint."""
    return {
        "status": "ok",
        "message": "CRM Agent is running",
        "version": settings.API_VERSION,
        "graphql_backend": settings.GRAPHQL_URL
    }


# Serve chat interface
@app.get("/chat", response_class=HTMLResponse)
async def chat_interface():
    """Serve the chat interface."""
    html_file = os.path.join(os.path.dirname(os.path.dirname(__file__)), "static", "index.html")
    if os.path.exists(html_file):
        with open(html_file, "r", encoding="utf-8") as f:
            return HTMLResponse(content=f.read())
    return HTMLResponse(content="<h1>Chat interface not found</h1>", status_code=404)


# Chat Interface Endpoints
@app.post("/chat/message", response_model=ChatMessageResponse)
async def send_chat_message(
    request: ChatMessageRequest,
    db: AsyncSession = Depends(get_db),
    chat_service: IChatService = Depends(get_chat_service)
):
    """Send a message to the CRM agent and get a response."""
    try:
        return await chat_service.send_message(db, request)
    except Exception as e:
        # Log error but return a response instead of 500
        print(f"Error in send_chat_message endpoint: {e}")
        import traceback
        traceback.print_exc()
        # Return error message in response instead of raising exception
        return ChatMessageResponse(
            user_id=request.user_id,
            message=f"I'm sorry, but I encountered an error processing your message. Please try again later.",
            agent_type="AGENT_RESPONSE"
        )


@app.get("/chat/history/{user_id}", response_model=ChatHistoryResponse)
async def get_chat_history(
    user_id: int,
    limit: int = 50,
    db: AsyncSession = Depends(get_db),
    chat_service: IChatService = Depends(get_chat_service)
):
    """Get chat history for a user."""
    try:
        return await chat_service.get_history(db, user_id, limit)
    except (RuntimeError, ValueError) as e:
        # Database not configured or validation error - return empty history
        print(f"Warning in get_chat_history: {e}")
        return ChatHistoryResponse(user_id=user_id, messages=[])
    except Exception as e:
        # Log error and return empty history instead of 500
        print(f"Error in get_chat_history: {e}")
        import traceback
        traceback.print_exc()
        # Return empty history instead of 500 error
        return ChatHistoryResponse(user_id=user_id, messages=[])


# Business Profile Endpoints
@app.post("/business-profile/{user_id}")
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


@app.get("/business-profile/{user_id}")
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


# Tasks & Progress Endpoints
@app.get("/tasks/today/{user_id}")
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


@app.get("/progress/{user_id}")
async def get_progress_endpoint(
    user_id: int,
    db: AsyncSession = Depends(get_db)
):
    """Get user progress."""
    progress = await crud.get_progress(db, user_id)
    return {"user_id": user_id, "progress": progress}


# Agent Control Endpoints
@app.post("/agents/run", response_model=AgentRunResponse)
async def run_agent_endpoint(
    request: AgentRunRequest,
    db: AsyncSession = Depends(get_db),
    agent_service: IAgentService = Depends(get_agent_service)
):
    """Manually trigger an agent to run and return the generated message."""
    return await agent_service.run_agent(db, request.user_id, request.agent_type)


@app.get("/agents/list", response_model=AgentListResponse)
async def list_agents(
    agent_service: IAgentService = Depends(get_agent_service)
):
    """List all available agent types."""
    return await agent_service.list_agents()
