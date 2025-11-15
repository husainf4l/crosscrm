from sqlalchemy import Column, Integer, String, DateTime, Numeric
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from db.database import Base


class Company(Base):
    __tablename__ = "companies"

    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, nullable=False, index=True)
    industry = Column(String)
    website = Column(String)
    address = Column(String)
    employee_count = Column(Integer)
    annual_revenue = Column(Numeric(15, 2))
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationships
    contacts = relationship("Contact", back_populates="company")
    deals = relationship("Deal", back_populates="company")

