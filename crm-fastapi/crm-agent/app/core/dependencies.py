"""Dependency injection for FastAPI."""
from functools import lru_cache
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.database import get_db
from app.modules.agent.services.agent_service import IAgentService, AgentService
from app.modules.chat.services.chat_service import IChatService, ChatService


@lru_cache()
def get_agent_service() -> IAgentService:
    """Get agent service instance (singleton)."""
    return AgentService()


@lru_cache()
def get_chat_service() -> IChatService:
    """Get chat service instance (singleton)."""
    return ChatService()


# Database dependency (re-exported for convenience)
__all__ = [
    "get_db",
    "get_agent_service",
    "get_chat_service"
]

