from sqlalchemy.orm import Session
from sqlalchemy import or_
from typing import List, Optional
from db.models.contact import Contact
from schema.contact import ContactCreate, ContactUpdate
from schema.common import PaginationParams


def get_contacts(
    db: Session,
    skip: int = 0,
    limit: int = 20,
    search: Optional[str] = None,
    company_id: Optional[int] = None
) -> tuple[List[Contact], int]:
    """Get contacts with pagination and filters"""
    query = db.query(Contact)
    
    if search:
        query = query.filter(
            or_(
                Contact.first_name.ilike(f"%{search}%"),
                Contact.last_name.ilike(f"%{search}%"),
                Contact.email.ilike(f"%{search}%")
            )
        )
    
    if company_id:
        query = query.filter(Contact.company_id == company_id)
    
    total = query.count()
    contacts = query.offset(skip).limit(limit).all()
    
    return contacts, total


def get_contact_by_id(db: Session, contact_id: int) -> Optional[Contact]:
    """Get contact by ID"""
    return db.query(Contact).filter(Contact.id == contact_id).first()


def create_contact(db: Session, contact_data: ContactCreate, created_by: int) -> Contact:
    """Create a new contact"""
    db_contact = Contact(
        **contact_data.model_dump(),
        created_by=created_by
    )
    db.add(db_contact)
    db.commit()
    db.refresh(db_contact)
    return db_contact


def update_contact(db: Session, contact_id: int, contact_data: ContactUpdate) -> Optional[Contact]:
    """Update a contact"""
    db_contact = get_contact_by_id(db, contact_id)
    if not db_contact:
        return None
    
    update_data = contact_data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(db_contact, field, value)
    
    db.commit()
    db.refresh(db_contact)
    return db_contact


def delete_contact(db: Session, contact_id: int) -> bool:
    """Delete a contact"""
    db_contact = get_contact_by_id(db, contact_id)
    if not db_contact:
        return False
    
    db.delete(db_contact)
    db.commit()
    return True


def search_contacts(db: Session, query: str, limit: int = 10) -> List[Contact]:
    """Search contacts"""
    return db.query(Contact).filter(
        or_(
            Contact.first_name.ilike(f"%{query}%"),
            Contact.last_name.ilike(f"%{query}%"),
            Contact.email.ilike(f"%{query}%")
        )
    ).limit(limit).all()

