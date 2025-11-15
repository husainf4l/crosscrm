from sqlalchemy.orm import Session
from sqlalchemy import func
from typing import Dict, List
from datetime import datetime, timedelta
from db.models.deal import Deal, DealStage
from db.models.activity import Activity
from decimal import Decimal


def get_sales_metrics(db: Session, start_date: datetime = None, end_date: datetime = None) -> Dict:
    """Get sales metrics for a date range"""
    query = db.query(Deal).filter(Deal.stage == DealStage.CLOSED_WON)
    
    if start_date:
        query = query.filter(Deal.actual_close_date >= start_date.date())
    if end_date:
        query = query.filter(Deal.actual_close_date <= end_date.date())
    
    total_revenue = query.with_entities(func.sum(Deal.value)).scalar() or Decimal('0')
    deal_count = query.count()
    
    return {
        "total_revenue": float(total_revenue),
        "deal_count": deal_count,
        "average_deal_value": float(total_revenue / deal_count) if deal_count > 0 else 0
    }


def get_pipeline_metrics(db: Session) -> Dict:
    """Get pipeline metrics"""
    active_deals = db.query(Deal).filter(
        Deal.stage.notin_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST])
    )
    
    total_value = active_deals.with_entities(func.sum(Deal.value)).scalar() or Decimal('0')
    weighted_value = active_deals.with_entities(
        func.sum(Deal.value * Deal.probability / 100)
    ).scalar() or Decimal('0')
    
    # Get deal counts by stage
    deal_counts = {}
    for stage in DealStage:
        if stage not in [DealStage.CLOSED_WON, DealStage.CLOSED_LOST]:
            count = db.query(Deal).filter(Deal.stage == stage).count()
            deal_counts[stage.value.lower()] = count
    
    return {
        "total_pipeline_value": float(total_value),
        "weighted_pipeline_value": float(weighted_value),
        "deal_count": active_deals.count(),
        "deal_counts": deal_counts
    }


def get_sales_trends(db: Session, period_days: int = 30) -> List[Dict]:
    """Get sales trends over time"""
    end_date = datetime.now()
    start_date = end_date - timedelta(days=period_days)
    
    deals = db.query(Deal).filter(
        Deal.stage == DealStage.CLOSED_WON,
        Deal.actual_close_date >= start_date.date()
    ).all()
    
    # Group by date
    daily_sales = {}
    for deal in deals:
        date_key = deal.actual_close_date.isoformat()
        if date_key not in daily_sales:
            daily_sales[date_key] = {"date": date_key, "revenue": 0, "count": 0}
        daily_sales[date_key]["revenue"] += float(deal.value)
        daily_sales[date_key]["count"] += 1
    
    return sorted(daily_sales.values(), key=lambda x: x["date"])


def get_performance_by_salesperson(db: Session) -> List[Dict]:
    """Get performance metrics by salesperson"""
    from db.models.user import User
    
    users = db.query(User).all()
    performance = []
    
    for user in users:
        deals = db.query(Deal).filter(Deal.assigned_to == user.id).all()
        won_deals = [d for d in deals if d.stage == DealStage.CLOSED_WON]
        active_deals = [d for d in deals if d.stage not in [DealStage.CLOSED_WON, DealStage.CLOSED_LOST]]
        
        total_revenue = sum(float(d.value) for d in won_deals)
        pipeline_value = sum(float(d.value) for d in active_deals)
        
        performance.append({
            "user_id": user.id,
            "user_name": user.full_name,
            "total_revenue": total_revenue,
            "won_deals": len(won_deals),
            "active_deals": len(active_deals),
            "pipeline_value": pipeline_value
        })
    
    return sorted(performance, key=lambda x: x["total_revenue"], reverse=True)


def get_win_rate(db: Session) -> Dict:
    """Get win/loss ratio"""
    total_deals = db.query(Deal).filter(
        Deal.stage.in_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST])
    ).count()
    
    won_deals = db.query(Deal).filter(Deal.stage == DealStage.CLOSED_WON).count()
    
    win_rate = (won_deals / total_deals * 100) if total_deals > 0 else 0
    
    return {
        "win_rate": win_rate,
        "won_deals": won_deals,
        "lost_deals": total_deals - won_deals,
        "total_closed": total_deals
    }

