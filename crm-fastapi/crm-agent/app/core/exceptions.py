"""Custom exceptions for the CRM Agent application."""
from fastapi import HTTPException, status


class CRMException(HTTPException):
    """Base exception for CRM Agent."""
    
    def __init__(self, detail: str, status_code: int = status.HTTP_500_INTERNAL_SERVER_ERROR):
        super().__init__(status_code=status_code, detail=detail)


class BusinessProfileNotFoundError(CRMException):
    """Raised when business profile is not found."""
    
    def __init__(self, user_id: int):
        super().__init__(
            detail=f"Business profile not found for user {user_id}",
            status_code=status.HTTP_404_NOT_FOUND
        )


class AgentExecutionError(CRMException):
    """Raised when agent execution fails."""
    
    def __init__(self, agent_type: str, reason: str):
        super().__init__(
            detail=f"Agent {agent_type} execution failed: {reason}",
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR
        )


class DatabaseError(CRMException):
    """Raised when database operation fails."""
    
    def __init__(self, operation: str, reason: str):
        super().__init__(
            detail=f"Database {operation} failed: {reason}",
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR
        )


class ValidationError(CRMException):
    """Raised when validation fails."""
    
    def __init__(self, field: str, reason: str):
        super().__init__(
            detail=f"Validation failed for {field}: {reason}",
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY
        )

