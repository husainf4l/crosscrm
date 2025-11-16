"""DTOs for chat operations."""
from typing import List, Optional
from datetime import datetime
from pydantic import BaseModel, Field, field_validator, ConfigDict


class ChatMessageRequest(BaseModel):
    """Request DTO for sending a chat message."""
    user_id: int = Field(..., gt=0, description="User ID")
    message: str = Field(..., min_length=1, max_length=5000, description="Message text")
    
    @field_validator('user_id')
    @classmethod
    def validate_user_id(cls, v):
        if v <= 0:
            raise ValueError("user_id must be greater than 0")
        return v
    
    @field_validator('message')
    @classmethod
    def validate_message(cls, v):
        if not v or not v.strip():
            raise ValueError("message cannot be empty")
        return v.strip()
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "user_id": 1,
                "message": "What are my tasks for today?"
            }
        }
    )


class ChatMessageItem(BaseModel):
    """DTO for a single chat message."""
    id: int = Field(..., description="Message ID")
    agent_type: str = Field(..., description="Type of agent or message")
    message: str = Field(..., description="Message content")
    created_at: str = Field(..., description="ISO format timestamp")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "id": 1,
                "agent_type": "AGENT_RESPONSE",
                "message": "Here are your tasks...",
                "created_at": "2024-01-15T09:00:00"
            }
        }
    )


class ChatMessageResponse(BaseModel):
    """Response DTO for chat message."""
    user_id: int = Field(..., description="User ID")
    message: str = Field(..., description="Agent response message")
    agent_type: Optional[str] = Field(default="AGENT_RESPONSE", description="Agent type")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "user_id": 1,
                "message": "Based on your business profile...",
                "agent_type": "AGENT_RESPONSE"
            }
        }
    )


class ChatHistoryResponse(BaseModel):
    """Response DTO for chat history."""
    user_id: int = Field(..., description="User ID")
    messages: List[ChatMessageItem] = Field(..., description="List of messages")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "user_id": 1,
                "messages": [
                    {
                        "id": 1,
                        "agent_type": "USER_MESSAGE",
                        "message": "Hello",
                        "created_at": "2024-01-15T09:00:00"
                    }
                ]
            }
        }
    )

