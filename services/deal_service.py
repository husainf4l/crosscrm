from sqlalchemy.orm import Session
from sqlalchemy import func, and_
from typing import List, Optional, Dict
from datetime import date, datetime, timedelta
from db.models.deal import Deal, DealStage
from db.models.deal_history import DealHistory
from db.models.activity import Activity, ActivityType
from schema.deal import DealCreate, DealUpdate, DealClose
from decimal import Decimal


# Auto-calculate probability based on stage
STAGE_PROBABILITIES = {
    DealStage.PROSPECTING: 10,
    DealStage.QUALIFICATION: 25,
    DealStage.PROPOSAL: 50,
    DealStage.NEGOTIATION: 75,
    DealStage.CLOSED_WON: 100,
    DealStage.CLOSED_LOST: 0
}


def get_deals(
    db: Session,
    skip: int = 0,
    limit: int = 20,
    stage: Optional[DealStage] = None,
    assigned_to: Optional[int] = None,
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    min_value: Optional[Decimal] = None,
    max_value: Optional[Decimal] = None
) -> tuple[List[Deal], int]:
    """Get deals with pagination and filters"""
    query = db.query(Deal)
    
    if stage:
        query = query.filter(Deal.stage == stage)
    
    if assigned_to:
        query = query.filter(Deal.assigned_to == assigned_to)
    
    if start_date:
        query = query.filter(Deal.created_at >= start_date)
    
    if end_date:
        query = query.filter(Deal.created_at <= end_date)
    
    if min_value:
        query = query.filter(Deal.value >= min_value)
    
    if max_value:
        query = query.filter(Deal.value <= max_value)
    
    total = query.count()
    deals = query.order_by(Deal.created_at.desc()).offset(skip).limit(limit).all()
    
    return deals, total


def get_deal_by_id(db: Session, deal_id: int) -> Optional[Deal]:
    """Get deal by ID"""
    return db.query(Deal).filter(Deal.id == deal_id).first()


def create_deal(db: Session, deal_data: DealCreate, user_id: Optional[int] = None) -> Deal:
    """Create a new deal with initial history tracking"""
    deal_dict = deal_data.model_dump()
    if user_id and not deal_dict.get('assigned_to'):
        deal_dict['assigned_to'] = user_id
    
    # Auto-calculate probability if not provided or is 0
    initial_stage = deal_dict.get('stage', DealStage.PROSPECTING)
    if deal_dict.get('probability', 0) == 0:
        deal_dict['probability'] = STAGE_PROBABILITIES.get(initial_stage, 0)
    
    db_deal = Deal(**deal_dict)
    db.add(db_deal)
    db.flush()  # Flush to get the ID
    
    # Create initial history record
    initial_history = DealHistory(
        deal_id=db_deal.id,
        new_stage=initial_stage.value if isinstance(initial_stage, DealStage) else initial_stage,
        new_value=db_deal.value,
        new_probability=db_deal.probability,
        changed_by=user_id,
        change_reason="Deal created"
    )
    db.add(initial_history)
    
    db.commit()
    db.refresh(db_deal)
    return db_deal


def update_deal(db: Session, deal_id: int, deal_data: DealUpdate, changed_by: Optional[int] = None) -> Optional[Deal]:
    """Update a deal with history tracking"""
    db_deal = get_deal_by_id(db, deal_id)
    if not db_deal:
        return None
    
    update_data = deal_data.model_dump(exclude_unset=True)
    
    # Track changes for history
    history_data = {}
    if 'value' in update_data and db_deal.value != update_data['value']:
        history_data['old_value'] = db_deal.value
        history_data['new_value'] = update_data['value']
    
    if 'stage' in update_data and db_deal.stage != update_data['stage']:
        history_data['old_stage'] = db_deal.stage.value if db_deal.stage else None
        history_data['new_stage'] = update_data['stage'].value if isinstance(update_data['stage'], DealStage) else update_data['stage']
        # Auto-update probability when stage changes (if not explicitly set)
        if 'probability' not in update_data:
            update_data['probability'] = STAGE_PROBABILITIES.get(update_data['stage'], db_deal.probability)
            history_data['old_probability'] = db_deal.probability
            history_data['new_probability'] = update_data['probability']
    
    if 'probability' in update_data and db_deal.probability != update_data['probability']:
        # Only track if not already tracked by stage change
        if 'old_probability' not in history_data:
            history_data['old_probability'] = db_deal.probability
            history_data['new_probability'] = update_data['probability']
    
    # Save history if there are changes
    if history_data:
        history_data['deal_id'] = deal_id
        history_data['changed_by'] = changed_by
        history = DealHistory(**history_data)
        db.add(history)
        
        # Auto-create activity for stage changes
        if 'old_stage' in history_data and 'new_stage' in history_data:
            activity = Activity(
                type=ActivityType.NOTE,
                subject=f"Deal moved from {history_data['old_stage']} to {history_data['new_stage']}",
                description=f"Deal stage changed automatically",
                deal_id=deal_id,
                user_id=changed_by or db_deal.assigned_to,
                completed_at=datetime.now()
            )
            db.add(activity)
    
    for field, value in update_data.items():
        setattr(db_deal, field, value)
    
    db.commit()
    db.refresh(db_deal)
    return db_deal


