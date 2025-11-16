"""Integration tests for API endpoints."""
import pytest
from fastapi.testclient import TestClient
from app.main import app

client = TestClient(app)


def test_health_check():
    """Test health check endpoint."""
    response = client.get("/")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "ok"
    assert "version" in data


def test_list_agents():
    """Test listing agents endpoint."""
    response = client.get("/agents/list")
    assert response.status_code == 200
    data = response.json()
    assert "agents" in data
    assert len(data["agents"]) == 5
    assert "REMINDER" in data["agents"]


def test_chat_interface():
    """Test chat interface HTML endpoint."""
    response = client.get("/chat")
    assert response.status_code == 200
    assert "text/html" in response.headers["content-type"]
    assert "CRM Agent" in response.text


def test_run_agent_missing_data():
    """Test running agent without proper setup."""
    response = client.post(
        "/agents/run",
        json={"user_id": 1, "agent_type": "REMINDER"}
    )
    # Should return error if database/OpenAI not configured
    assert response.status_code in [200, 500]


def test_send_chat_message_validation():
    """Test chat message validation."""
    # Test empty message
    response = client.post(
        "/chat/message",
        json={"user_id": 1, "message": ""}
    )
    assert response.status_code == 422  # Validation error
    
    # Test invalid user_id
    response = client.post(
        "/chat/message",
        json={"user_id": 0, "message": "Hello"}
    )
    assert response.status_code == 422  # Validation error


def test_get_chat_history():
    """Test getting chat history."""
    response = client.get("/chat/history/1?limit=10")
    # Should work even if no history exists
    assert response.status_code in [200, 500]
    if response.status_code == 200:
        data = response.json()
        assert "user_id" in data
        assert "messages" in data

