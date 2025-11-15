"""Insight AI Agent"""
from typing import List, Dict
from db.models.market_data import MarketData
from db.models.deal import Deal


def generate_market_insights(market_data: List[MarketData], sales_data: List[Dict]) -> List[str]:
    """Generate market insights from market and sales data"""
    insights = []
    
    if not market_data and not sales_data:
        return ["No data available for insights"]
    
    # Simple insight generation
    if sales_data:
        total_revenue = sum(d.get("revenue", 0) for d in sales_data)
        insights.append(f"Total sales revenue: ${total_revenue:,.2f}")
    
    if market_data:
        trends = [d for d in market_data if d.data_type.value == "trend"]
        if trends:
            insights.append(f"{len(trends)} market trends identified")
    
    return insights


def analyze_competitor_activity(competitor_data: List[MarketData]) -> Dict:
    """Analyze competitor activity"""
    if not competitor_data:
        return {"count": 0, "insights": []}
    
    industries = {}
    for data in competitor_data:
        industry = data.industry or "Unknown"
        industries[industry] = industries.get(industry, 0) + 1
    
    return {
        "count": len(competitor_data),
        "industries": industries,
        "insights": [f"Competitor activity in {len(industries)} industries"]
    }


def identify_market_opportunities(market_data: List[MarketData], deals: List[Deal]) -> List[str]:
    """Identify market opportunities"""
    opportunities = []
    
    # Simple opportunity identification
    if market_data:
        opportunities.append("Market data suggests growing demand in target industries")
    
    active_deals = [d for d in deals if d.stage.value not in ["closed_won", "closed_lost"]]
    if len(active_deals) > 10:
        opportunities.append(f"Strong pipeline with {len(active_deals)} active deals")
    
    return opportunities

