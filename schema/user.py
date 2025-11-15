from pydantic import BaseModel, EmailStr, field_validator
from typing import Optional
from datetime import datetime
from db.models.user import UserRole


class UserBase(BaseModel):
    email: EmailStr
    username: str
    full_name: str
    role: UserRole = UserRole.SALESPERSON


class UserCreate(UserBase):
    password: str


class UserUpdate(BaseModel):
    email: Optional[EmailStr] = None
    username: Optional[str] = None
    full_name: Optional[str] = None
    role: Optional[UserRole] = None
    is_active: Optional[bool] = None


class UserResponse(UserBase):
    id: int
    is_active: bool
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class UserLogin(BaseModel):
    email: EmailStr
    password: str

