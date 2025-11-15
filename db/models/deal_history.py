"""Deal Value History Model for tracking deal changes"""
from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, Numeric
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from db.database import Base


class DealHistory(Base):
    """Track deal value and stage changes over time"""
    __tablename__ = "deal_history"

    id = Column(Integer, primary_key=True, index=True)
    deal_id = Column(Integer, ForeignKey("deals.id"), nullable=False)
    old_value = Column(Numeric(15, 2))
    new_value = Column(Numeric(15, 2))
    old_stage = Column(String)
    new_stage = Column(String)
    old_probability = Column(Integer)
    new_probability = Column(Integer)
    changed_by = Column(Integer, ForeignKey("users.id"))
    change_reason = Column(String)  # Optional reason for change
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    # Relationships
    deal = relationship("Deal")
    user = relationship("User", foreign_keys=[changed_by])

