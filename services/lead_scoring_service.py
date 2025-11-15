"""Lead Scoring Service - Calculate lead scores based on various factors"""
from sqlalchemy.orm import Session
from typing import Dict
from db.models.contact import Contact
from db.models.deal import Deal, DealStage
from db.models.activity import Activity, ActivityType
from datetime import datetime, timedelta


def calculate_lead_score(contact: Contact, db: Session) -> int:
    """Calculate lead score based on multiple factors"""
    score = 0
    
    # Base score factors
    if contact.email:
        score += 10
    if contact.phone:
        score += 10
    if contact.job_title:
        score += 5
    if contact.company_id:
        score += 15
    
    # Activity-based scoring
    activities = db.query(Activity).filter(Activity.contact_id == contact.id).all()
    if activities:
        score += min(len(activities) * 5, 25)  # Max 25 points for activities
        
        # Recent activity bonus
        recent_activities = [a for a in activities if a.created_at > datetime.now() - timedelta(days=30)]
        if recent_activities:
            score += 10
    
    # Deal-based scoring
    deals = db.query(Deal).filter(Deal.contact_id == contact.id).all()
    if deals:
        score += min(len(deals) * 10, 30)  # Max 30 points for deals
        
        # Active deals bonus
        active_deals = [d for d in deals if d.stage not in [DealStage.CLOSED_WON, DealStage.CLOSED_LOST]]
        if active_deals:
            score += 15
        
        # Won deals bonus
        won_deals = [d for d in deals if d.stage == DealStage.CLOSED_WON]
        if won_deals:
            score += 20
    
    # Lead source scoring
    if contact.lead_source:
        source_scores = {
            "referral": 20,
            "website": 10,
            "social_media": 5,
            "cold_call": 5,
            "event": 15,
            "partner": 15
        }
        score += source_scores.get(contact.lead_source.lower(), 0)
    
    # Lifecycle stage scoring
    if contact.lifecycle_stage:
        stage_scores = {
            "lead": 0,
            "qualified": 20,
            "customer": 30,
            "champion": 40
        }
        score += stage_scores.get(contact.lifecycle_stage.lower(), 0)
    
    return min(score, 100)  # Cap at 100


def update_lead_score(contact_id: int, db: Session) -> int:
    """Update and save lead score for a contact"""
    contact = db.query(Contact).filter(Contact.id == contact_id).first()
    if not contact:
        return 0
    
    score = calculate_lead_score(contact, db)
    contact.lead_score = score
    db.commit()
    return score


def get_lead_scoring_factors(contact: Contact, db: Session) -> Dict:
    """Get detailed breakdown of lead scoring factors"""
    factors = {
        "base_score": 0,
        "activity_score": 0,
        "deal_score": 0,
        "source_score": 0,
        "stage_score": 0,
        "total_score": 0
    }
    
    # Base factors
    if contact.email:
        factors["base_score"] += 10
    if contact.phone:
        factors["base_score"] += 10
    if contact.job_title:
        factors["base_score"] += 5
    if contact.company_id:
        factors["base_score"] += 15
    
    # Activity factors
    activities = db.query(Activity).filter(Activity.contact_id == contact.id).all()
    if activities:
        factors["activity_score"] = min(len(activities) * 5, 25)
        recent_activities = [a for a in activities if a.created_at > datetime.now() - timedelta(days=30)]
        if recent_activities:
            factors["activity_score"] += 10
    
    # Deal factors
    deals = db.query(Deal).filter(Deal.contact_id == contact.id).all()
    if deals:
        factors["deal_score"] = min(len(deals) * 10, 30)
        active_deals = [d for d in deals if d.stage not in [DealStage.CLOSED_WON, DealStage.CLOSED_LOST]]
        if active_deals:
            factors["deal_score"] += 15
        won_deals = [d for d in deals if d.stage == DealStage.CLOSED_WON]
        if won_deals:
            factors["deal_score"] += 20
    
    # Source factors
    if contact.lead_source:
        source_scores = {
            "referral": 20,
            "website": 10,
            "social_media": 5,
            "cold_call": 5,
            "event": 15,
            "partner": 15
        }
        factors["source_score"] = source_scores.get(contact.lead_source.lower(), 0)
    
    # Stage factors
    if contact.lifecycle_stage:
        stage_scores = {
            "lead": 0,
            "qualified": 20,
            "customer": 30,
            "champion": 40
        }
        factors["stage_score"] = stage_scores.get(contact.lifecycle_stage.lower(), 0)
    
    factors["total_score"] = min(
        factors["base_score"] + 
        factors["activity_score"] + 
        factors["deal_score"] + 
        factors["source_score"] + 
        factors["stage_score"],
        100
    )
    
    return factors

