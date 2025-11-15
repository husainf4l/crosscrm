from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, Enum, Text, Date
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
import enum
from db.database import Base


class TaskPriority(str, enum.Enum):
    LOW = "low"
    MEDIUM = "medium"
    HIGH = "high"
    URGENT = "urgent"


class TaskStatus(str, enum.Enum):
    PENDING = "pending"
    IN_PROGRESS = "in_progress"
    COMPLETED = "completed"
    CANCELLED = "cancelled"


class Task(Base):
    __tablename__ = "tasks"

    id = Column(Integer, primary_key=True, index=True)
    title = Column(String, nullable=False)
    description = Column(Text)
    priority = Column(Enum(TaskPriority), default=TaskPriority.MEDIUM)
    status = Column(Enum(TaskStatus), default=TaskStatus.PENDING)
    due_date = Column(Date)
    completed_at = Column(DateTime(timezone=True))
    assigned_to = Column(Integer, ForeignKey("users.id"), nullable=False)
    created_by = Column(Integer, ForeignKey("users.id"), nullable=False)
    related_contact_id = Column(Integer, ForeignKey("contacts.id"))
    related_deal_id = Column(Integer, ForeignKey("deals.id"))
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationships
    assigned_user = relationship("User", foreign_keys=[assigned_to], back_populates="tasks_assigned")
    creator = relationship("User", foreign_keys=[created_by], back_populates="tasks_created")

