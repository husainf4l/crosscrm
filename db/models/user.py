from sqlalchemy import Column, Integer, String, Boolean, DateTime, Enum
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
import enum
from db.database import Base


class UserRole(str, enum.Enum):
    ADMIN = "admin"
    MANAGER = "manager"
    SALESPERSON = "salesperson"


class User(Base):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True, index=True)
    email = Column(String, unique=True, index=True, nullable=False)
    username = Column(String, unique=True, index=True, nullable=False)
    hashed_password = Column(String, nullable=False)
    full_name = Column(String, nullable=False)
    role = Column(Enum(UserRole), default=UserRole.SALESPERSON, nullable=False)
    is_active = Column(Boolean, default=True, nullable=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationships
    deals = relationship("Deal", back_populates="assigned_user", foreign_keys="Deal.assigned_to")
    activities = relationship("Activity", back_populates="user")
    tasks_assigned = relationship("Task", back_populates="assigned_user", foreign_keys="Task.assigned_to")
    tasks_created = relationship("Task", back_populates="creator", foreign_keys="Task.created_by")
    contacts_created = relationship("Contact", back_populates="creator", foreign_keys="Contact.created_by")

