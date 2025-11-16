"""Unit tests for AgentService."""
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
from sqlalchemy.ext.asyncio import AsyncSession

from app.modules.agent.services.agent_service import AgentService
from app.modules.agent.dto.agent_dto import AgentRunRequest, AgentRunResponse
from app.core.exceptions import AgentExecutionError
from app.agent.prompts import AgentType


@pytest.mark.asyncio
async def test_run_agent_success(mock_db):
    """Test successful agent run."""
    service = AgentService()
    
    with patch.object(service, '_run_agent_impl') as mock_run:
        mock_run.return_value = "Test message from agent"
        
        response = await service.run_agent(mock_db, 1, "REMINDER")
        
        assert response.status == "ok"
        assert response.agent_type == "REMINDER"
        assert response.user_id == 1
        assert response.message == "Test message from agent"
        mock_run.assert_called_once_with(mock_db, 1, "REMINDER")


@pytest.mark.asyncio
async def test_run_agent_validation_error(mock_db):
    """Test agent run with validation error."""
    service = AgentService()
    
    with patch.object(service, '_run_agent_impl') as mock_run:
        mock_run.side_effect = ValueError("Invalid agent type")
        
        with pytest.raises(AgentExecutionError) as exc_info:
            await service.run_agent(mock_db, 1, "INVALID")
        
        assert "INVALID" in str(exc_info.value.detail)
        assert exc_info.value.status_code == 500


@pytest.mark.asyncio
async def test_run_agent_general_error(mock_db):
    """Test agent run with general error."""
    service = AgentService()
    
    with patch.object(service, '_run_agent_impl') as mock_run:
        mock_run.side_effect = Exception("Database connection failed")
        
        with pytest.raises(AgentExecutionError) as exc_info:
            await service.run_agent(mock_db, 1, "REMINDER")
        
        assert "Unexpected error" in str(exc_info.value.detail)


@pytest.mark.asyncio
async def test_list_agents():
    """Test listing available agents."""
    service = AgentService()
    
    response = await service.list_agents()
    
    assert len(response.agents) == 5
    assert "REMINDER" in response.agents
    assert "FOLLOW_UP" in response.agents
    assert "CLOSURE" in response.agents
    assert "NURTURE" in response.agents
    assert "UPSELL" in response.agents

