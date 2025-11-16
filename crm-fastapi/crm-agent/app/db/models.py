from sqlalchemy import Column, Integer, String, Date, Text, DateTime, JSON
from sqlalchemy.sql import func
from app.db.database import Base


class BusinessProfile(Base):
    """Business profile model for agent adaptation."""
    __tablename__ = "business_profiles"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False, index=True)
    business_type = Column(String(255), nullable=False)  # ex: chicken, cheese, cars, real estate
    products = Column(JSON, nullable=True)  # ["cheese", "labaneh", "milk"]
    tone = Column(String(255), nullable=True)  # ex: friendly, strict, professional
    daily_goal = Column(String(500), nullable=True)  # "sell 20 cheese blocks"
    keywords = Column(JSON, nullable=True)  # ["target", "follow-up", "closing"]


class Leads(Base):
    """Leads model - universal CRM data model."""
    __tablename__ = "leads"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False, index=True)
    customer_name = Column(String(255), nullable=False)
    stage = Column(String(255), nullable=True)  # Generic stage field - LLM interprets
    notes = Column(Text, nullable=True)
    updated_at = Column(DateTime(timezone=True), server_default=func.now(), onupdate=func.now())


class Task(Base):
    """Task model - universal CRM data model."""
    __tablename__ = "tasks"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False, index=True)
    title = Column(String(255), nullable=False)
    status = Column(String(255), nullable=True)  # Generic status field - LLM interprets
    due_date = Column(Date, nullable=False)


class Sales(Base):
    """Sales model - universal CRM data model."""
    __tablename__ = "sales"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False, index=True)
    customer = Column(String(255), nullable=False)
    product = Column(String(255), nullable=False)
    status = Column(String(255), nullable=True)  # Generic status field - LLM interprets
    reason_failed = Column(String(500), nullable=True)


class Targets(Base):
    """Targets model."""
    __tablename__ = "targets"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False, index=True)
    product = Column(String(255), nullable=False)
    target = Column(Integer, nullable=False)
    achieved = Column(Integer, nullable=False, default=0)


class AgentRunLog(Base):
    """Agent run log model for tracking agent messages."""
    __tablename__ = "agent_run_logs"
    
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False, index=True)
    agent_type = Column(String(50), nullable=False, index=True)
    message = Column(Text, nullable=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now(), index=True)

