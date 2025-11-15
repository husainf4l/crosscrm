from sqlalchemy.orm import Session
from sqlalchemy import or_
from typing import List, Optional
from db.models.company import Company
from schema.company import CompanyCreate, CompanyUpdate


def get_companies(
    db: Session,
    skip: int = 0,
    limit: int = 20,
    search: Optional[str] = None
) -> tuple[List[Company], int]:
    """Get companies with pagination and filters"""
    query = db.query(Company)
    
    if search:
        query = query.filter(
            or_(
                Company.name.ilike(f"%{search}%"),
                Company.industry.ilike(f"%{search}%")
            )
        )
    
    total = query.count()
    companies = query.offset(skip).limit(limit).all()
    
    return companies, total


def get_company_by_id(db: Session, company_id: int) -> Optional[Company]:
    """Get company by ID"""
    return db.query(Company).filter(Company.id == company_id).first()


def create_company(db: Session, company_data: CompanyCreate) -> Company:
    """Create a new company"""
    db_company = Company(**company_data.model_dump())
    db.add(db_company)
    db.commit()
    db.refresh(db_company)
    return db_company


def update_company(db: Session, company_id: int, company_data: CompanyUpdate) -> Optional[Company]:
    """Update a company"""
    db_company = get_company_by_id(db, company_id)
    if not db_company:
        return None
    
    update_data = company_data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(db_company, field, value)
    
    db.commit()
    db.refresh(db_company)
    return db_company


def delete_company(db: Session, company_id: int) -> bool:
    """Delete a company"""
    db_company = get_company_by_id(db, company_id)
    if not db_company:
        return False
    
    db.delete(db_company)
    db.commit()
    return True

