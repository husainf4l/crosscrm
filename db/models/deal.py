from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, Numeric, Enum, Date
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
import enum
from db.database import Base


class DealStage(str, enum.Enum):
    PROSPECTING = "prospecting"
    QUALIFICATION = "qualification"
    PROPOSAL = "proposal"
    NEGOTIATION = "negotiation"
    CLOSED_WON = "closed_won"
    CLOSED_LOST = "closed_lost"


class Deal(Base):
    __tablename__ = "deals"

    id = Column(Integer, primary_key=True, index=True)
    title = Column(String, nullable=False)
    description = Column(String)
    value = Column(Numeric(15, 2), nullable=False)
    currency = Column(String, default="USD")
    stage = Column(Enum(DealStage), default=DealStage.PROSPECTING, nullable=False)
    probability = Column(Integer, default=0)  # 0-100
    expected_close_date = Column(Date)
    actual_close_date = Column(Date)
    contact_id = Column(Integer, ForeignKey("contacts.id"))
    company_id = Column(Integer, ForeignKey("companies.id"))
    assigned_to = Column(Integer, ForeignKey("users.id"))
    lead_source = Column(String)  # Where the deal came from (website, referral, cold call, etc.)
    competitor = Column(String)  # Main competitor for this deal
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationships
    contact = relationship("Contact", back_populates="deals")
    company = relationship("Company", back_populates="deals")
    assigned_user = relationship("User", foreign_keys=[assigned_to], back_populates="deals")
    activities = relationship("Activity", back_populates="deal")

