from pydantic import BaseModel
from typing import Optional
from datetime import datetime
from decimal import Decimal


class CompanyBase(BaseModel):
    name: str
    industry: Optional[str] = None
    website: Optional[str] = None
    address: Optional[str] = None
    employee_count: Optional[int] = None
    annual_revenue: Optional[Decimal] = None


class CompanyCreate(CompanyBase):
    pass


class CompanyUpdate(BaseModel):
    name: Optional[str] = None
    industry: Optional[str] = None
    website: Optional[str] = None
    address: Optional[str] = None
    employee_count: Optional[int] = None
    annual_revenue: Optional[Decimal] = None


class CompanyResponse(CompanyBase):
    id: int
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True

