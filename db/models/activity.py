from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, Enum, Text
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
import enum
from db.database import Base


class ActivityType(str, enum.Enum):
    CALL = "call"
    MEETING = "meeting"
    EMAIL = "email"
    NOTE = "note"
    TASK = "task"


class Activity(Base):
    __tablename__ = "activities"

    id = Column(Integer, primary_key=True, index=True)
    type = Column(Enum(ActivityType), nullable=False)
    subject = Column(String, nullable=False)
    description = Column(Text)
    outcome = Column(String)
    contact_id = Column(Integer, ForeignKey("contacts.id"))
    deal_id = Column(Integer, ForeignKey("deals.id"))
    user_id = Column(Integer, ForeignKey("users.id"), nullable=False)
    scheduled_at = Column(DateTime(timezone=True))
    completed_at = Column(DateTime(timezone=True))
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    # Relationships
    contact = relationship("Contact", back_populates="activities")
    deal = relationship("Deal", back_populates="activities")
    user = relationship("User", back_populates="activities")

