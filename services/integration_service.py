"""Integration service for accounting system"""
import csv
from typing import List, Dict
from sqlalchemy.orm import Session
from datetime import datetime
from db.models.deal import Deal, DealStage
from db.models.contact import Contact
from db.models.company import Company
from decimal import Decimal
from sqlalchemy import func


def import_sales_history(file_path: str, db: Session) -> Dict:
    """Import sales history from CSV file"""
    imported = 0
    errors = []
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            reader = csv.DictReader(f)
            for row in reader:
                try:
                    # Parse CSV row (adjust column names as needed)
                    contact_name = row.get('contact_name', '').split()
                    first_name = contact_name[0] if contact_name else ''
                    last_name = ' '.join(contact_name[1:]) if len(contact_name) > 1 else ''
                    
                    # Find or create contact
                    contact = db.query(Contact).filter(
                        Contact.first_name == first_name,
                        Contact.last_name == last_name
                    ).first()
                    
                    if not contact and first_name:
                        contact = Contact(
                            first_name=first_name,
                            last_name=last_name,
                            email=row.get('email', ''),
                            created_by=1  # Default user
                        )
                        db.add(contact)
                        db.flush()
                    
                    # Create deal
                    deal = Deal(
                        title=row.get('title', 'Imported Deal'),
                        description=row.get('description', ''),
                        value=Decimal(row.get('value', '0')),
                        currency=row.get('currency', 'USD'),
                        stage=DealStage.CLOSED_WON,
                        probability=100,
                        contact_id=contact.id if contact else None,
                        assigned_to=1,  # Default user
                        actual_close_date=datetime.strptime(row.get('date', ''), '%Y-%m-%d').date() if row.get('date') else None
                    )
                    db.add(deal)
                    imported += 1
                except Exception as e:
                    errors.append(f"Row error: {str(e)}")
        
        db.commit()
        return {"imported": imported, "errors": errors}
    except Exception as e:
        db.rollback()
        return {"imported": 0, "errors": [str(e)]}


def sync_customer_data(db: Session) -> Dict:
    """Sync customer data from accounting system"""
    # This would typically connect to an external API
    # For now, return a placeholder
    return {"synced": 0, "message": "Integration not configured"}


def reconcile_revenue(db: Session, start_date: datetime, end_date: datetime) -> Dict:
    """Reconcile revenue between CRM and accounting system"""
    # Get CRM revenue
    crm_revenue = db.query(Deal).filter(
        Deal.stage == DealStage.CLOSED_WON,
        Deal.actual_close_date >= start_date.date(),
        Deal.actual_close_date <= end_date.date()
    ).with_entities(func.sum(Deal.value)).scalar() or Decimal('0')
    
    # In production, fetch from accounting system API
    accounting_revenue = Decimal('0')  # Placeholder
    
    difference = crm_revenue - accounting_revenue
    
    return {
        "crm_revenue": float(crm_revenue),
        "accounting_revenue": float(accounting_revenue),
        "difference": float(difference),
        "reconciled": abs(difference) < 0.01
    }

