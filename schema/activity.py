from pydantic import BaseModel
from typing import Optional
from datetime import datetime
from db.models.activity import ActivityType


class ActivityBase(BaseModel):
    type: ActivityType
    subject: str
    description: Optional[str] = None
    outcome: Optional[str] = None
    contact_id: Optional[int] = None
    deal_id: Optional[int] = None
    scheduled_at: Optional[datetime] = None


class ActivityCreate(ActivityBase):
    pass


class ActivityUpdate(BaseModel):
    type: Optional[ActivityType] = None
    subject: Optional[str] = None
    description: Optional[str] = None
    outcome: Optional[str] = None
    contact_id: Optional[int] = None
    deal_id: Optional[int] = None
    scheduled_at: Optional[datetime] = None
    completed_at: Optional[datetime] = None


class ActivityResponse(ActivityBase):
    id: int
    user_id: int
    completed_at: Optional[datetime] = None
    created_at: datetime

    class Config:
        from_attributes = True


class ActivityDetail(ActivityResponse):
    user_name: Optional[str] = None
    contact_name: Optional[str] = None
    deal_title: Optional[str] = None

