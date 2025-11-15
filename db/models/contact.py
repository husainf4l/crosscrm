from sqlalchemy import Column, Integer, String, DateTime, ForeignKey, ARRAY
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from db.database import Base


class Contact(Base):
    __tablename__ = "contacts"

    id = Column(Integer, primary_key=True, index=True)
    first_name = Column(String, nullable=False)
    last_name = Column(String, nullable=False)
    email = Column(String, index=True)
    phone = Column(String)
    job_title = Column(String)
    company_id = Column(Integer, ForeignKey("companies.id"))
    tags = Column(ARRAY(String))
    lead_score = Column(Integer, default=0)  # 0-100 lead scoring
    lead_source = Column(String)  # Where the lead came from (website, referral, etc.)
    lifecycle_stage = Column(String, default="lead")  # lead, qualified, customer, etc.
    created_by = Column(Integer, ForeignKey("users.id"))
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationships
    company = relationship("Company", back_populates="contacts")
    creator = relationship("User", foreign_keys=[created_by], back_populates="contacts_created")
    activities = relationship("Activity", back_populates="contact")
    deals = relationship("Deal", back_populates="contact")

