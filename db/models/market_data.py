from sqlalchemy import Column, Integer, String, DateTime, Enum, Text, JSON
from sqlalchemy.sql import func
import enum
from db.database import Base


class MarketDataType(str, enum.Enum):
    TREND = "trend"
    COMPETITOR = "competitor"
    NEWS = "news"
    SENTIMENT = "sentiment"


class MarketData(Base):
    __tablename__ = "market_data"

    id = Column(Integer, primary_key=True, index=True)
    data_type = Column(Enum(MarketDataType), nullable=False)
    title = Column(String, nullable=False)
    description = Column(Text)
    source = Column(String)
    url = Column(String)
    industry = Column(String)
    region = Column(String)
    date = Column(DateTime(timezone=True))
    data_metadata = Column(JSON)
    created_at = Column(DateTime(timezone=True), server_default=func.now())

