from pydantic import BaseModel
from typing import Optional
from datetime import datetime, date
from decimal import Decimal
from db.models.deal import DealStage


class DealBase(BaseModel):
    title: str
    description: Optional[str] = None
    value: Decimal
    currency: str = "USD"
    stage: DealStage = DealStage.PROSPECTING
    probability: int = 0
    expected_close_date: Optional[date] = None
    contact_id: Optional[int] = None
    company_id: Optional[int] = None
    assigned_to: Optional[int] = None


class DealCreate(DealBase):
    pass


class DealUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    value: Optional[Decimal] = None
    currency: Optional[str] = None
    stage: Optional[DealStage] = None
    probability: Optional[int] = None
    expected_close_date: Optional[date] = None
    contact_id: Optional[int] = None
    company_id: Optional[int] = None
    assigned_to: Optional[int] = None


class DealResponse(DealBase):
    id: int
    actual_close_date: Optional[date] = None
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class DealDetail(DealResponse):
    contact_name: Optional[str] = None
    company_name: Optional[str] = None
    assigned_user_name: Optional[str] = None


class DealClose(BaseModel):
    won: bool
    actual_close_date: Optional[date] = None

