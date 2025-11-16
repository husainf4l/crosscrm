"""DTOs for agent operations."""
from typing import Optional
from pydantic import BaseModel, Field, field_validator, ConfigDict
from app.agent.prompts import AgentType


class AgentRunRequest(BaseModel):
    """Request DTO for running an agent."""
    user_id: int = Field(..., gt=0, description="User ID to run agent for")
    agent_type: AgentType = Field(..., description="Type of agent to run")
    
    @field_validator('user_id')
    @classmethod
    def validate_user_id(cls, v):
        if v <= 0:
            raise ValueError("user_id must be greater than 0")
        return v
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "user_id": 1,
                "agent_type": "REMINDER"
            }
        }
    )


class AgentRunResponse(BaseModel):
    """Response DTO for agent run."""
    status: str = Field(default="ok", description="Status of the operation")
    agent_type: str = Field(..., description="Type of agent that ran")
    user_id: int = Field(..., description="User ID")
    message: str = Field(..., description="Generated message")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "status": "ok",
                "agent_type": "REMINDER",
                "user_id": 1,
                "message": "Good morning! Here are your tasks for today..."
            }
        }
    )


class AgentListResponse(BaseModel):
    """Response DTO for listing agents."""
    agents: list[str] = Field(..., description="List of available agent types")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "agents": ["REMINDER", "FOLLOW_UP", "CLOSURE", "NURTURE", "UPSELL"]
            }
        }
    )

