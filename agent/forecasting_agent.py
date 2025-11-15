"""Forecasting AI Agent"""
from typing import List, Dict
from datetime import datetime, timedelta
from db.models.deal import Deal, DealStage
from decimal import Decimal


def forecast_sales(historical_data: List[Dict], period_days: int = 30) -> Dict:
    """Forecast future sales based on historical data"""
    if not historical_data:
        return {"forecasted_revenue": 0, "forecasted_deals": 0}
    
    # Simple average-based forecasting
    total_revenue = sum(d.get("revenue", 0) for d in historical_data)
    total_deals = len(historical_data)
    
    avg_daily_revenue = total_revenue / len(historical_data) if historical_data else 0
    avg_daily_deals = total_deals / len(historical_data) if historical_data else 0
    
    forecasted_revenue = avg_daily_revenue * period_days
    forecasted_deals = int(avg_daily_deals * period_days)
    
    return {
        "forecasted_revenue": forecasted_revenue,
        "forecasted_deals": forecasted_deals,
        "period_days": period_days
    }


def predict_deal_close(deal: Deal) -> float:
    """Predict probability of deal closing"""
    # Use deal's existing probability as base
    base_probability = deal.probability
    
    # Adjust based on stage
    stage_multipliers = {
        DealStage.PROSPECTING: 0.3,
        DealStage.QUALIFICATION: 0.5,
        DealStage.PROPOSAL: 0.7,
        DealStage.NEGOTIATION: 0.85,
    }
    
    multiplier = stage_multipliers.get(deal.stage, 0.5)
    adjusted_probability = base_probability * multiplier
    
    return min(100, max(0, adjusted_probability))


def identify_at_risk_deals(deals: List[Deal]) -> List[Deal]:
    """Identify deals at risk of being lost"""
    at_risk = []
    
    for deal in deals:
        if deal.stage in [DealStage.CLOSED_WON, DealStage.CLOSED_LOST]:
            continue
        
        # Deals with low probability
        if deal.probability < 30:
            at_risk.append(deal)
        
        # Deals in negotiation for too long (if we had date tracking)
        # This is simplified - in production, check actual dates
    
    return at_risk

