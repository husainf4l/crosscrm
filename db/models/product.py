from sqlalchemy import Column, Integer, String, DateTime, Numeric, Boolean
from sqlalchemy.sql import func
from db.database import Base


class Product(Base):
    __tablename__ = "products"

    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, nullable=False)
    description = Column(String)
    price = Column(Numeric(15, 2), nullable=False)
    category = Column(String)
    sku = Column(String, unique=True, index=True)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