def update_deal_stage(db: Session, deal_id: int, stage: DealStage, changed_by: Optional[int] = None) -> Optional[Deal]:
    """Update deal stage and auto-calculate probability with history tracking"""
    db_deal = get_deal_by_id(db, deal_id)
    if not db_deal:
        return None
    
    old_stage = db_deal.stage
    old_probability = db_deal.probability
    
    db_deal.stage = stage
    db_deal.probability = STAGE_PROBABILITIES.get(stage, db_deal.probability)
    
    # Track history
    if old_stage != stage:
        history = DealHistory(
            deal_id=deal_id,
            old_stage=old_stage.value if old_stage else None,
            new_stage=stage.value,
            old_probability=old_probability,
            new_probability=db_deal.probability,
            changed_by=changed_by
        )
        db.add(history)
        
        # Auto-create activity
        activity = Activity(
            type=ActivityType.NOTE,
            subject=f"Deal moved from {old_stage.value} to {stage.value}",
            description=f"Deal stage changed automatically",
            deal_id=deal_id,
            user_id=changed_by or db_deal.assigned_to,
            completed_at=datetime.now()
        )
        db.add(activity)
    
    db.commit()
    db.refresh(db_deal)
    return db_deal


def close_deal(db: Session, deal_id: int, deal_close: DealClose, changed_by: Optional[int] = None) -> Optional[Deal]:
    """Close a deal (won or lost) with history tracking"""
    db_deal = get_deal_by_id(db, deal_id)
    if not db_deal:
        return None
    
    old_stage = db_deal.stage
    old_probability = db_deal.probability
    new_stage = DealStage.CLOSED_WON if deal_close.won else DealStage.CLOSED_LOST
    new_probability = 100 if deal_close.won else 0
    
    # Track history
    history = DealHistory(
        deal_id=deal_id,
        old_stage=old_stage.value if old_stage else None,
        new_stage=new_stage.value,
        old_probability=old_probability,
        new_probability=new_probability,
        changed_by=changed_by,
        change_reason=f"Deal closed as {'WON' if deal_close.won else 'LOST'}"
    )
    db.add(history)
    
    # Auto-create activity
    activity = Activity(
        type=ActivityType.NOTE,
        subject=f"Deal closed as {'WON' if deal_close.won else 'LOST'}",
        description=f"Deal closed with value ${db_deal.value}",
        deal_id=deal_id,
        user_id=changed_by or db_deal.assigned_to,
        completed_at=datetime.now()
    )
    db.add(activity)
    
    db_deal.stage = new_stage
    db_deal.actual_close_date = deal_close.actual_close_date or date.today()
    db_deal.probability = new_probability
    
    db.commit()
    db.refresh(db_deal)
    return db_deal


def get_pipeline_summary(db: Session) -> dict:
    """Get pipeline statistics"""
    total_value = db.query(func.sum(Deal.value)).filter(
        Deal.stage.notin_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST])
    ).scalar() or Decimal('0')
    
    deal_counts = {}
    for stage in DealStage:
        count = db.query(Deal).filter(Deal.stage == stage).count()
        deal_counts[stage.value] = count
    
    return {
        "total_pipeline_value": float(total_value),
        "deal_counts": deal_counts,
        "total_deals": sum(deal_counts.values())
    }


def get_deals_by_stage(db: Session, stage: DealStage) -> List[Deal]:
    """Get deals by stage"""
    return db.query(Deal).filter(Deal.stage == stage).all()


