from sqlalchemy.orm import Session
from typing import List, Optional
from db.models.activity import Activity
from schema.activity import ActivityCreate, ActivityUpdate


def get_activities(
    db: Session,
    skip: int = 0,
    limit: int = 50,
    contact_id: Optional[int] = None,
    deal_id: Optional[int] = None,
    user_id: Optional[int] = None
) -> tuple[List[Activity], int]:
    """Get activities with pagination and filters"""
    query = db.query(Activity)
    
    if contact_id:
        query = query.filter(Activity.contact_id == contact_id)
    
    if deal_id:
        query = query.filter(Activity.deal_id == deal_id)
    
    if user_id:
        query = query.filter(Activity.user_id == user_id)
    
    total = query.count()
    activities = query.order_by(Activity.created_at.desc()).offset(skip).limit(limit).all()
    
    return activities, total


def get_activity_by_id(db: Session, activity_id: int) -> Optional[Activity]:
    """Get activity by ID"""
    return db.query(Activity).filter(Activity.id == activity_id).first()


def create_activity(db: Session, activity_data: ActivityCreate, user_id: int) -> Activity:
    """Create a new activity"""
    db_activity = Activity(
        **activity_data.model_dump(),
        user_id=user_id
    )
    db.add(db_activity)
    db.commit()
    db.refresh(db_activity)
    return db_activity


def update_activity(db: Session, activity_id: int, activity_data: ActivityUpdate) -> Optional[Activity]:
    """Update an activity"""
    db_activity = get_activity_by_id(db, activity_id)
    if not db_activity:
        return None
    
    update_data = activity_data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(db_activity, field, value)
    
    db.commit()
    db.refresh(db_activity)
    return db_activity


def delete_activity(db: Session, activity_id: int) -> bool:
    """Delete an activity"""
    db_activity = get_activity_by_id(db, activity_id)
    if not db_activity:
        return False
    
    db.delete(db_activity)
    db.commit()
    return True

