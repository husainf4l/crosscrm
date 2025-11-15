from fastapi import APIRouter, Request, Depends, Form, Query
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from services.company_service import (
    get_companies, get_company_by_id, create_company,
    update_company, delete_company
)
from schema.company import CompanyCreate, CompanyUpdate
from typing import Optional
from decimal import Decimal

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request) -> Optional[int]:
    """Get current user ID from session"""
    return request.session.get("user_id")


@router.get("/", response_class=HTMLResponse)
async def list_companies(
    request: Request,
    page: int = Query(1, ge=1),
    page_size: int = Query(20, ge=1, le=100),
    search: Optional[str] = Query(None),
    db: Session = Depends(get_db)
):
    """List all companies"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    skip = (page - 1) * page_size
    companies, total = get_companies(db, skip=skip, limit=page_size, search=search)
    
    total_pages = (total + page_size - 1) // page_size
    
    return templates.TemplateResponse(
        "companies/list.html",
        {
            "request": request,
            "companies": companies,
            "page": page,
            "page_size": page_size,
            "total": total,
            "total_pages": total_pages,
            "search": search
        }
    )


@router.get("/{company_id}", response_class=HTMLResponse)
async def company_detail(
    request: Request,
    company_id: int,
    db: Session = Depends(get_db)
):
    """Company detail page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    company = get_company_by_id(db, company_id)
    if not company:
        return RedirectResponse(url="/companies?error=Company not found", status_code=303)
    
    # Get related contacts and deals
    from services.contact_service import get_contacts
    from services.deal_service import get_deals
    
    contacts, _ = get_contacts(db, skip=0, limit=100, company_id=company_id)
    deals, _ = get_deals(db, skip=0, limit=100)
    company_deals = [d for d in deals if d.company_id == company_id]
    
    return templates.TemplateResponse(
        "companies/detail.html",
        {
            "request": request,
            "company": company,
            "contacts": contacts,
            "deals": company_deals
        }
    )


@router.get("/new", response_class=HTMLResponse)
async def new_company_form(request: Request):
    """Create company form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    return templates.TemplateResponse(
        "companies/form.html",
        {"request": request, "company": None}
    )


@router.post("/")
async def create_company_handler(
    request: Request,
    name: str = Form(...),
    industry: Optional[str] = Form(None),
    website: Optional[str] = Form(None),
    address: Optional[str] = Form(None),
    employee_count: Optional[int] = Form(None),
    annual_revenue: Optional[str] = Form(None),
    db: Session = Depends(get_db)
):
    """Create company handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    revenue = None
    if annual_revenue:
        try:
            revenue = Decimal(annual_revenue)
        except:
            pass
    
    company_data = CompanyCreate(
        name=name,
        industry=industry,
        website=website,
        address=address,
        employee_count=employee_count,
        annual_revenue=revenue
    )
    
    company = create_company(db, company_data)
    return RedirectResponse(url=f"/companies/{company.id}?success=Company created", status_code=303)


@router.get("/{company_id}/edit", response_class=HTMLResponse)
async def edit_company_form(
    request: Request,
    company_id: int,
    db: Session = Depends(get_db)
):
    """Edit company form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    company = get_company_by_id(db, company_id)
    if not company:
        return RedirectResponse(url="/companies?error=Company not found", status_code=303)
    
    return templates.TemplateResponse(
        "companies/form.html",
        {"request": request, "company": company}
    )


@router.post("/{company_id}")
async def update_company_handler(
    request: Request,
    company_id: int,
    name: str = Form(...),
    industry: Optional[str] = Form(None),
    website: Optional[str] = Form(None),
    address: Optional[str] = Form(None),
    employee_count: Optional[int] = Form(None),
    annual_revenue: Optional[str] = Form(None),
    db: Session = Depends(get_db)
):
    """Update company handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    revenue = None
    if annual_revenue:
        try:
            revenue = Decimal(annual_revenue)
        except:
            pass
    
    company_data = CompanyUpdate(
        name=name,
        industry=industry,
        website=website,
        address=address,
        employee_count=employee_count,
        annual_revenue=revenue
    )
    
    company = update_company(db, company_id, company_data)
    if not company:
        return RedirectResponse(url="/companies?error=Company not found", status_code=303)
    
    return RedirectResponse(url=f"/companies/{company.id}?success=Company updated", status_code=303)


@router.post("/{company_id}/delete")
async def delete_company_handler(
    request: Request,
    company_id: int,
    db: Session = Depends(get_db)
):
    """Delete company handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    success = delete_company(db, company_id)
    if not success:
        return RedirectResponse(url="/companies?error=Company not found", status_code=303)
    
    return RedirectResponse(url="/companies?success=Company deleted", status_code=303)