def get_deal_aging_analysis(db: Session) -> Dict:
    """Analyze deal aging - identify stale deals"""
    now = datetime.now()
    aging_threshold = 30  # days
    
    active_deals = db.query(Deal).filter(
        Deal.stage.notin_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST])
    ).all()
    
    stale_deals = []
    aging_by_stage = {}
    
    for deal in active_deals:
        days_since_update = (now - (deal.updated_at or deal.created_at)).days
        
        if days_since_update > aging_threshold:
            stale_deals.append({
                "deal_id": deal.id,
                "title": deal.title,
                "stage": deal.stage.value,
                "days_stale": days_since_update,
                "assigned_to": deal.assigned_to
            })
        
        stage = deal.stage.value
        if stage not in aging_by_stage:
            aging_by_stage[stage] = {"count": 0, "avg_days": 0, "total_days": 0}
        
        aging_by_stage[stage]["count"] += 1
        aging_by_stage[stage]["total_days"] += days_since_update
    
    # Calculate averages
    for stage in aging_by_stage:
        if aging_by_stage[stage]["count"] > 0:
            aging_by_stage[stage]["avg_days"] = aging_by_stage[stage]["total_days"] / aging_by_stage[stage]["count"]
    
    return {
        "stale_deals": stale_deals,
        "aging_by_stage": aging_by_stage,
        "total_stale": len(stale_deals)
    }


def get_deal_velocity(db: Session, deal_id: int) -> Dict:
    """Calculate deal velocity - time spent in each stage"""
    deal = get_deal_by_id(db, deal_id)
    if not deal:
        return {}
    
    # Get stage change history
    history = db.query(DealHistory).filter(
        DealHistory.deal_id == deal_id,
        DealHistory.new_stage.isnot(None)
    ).order_by(DealHistory.created_at).all()
    
    velocity = {}
    current_stage_start = deal.created_at
    
    for change in history:
        if change.old_stage:
            stage_duration = (change.created_at - current_stage_start).days
            if change.old_stage not in velocity:
                velocity[change.old_stage] = []
            velocity[change.old_stage].append(stage_duration)
        current_stage_start = change.created_at
    
    # Calculate average time per stage
    avg_velocity = {}
    for stage, durations in velocity.items():
        avg_velocity[stage] = sum(durations) / len(durations) if durations else 0
    
    # Current stage duration
    current_duration = (datetime.now() - current_stage_start).days
    if deal.stage.value not in avg_velocity:
        avg_velocity[deal.stage.value] = current_duration
    
    return {
        "deal_id": deal_id,
        "current_stage": deal.stage.value,
        "days_in_current_stage": current_duration,
        "average_days_per_stage": avg_velocity,
        "total_days_open": (datetime.now() - deal.created_at).days
    }


def get_pipeline_health(db: Session) -> Dict:
    """Analyze pipeline health - conversion rates, bottlenecks"""
    # Get current stage counts
    stage_counts = {}
    for stage in DealStage:
        count = db.query(Deal).filter(Deal.stage == stage).count()
        stage_counts[stage.value] = count
    
    # Calculate actual conversion rates from history
    stages_order = ['prospecting', 'qualification', 'proposal', 'negotiation', 'closed_won']
    stage_conversions = {}
    
    for i in range(len(stages_order) - 1):
        current_stage = stages_order[i]
        next_stage = stages_order[i + 1]
        
        # Count actual transitions from history
        transitions = db.query(DealHistory).filter(
            DealHistory.old_stage == current_stage,
            DealHistory.new_stage == next_stage
        ).count()
        
        # Count deals that have been in current stage (current + moved out)
        deals_in_stage = db.query(DealHistory).filter(
            DealHistory.old_stage == current_stage
        ).distinct(DealHistory.deal_id).count()
        
        # Also count current deals in this stage
        current_in_stage = stage_counts.get(current_stage, 0)
        total_deals_in_stage = max(deals_in_stage, current_in_stage)
        
        # Calculate conversion rate
        conversion_rate = (transitions / total_deals_in_stage * 100) if total_deals_in_stage > 0 else 0
        
        stage_conversions[f"{current_stage}_to_{next_stage}"] = {
            "from_count": total_deals_in_stage,
            "transitions": transitions,
            "conversion_rate": conversion_rate
        }
    
    # Identify bottlenecks (stages with low conversion rates)
    bottlenecks = []
    for conversion, data in stage_conversions.items():
        if data["conversion_rate"] < 30 and data.get("from_count", 0) > 0:  # Less than 30% conversion
            bottlenecks.append({
                "stage": conversion,
                "conversion_rate": data["conversion_rate"],
                "deals_stuck": data.get("from_count", 0),
                "transitions": data.get("transitions", 0)
            })
    
    # Calculate weighted pipeline value
    active_deals = db.query(Deal).filter(
        Deal.stage.notin_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST])
    ).all()
    weighted_value = sum(float(d.value * d.probability / 100) for d in active_deals)
    total_value = sum(float(d.value) for d in active_deals)
    
    return {
        "stage_counts": stage_counts,
        "conversion_rates": stage_conversions,
        "bottlenecks": bottlenecks,
        "weighted_pipeline_value": weighted_value,
        "total_pipeline_value": total_value,
        "health_score": max(0, 100 - (len(bottlenecks) * 10))  # Simple health score, ensure non-negative
    }


