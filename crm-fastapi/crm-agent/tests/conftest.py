"""Pytest configuration and fixtures."""
import pytest
from unittest.mock import AsyncMock, MagicMock
from sqlalchemy.ext.asyncio import AsyncSession

from app.modules.agent.services.agent_service import AgentService
from app.modules.chat.services.chat_service import ChatService
from app.core.graphql_client import GraphQLClient


@pytest.fixture
def mock_db():
    """Mock database session."""
    db = AsyncMock(spec=AsyncSession)
    return db


@pytest.fixture
def mock_agent_service():
    """Mock agent service."""
    service = MagicMock(spec=AgentService)
    return service


@pytest.fixture
def mock_chat_service():
    """Mock chat service."""
    service = MagicMock(spec=ChatService)
    return service


@pytest.fixture
def mock_graphql_client():
    """Mock GraphQL client."""
    client = MagicMock(spec=GraphQLClient)
    client.query = AsyncMock(return_value={})
    client.mutate = AsyncMock(return_value={})
    return client

