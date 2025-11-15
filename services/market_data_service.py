from sqlalchemy.orm import Session
from typing import List, Optional
from db.models.market_data import MarketData, MarketDataType
from datetime import datetime


def get_market_data(
    db: Session,
    skip: int = 0,
    limit: int = 20,
    data_type: Optional[MarketDataType] = None
) -> tuple[List[MarketData], int]:
    """Get market data with pagination and filters"""
    query = db.query(MarketData)
    
    if data_type:
        query = query.filter(MarketData.data_type == data_type)
    
    total = query.count()
    data = query.order_by(MarketData.created_at.desc()).offset(skip).limit(limit).all()
    
    return data, total


def get_market_data_by_id(db: Session, data_id: int) -> Optional[MarketData]:
    """Get market data by ID"""
    return db.query(MarketData).filter(MarketData.id == data_id).first()


def create_market_data(
    db: Session,
    data_type: MarketDataType,
    title: str,
    description: Optional[str] = None,
    source: Optional[str] = None,
    url: Optional[str] = None,
    industry: Optional[str] = None,
    region: Optional[str] = None,
    date: Optional[datetime] = None,
    metadata: Optional[dict] = None
) -> MarketData:
    """Create market data entry"""
    db_data = MarketData(
        data_type=data_type,
        title=title,
        description=description,
        source=source,
        url=url,
        industry=industry,
        region=region,
        date=date or datetime.now(),
        metadata=metadata
    )
    db.add(db_data)
    db.commit()
    db.refresh(db_data)
    return db_data

