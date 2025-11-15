from fastapi import APIRouter, Request, Depends, Form, Query
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.deal import DealStage
from services.deal_service import (
    get_deals, get_deal_by_id, create_deal, update_deal,
    update_deal_stage, close_deal, get_pipeline_summary,
    get_deals_by_stage, delete_deal
)
from schema.deal import DealCreate, DealUpdate, DealClose
from typing import Optional
from decimal import Decimal
from db.models.user import User

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request) -> Optional[int]:
    return request.session.get("user_id")


@router.get("/", response_class=HTMLResponse)
async def list_deals(
    request: Request,
    page: int = Query(1, ge=1),
    stage: Optional[str] = Query(None),
    db: Session = Depends(get_db)
):
    """List all deals"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    deal_stage = DealStage(stage) if stage else None
    deals, total = get_deals(db, skip=(page-1)*20, limit=20, stage=deal_stage)
    
    return templates.TemplateResponse(
        "deals/list.html",
        {"request": request, "deals": deals, "page": page, "total": total, "stage": stage}
    )


@router.get("/pipeline", response_class=HTMLResponse)
async def pipeline_view(
    request: Request,
    assigned_to: Optional[int] = Query(None),
    min_value: Optional[str] = Query(None),
    db: Session = Depends(get_db)
):
    """Pipeline kanban view with user management"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from decimal import Decimal
    from services.deal_service import get_deals
    
    min_val = None
    if min_value:
        try:
            min_val = Decimal(min_value)
        except:
            pass
    
    pipeline_data = {}
    for stage in DealStage:
        if stage.value not in ['closed_won', 'closed_lost']:
            deals = get_deals_by_stage(db, stage)
            # Apply filters
            if assigned_to:
                deals = [d for d in deals if d.assigned_to == assigned_to]
            if min_val:
                deals = [d for d in deals if d.value >= min_val]
            pipeline_data[stage.value] = deals
    
    summary = get_pipeline_summary(db)
    
    # Get all users/salespeople for management
    users = db.query(User).filter(User.role.in_(["salesperson", "manager", "admin"])).all()
    
    # Get user performance stats
    from services.analytics_service import get_performance_by_salesperson
    performance = get_performance_by_salesperson(db)
    perf_dict = {p["user_id"]: p for p in performance}
    
    return templates.TemplateResponse(
        "deals/pipeline.html",
        {
            "request": request,
            "pipeline_data": pipeline_data,
            "summary": summary,
            "stages": DealStage,
            "users": users,
            "selected_user": assigned_to,
            "min_value": min_value,
            "performance": perf_dict
        }
    )


@router.get("/{deal_id}", response_class=HTMLResponse)
async def deal_detail(request: Request, deal_id: int, db: Session = Depends(get_db)):
    """Deal detail page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    deal = get_deal_by_id(db, deal_id)
    if not deal:
        return RedirectResponse(url="/deals?error=Deal not found", status_code=303)
    
    from services.activity_service import get_activities
    activities, _ = get_activities(db, skip=0, limit=10, deal_id=deal_id)
    
    return templates.TemplateResponse(
        "deals/detail.html",
        {"request": request, "deal": deal, "activities": activities}
    )


@router.get("/new", response_class=HTMLResponse)
async def new_deal_form(request: Request, db: Session = Depends(get_db), template: Optional[str] = None):
    """Create deal form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from services.contact_service import get_contacts
    from services.company_service import get_companies
    from services.deal_templates_service import get_deal_templates
    
    contacts, _ = get_contacts(db, skip=0, limit=1000)
    companies, _ = get_companies(db, skip=0, limit=1000)
    users = db.query(User).all()
    deal_templates = get_deal_templates()
    
    template_data = None
    if template and template in deal_templates:
        template_data = deal_templates[template]
    
    return templates.TemplateResponse(
        "deals/form.html",
        {
            "request": request,
            "deal": None,
            "contacts": contacts,
            "companies": companies,
            "users": users,
            "stages": DealStage,
            "templates": deal_templates,
            "template_data": template_data,
            "selected_template": template
        }
    )


@router.post("/{deal_id}/clone")
async def clone_deal_handler(
    request: Request,
    deal_id: int,
    db: Session = Depends(get_db)
):
    """Clone deal handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from services.deal_templates_service import clone_deal
    
    try:
        new_deal = clone_deal(db, deal_id)
        return RedirectResponse(url=f"/deals/{new_deal.id}?success=Deal cloned", status_code=303)
    except Exception as e:
        return RedirectResponse(url=f"/deals/{deal_id}?error={str(e)}", status_code=303)


@router.post("/")
async def create_deal_handler(
    request: Request,
    title: str = Form(...),
    description: Optional[str] = Form(None),
    value: str = Form(...),
    currency: str = Form("USD"),
    stage: str = Form(...),
    probability: int = Form(0),
    contact_id: Optional[int] = Form(None),
    company_id: Optional[int] = Form(None),
    assigned_to: Optional[int] = Form(None),
    db: Session = Depends(get_db)
):
    """Create deal handler"""
    user_id = get_current_user_id(request)
    if not user_id:
        return RedirectResponse(url="/auth/login", status_code=303)
    
    deal_data = DealCreate(
        title=title,
        description=description,
        value=Decimal(value),
        currency=currency,
        stage=DealStage(stage),
        probability=probability,
        contact_id=contact_id,
        company_id=company_id,
        assigned_to=assigned_to or user_id
    )
    
    deal = create_deal(db, deal_data, user_id)
    return RedirectResponse(url=f"/deals/{deal.id}?success=Deal created", status_code=303)


@router.post("/{deal_id}/stage")
async def update_stage_handler(
    request: Request,
    deal_id: int,
    stage: str = Form(...),
    db: Session = Depends(get_db)
):
    """Update deal stage"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    deal = update_deal_stage(db, deal_id, DealStage(stage))
    if not deal:
        return RedirectResponse(url="/deals?error=Deal not found", status_code=303)
    
    return RedirectResponse(url=f"/deals/{deal.id}?success=Stage updated", status_code=303)


@router.post("/{deal_id}/close")
async def close_deal_handler(
    request: Request,
    deal_id: int,
    won: bool = Form(...),
    db: Session = Depends(get_db)
):
    """Close deal handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    deal_close = DealClose(won=won)
    deal = close_deal(db, deal_id, deal_close)
    if not deal:
        return RedirectResponse(url="/deals?error=Deal not found", status_code=303)
    
    return RedirectResponse(url=f"/deals/{deal.id}?success=Deal closed", status_code=303)


@router.post("/{deal_id}/delete")
async def delete_deal_handler(
    request: Request,
    deal_id: int,
    db: Session = Depends(get_db)
):
    """Delete deal handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    success = delete_deal(db, deal_id)
    if not success:
        return RedirectResponse(url="/deals?error=Deal not found", status_code=303)
    
    return RedirectResponse(url="/deals?success=Deal deleted", status_code=303)

