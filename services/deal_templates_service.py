"""Deal templates service"""
from sqlalchemy.orm import Session
from typing import List, Optional
from db.models.deal import Deal, DealStage
from schema.deal import DealCreate
from decimal import Decimal


DEAL_TEMPLATES = {
    "enterprise_license": {
        "title": "Enterprise License",
        "description": "Annual enterprise software license",
        "value": Decimal("50000"),
        "stage": DealStage.QUALIFICATION,
        "probability": 50
    },
    "support_contract": {
        "title": "Annual Support Contract",
        "description": "12-month technical support and maintenance",
        "value": Decimal("25000"),
        "stage": DealStage.PROPOSAL,
        "probability": 60
    },
    "custom_development": {
        "title": "Custom Development Project",
        "description": "Custom software development project",
        "value": Decimal("75000"),
        "stage": DealStage.PROSPECTING,
        "probability": 40
    },
    "consulting": {
        "title": "Consulting Services",
        "description": "Professional consulting engagement",
        "value": Decimal("15000"),
        "stage": DealStage.QUALIFICATION,
        "probability": 55
    }
}


def get_deal_templates() -> dict:
    """Get available deal templates"""
    return DEAL_TEMPLATES


def create_deal_from_template(
    db: Session,
    template_name: str,
    contact_id: Optional[int] = None,
    company_id: Optional[int] = None,
    assigned_to: Optional[int] = None
) -> Deal:
    """Create a deal from a template"""
    if template_name not in DEAL_TEMPLATES:
        raise ValueError(f"Template '{template_name}' not found")
    
    template = DEAL_TEMPLATES[template_name]
    deal_data = DealCreate(
        title=template["title"],
        description=template["description"],
        value=template["value"],
        stage=template["stage"],
        probability=template["probability"],
        contact_id=contact_id,
        company_id=company_id,
        assigned_to=assigned_to
    )
    
    from services.deal_service import create_deal
    return create_deal(db, deal_data, assigned_to)


def clone_deal(db: Session, deal_id: int, new_title: Optional[str] = None) -> Deal:
    """Clone an existing deal"""
    from services.deal_service import get_deal_by_id
    
    original_deal = get_deal_by_id(db, deal_id)
    if not original_deal:
        raise ValueError("Deal not found")
    
    deal_data = DealCreate(
        title=new_title or f"{original_deal.title} (Copy)",
        description=original_deal.description,
        value=original_deal.value,
        currency=original_deal.currency,
        stage=DealStage.PROSPECTING,  # Reset to initial stage
        probability=0,  # Reset probability
        contact_id=original_deal.contact_id,
        company_id=original_deal.company_id,
        assigned_to=original_deal.assigned_to
    )
    
    from services.deal_service import create_deal
    return create_deal(db, deal_data, original_deal.assigned_to)

