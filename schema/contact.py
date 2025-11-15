from pydantic import BaseModel, EmailStr
from typing import Optional, List
from datetime import datetime


class ContactBase(BaseModel):
    first_name: str
    last_name: str
    email: Optional[EmailStr] = None
    phone: Optional[str] = None
    job_title: Optional[str] = None
    company_id: Optional[int] = None
    tags: Optional[List[str]] = None


class ContactCreate(ContactBase):
    pass


class ContactUpdate(BaseModel):
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    email: Optional[EmailStr] = None
    phone: Optional[str] = None
    job_title: Optional[str] = None
    company_id: Optional[int] = None
    tags: Optional[List[str]] = None


class ContactResponse(ContactBase):
    id: int
    created_by: int
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class ContactDetail(ContactResponse):
    company_name: Optional[str] = None
    creator_name: Optional[str] = None