def get_at_risk_deals(db: Session) -> List[Dict]:
    """Identify deals at risk of being lost - optimized version"""
    now = datetime.now()
    risk_deals = []
    
    active_deals = db.query(Deal).filter(
        Deal.stage.notin_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST])
    ).all()
    
    # Batch get stage change dates for velocity calculation
    deal_ids = [d.id for d in active_deals]
    last_stage_changes = {}
    if deal_ids:
        # Get most recent stage change for each deal
        from sqlalchemy import desc
        recent_changes = db.query(DealHistory).filter(
            DealHistory.deal_id.in_(deal_ids),
            DealHistory.new_stage.isnot(None)
        ).order_by(DealHistory.deal_id, desc(DealHistory.created_at)).all()
        
        for change in recent_changes:
            if change.deal_id not in last_stage_changes:
                last_stage_changes[change.deal_id] = change.created_at
    
    for deal in active_deals:
        risk_factors = []
        risk_score = 0
        
        # Check if expected close date has passed
        if deal.expected_close_date and deal.expected_close_date < now.date():
            days_overdue = (now.date() - deal.expected_close_date).days
            risk_factors.append(f"Expected close date passed {days_overdue} days ago")
            risk_score += min(30 + (days_overdue // 7) * 5, 50)  # Cap at 50
        
        # Check if deal is stale (no updates)
        days_since_update = (now - (deal.updated_at or deal.created_at)).days
        if days_since_update > 30:
            risk_factors.append(f"No updates in {days_since_update} days")
            risk_score += min(20 + (days_since_update - 30) // 7 * 2, 40)  # Cap at 40
        
        # Check if probability is low but in late stage
        if deal.stage in [DealStage.PROPOSAL, DealStage.NEGOTIATION] and deal.probability < 50:
            risk_factors.append("Low probability in late stage")
            risk_score += 25
        
        # Check if deal has been in same stage too long (optimized)
        stage_start = last_stage_changes.get(deal.id, deal.created_at)
        days_in_stage = (now - stage_start).days
        if days_in_stage > 60:
            risk_factors.append(f"In {deal.stage.value} stage for {days_in_stage} days")
            risk_score += min(15 + (days_in_stage - 60) // 7, 30)  # Cap at 30
        
        # Cap total risk score at 100
        risk_score = min(risk_score, 100)
        
        if risk_score > 30:  # Threshold for "at risk"
            risk_deals.append({
                "deal_id": deal.id,
                "title": deal.title,
                "stage": deal.stage.value,
                "value": float(deal.value),
                "probability": deal.probability,
                "risk_score": risk_score,
                "risk_factors": risk_factors,
                "assigned_to": deal.assigned_to
            })
    
    return sorted(risk_deals, key=lambda x: x["risk_score"], reverse=True)


def get_weighted_forecast(db: Session, forecast_days: int = 30) -> Dict:
    """Calculate weighted sales forecast based on probability and expected close dates"""
    now = datetime.now()
    forecast_end = now + timedelta(days=forecast_days)
    
    active_deals = db.query(Deal).filter(
        Deal.stage.notin_([DealStage.CLOSED_WON, DealStage.CLOSED_LOST]),
        Deal.expected_close_date.isnot(None),
        Deal.expected_close_date <= forecast_end.date()
    ).all()
    
    forecasted_revenue = 0
    forecasted_deals = []
    
    for deal in active_deals:
        # Weight by probability
        weighted_value = float(deal.value) * (deal.probability / 100)
        forecasted_revenue += weighted_value
        
        forecasted_deals.append({
            "deal_id": deal.id,
            "title": deal.title,
            "value": float(deal.value),
            "weighted_value": weighted_value,
            "probability": deal.probability,
            "expected_close_date": deal.expected_close_date.isoformat(),
            "stage": deal.stage.value
        })
    
    return {
        "forecast_period_days": forecast_days,
        "forecast_end_date": forecast_end.date().isoformat(),
        "forecasted_revenue": forecasted_revenue,
        "forecasted_deals_count": len(forecasted_deals),
        "deals": forecasted_deals
    }



def delete_deal(db: Session, deal_id: int) -> bool:
    """Delete a deal"""
    db_deal = get_deal_by_id(db, deal_id)
    if not db_deal:
        return False
    
    db.delete(db_deal)
    db.commit()
    return True
