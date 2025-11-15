from pydantic import BaseModel
from typing import Optional
from datetime import datetime, date
from db.models.task import TaskPriority, TaskStatus


class TaskBase(BaseModel):
    title: str
    description: Optional[str] = None
    priority: TaskPriority = TaskPriority.MEDIUM
    status: TaskStatus = TaskStatus.PENDING
    due_date: Optional[date] = None
    related_contact_id: Optional[int] = None
    related_deal_id: Optional[int] = None


class TaskCreate(TaskBase):
    assigned_to: int


class TaskUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    priority: Optional[TaskPriority] = None
    status: Optional[TaskStatus] = None
    due_date: Optional[date] = None
    assigned_to: Optional[int] = None
    related_contact_id: Optional[int] = None
    related_deal_id: Optional[int] = None


class TaskResponse(TaskBase):
    id: int
    assigned_to: int
    created_by: int
    completed_at: Optional[datetime] = None
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class TaskDetail(TaskResponse):
    assigned_user_name: Optional[str] = None
    creator_name: Optional[str] = None
    contact_name: Optional[str] = None
    deal_title: Optional[str] = None

