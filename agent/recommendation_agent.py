"""Recommendation AI Agent"""
from typing import Optional, List
from datetime import datetime
from db.models.contact import Contact
from db.models.deal import Deal
from db.models.activity import Activity


def get_next_best_action(contact: Contact, activities: List[Activity]) -> str:
    """Get recommended next action for a contact"""
    if not activities:
        return "Schedule an initial discovery call to understand their needs"
    
    last_activity = activities[0]  # Assuming sorted by date
    
    days_since_last = (datetime.now() - last_activity.created_at).days
    
    if days_since_last > 30:
        return "Re-engage with a check-in call or email"
    elif days_since_last > 14:
        return "Follow up on previous conversation"
    elif last_activity.type.value == "call":
        return "Send a follow-up email with meeting notes and next steps"
    elif last_activity.type.value == "email":
        return "Schedule a call to discuss the email content"
    
    return "Continue nurturing the relationship"


def suggest_upsell_opportunities(contact: Contact, deals: List[Deal]) -> List[str]:
    """Suggest upsell opportunities for a contact"""
    suggestions = []
    
    won_deals = [d for d in deals if d.stage.value == "closed_won"]
    
    if won_deals:
        total_value = sum(float(d.value) for d in won_deals)
        suggestions.append(f"Contact has purchased ${total_value:,.2f} worth of products - consider upselling")
    
    if contact.company:
        suggestions.append(f"Explore additional services for {contact.company.name}")
    
    return suggestions


def recommend_deal_strategy(deal: Deal) -> str:
    """Recommend strategy for a deal"""
    if deal.stage.value == "prospecting":
        return "Focus on understanding their pain points and building rapport"
    elif deal.stage.value == "qualification":
        return "Confirm budget, authority, need, and timeline (BANT)"
    elif deal.stage.value == "proposal":
        return "Present a compelling value proposition tailored to their needs"
    elif deal.stage.value == "negotiation":
        return "Be flexible on terms while protecting margins, focus on closing"
    
    return "Continue relationship building and provide value"

