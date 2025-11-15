from fastapi import APIRouter, Request, Depends, Form, Query
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.activity import ActivityType
from services.activity_service import (
    get_activities, get_activity_by_id, create_activity,
    update_activity, delete_activity
)
from schema.activity import ActivityCreate
from typing import Optional
from datetime import datetime

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request) -> Optional[int]:
    return request.session.get("user_id")


@router.get("/", response_class=HTMLResponse)
async def list_activities(
    request: Request,
    page: int = Query(1, ge=1),
    contact_id: Optional[int] = Query(None),
    deal_id: Optional[int] = Query(None),
    db: Session = Depends(get_db)
):
    """List all activities"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    user_id = get_current_user_id(request)
    activities, total = get_activities(
        db, skip=(page-1)*50, limit=50,
        contact_id=contact_id, deal_id=deal_id, user_id=user_id
    )
    
    return templates.TemplateResponse(
        "activities/list.html",
        {
            "request": request,
            "activities": activities,
            "page": page,
            "total": total,
            "contact_id": contact_id,
            "deal_id": deal_id
        }
    )


@router.get("/new", response_class=HTMLResponse)
async def new_activity_form(
    request: Request,
    contact_id: Optional[int] = Query(None),
    deal_id: Optional[int] = Query(None),
    db: Session = Depends(get_db)
):
    """Create activity form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from services.contact_service import get_contacts
    from services.deal_service import get_deals
    
    contacts, _ = get_contacts(db, skip=0, limit=1000)
    deals, _ = get_deals(db, skip=0, limit=1000)
    
    return templates.TemplateResponse(
        "activities/form.html",
        {
            "request": request,
            "activity": None,
            "contacts": contacts,
            "deals": deals,
            "contact_id": contact_id,
            "deal_id": deal_id,
            "types": ActivityType
        }
    )


@router.post("/")
async def create_activity_handler(
    request: Request,
    type: str = Form(...),
    subject: str = Form(...),
    description: Optional[str] = Form(None),
    outcome: Optional[str] = Form(None),
    contact_id: Optional[int] = Form(None),
    deal_id: Optional[int] = Form(None),
    scheduled_at: Optional[str] = Form(None),
    db: Session = Depends(get_db)
):
    """Create activity handler"""
    user_id = get_current_user_id(request)
    if not user_id:
        return RedirectResponse(url="/auth/login", status_code=303)
    
    scheduled = None
    if scheduled_at:
        try:
            scheduled = datetime.fromisoformat(scheduled_at.replace('Z', '+00:00'))
        except:
            pass
    
    activity_data = ActivityCreate(
        type=ActivityType(type),
        subject=subject,
        description=description,
        outcome=outcome,
        contact_id=contact_id,
        deal_id=deal_id,
        scheduled_at=scheduled
    )
    
    activity = create_activity(db, activity_data, user_id)
    
    redirect_url = "/activities"
    if contact_id:
        redirect_url = f"/contacts/{contact_id}"
    elif deal_id:
        redirect_url = f"/deals/{deal_id}"
    
    return RedirectResponse(url=f"{redirect_url}?success=Activity created", status_code=303)


@router.post("/{activity_id}/delete")
async def delete_activity_handler(
    request: Request,
    activity_id: int,
    db: Session = Depends(get_db)
):
    """Delete activity handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    success = delete_activity(db, activity_id)
    if not success:
        return RedirectResponse(url="/activities?error=Activity not found", status_code=303)
    
    return RedirectResponse(url="/activities?success=Activity deleted", status_code=303)

