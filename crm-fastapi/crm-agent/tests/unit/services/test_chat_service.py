"""Unit tests for ChatService."""
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
from sqlalchemy.ext.asyncio import AsyncSession

from app.modules.chat.services.chat_service import ChatService
from app.modules.chat.dto.chat_dto import ChatMessageRequest, ChatMessageResponse, ChatHistoryResponse
from app.core.exceptions import DatabaseError


@pytest.mark.asyncio
async def test_send_message_success(mock_db):
    """Test successful message sending."""
    service = ChatService()
    
    request = ChatMessageRequest(user_id=1, message="Hello, test message")
    
    with patch.object(service._orchestrator, 'process_user_message') as mock_process, \
         patch('app.db.crud.log_agent_run') as mock_log:
        mock_process.return_value = "Agent response message"
        mock_log.return_value = AsyncMock()
        
        response = await service.send_message(mock_db, request)
        
        assert response.user_id == 1
        assert response.message == "Agent response message"
        assert response.agent_type == "AGENT_RESPONSE"
        mock_process.assert_called_once_with(db=mock_db, user_id=1, message="Hello, test message")
        assert mock_log.call_count == 2  # User message + agent response


@pytest.mark.asyncio
async def test_send_message_logging_failure(mock_db):
    """Test message sending when logging fails."""
    service = ChatService()
    
    request = ChatMessageRequest(user_id=1, message="Hello, test message")
    
    with patch.object(service._orchestrator, 'process_user_message') as mock_process, \
         patch('app.db.crud.log_agent_run') as mock_log:
        mock_process.return_value = "Agent response message"
        mock_log.side_effect = Exception("Logging failed")
        
        # Should still succeed even if logging fails
        response = await service.send_message(mock_db, request)
        
        assert response.user_id == 1
        assert response.message == "Agent response message"


@pytest.mark.asyncio
async def test_send_message_orchestrator_error(mock_db):
    """Test message sending when orchestrator fails."""
    service = ChatService()
    
    request = ChatMessageRequest(user_id=1, message="Hello, test message")
    
    with patch.object(service._orchestrator, 'process_user_message') as mock_process:
        mock_process.side_effect = Exception("Orchestrator error")
        
        with pytest.raises(DatabaseError) as exc_info:
            await service.send_message(mock_db, request)
        
        assert "send_message" in str(exc_info.value.detail)


@pytest.mark.asyncio
async def test_get_history_success(mock_db):
    """Test successful history retrieval."""
    service = ChatService()
    
    from app.db.models import AgentRunLog
    from datetime import datetime
    
    mock_logs = [
        MagicMock(
            id=1,
            agent_type="USER_MESSAGE",
            message="Hello",
            created_at=datetime.now()
        ),
        MagicMock(
            id=2,
            agent_type="AGENT_RESPONSE",
            message="Hi there!",
            created_at=datetime.now()
        )
    ]
    
    with patch('app.db.crud.get_recent_agent_runs') as mock_get:
        mock_get.return_value = mock_logs
        
        response = await service.get_history(mock_db, 1, limit=50)
        
        assert response.user_id == 1
        assert len(response.messages) == 2
        assert response.messages[0].agent_type == "USER_MESSAGE"
        assert response.messages[1].agent_type == "AGENT_RESPONSE"


@pytest.mark.asyncio
async def test_get_history_invalid_user_id(mock_db):
    """Test history retrieval with invalid user_id."""
    service = ChatService()
    
    with pytest.raises(ValueError) as exc_info:
        await service.get_history(mock_db, 0, limit=50)
    
    assert "user_id must be greater than 0" in str(exc_info.value)


@pytest.mark.asyncio
async def test_get_history_limit_adjustment(mock_db):
    """Test history retrieval with limit adjustment."""
    service = ChatService()
    
    with patch('app.db.crud.get_recent_agent_runs') as mock_get:
        mock_get.return_value = []
        
        # Test limit too high
        await service.get_history(mock_db, 1, limit=200)
        mock_get.assert_called_with(mock_db, 1, limit=50)
        
        # Test limit too low
        await service.get_history(mock_db, 1, limit=-5)
        mock_get.assert_called_with(mock_db, 1, limit=50)

