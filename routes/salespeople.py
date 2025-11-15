from fastapi import APIRouter, Request, Depends
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.user import User
from services.analytics_service import get_performance_by_salesperson

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request):
    return request.session.get("user_id")


@router.get("/", response_class=HTMLResponse)
async def list_salespeople(request: Request, db: Session = Depends(get_db)):
    """List all salespeople"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    users = db.query(User).filter(User.role.in_(["salesperson", "manager", "admin"])).all()
    performance = get_performance_by_salesperson(db)
    
    # Create a dict for quick lookup
    perf_dict = {p["user_id"]: p for p in performance}
    
    # Calculate totals
    total_revenue = sum(p["total_revenue"] for p in performance)
    total_won = sum(p["won_deals"] for p in performance)
    
    return templates.TemplateResponse(
        "salespeople/list.html",
        {
            "request": request,
            "users": users,
            "performance": perf_dict,
            "total_revenue": total_revenue,
            "total_won": total_won
        }
    )


@router.get("/{user_id}", response_class=HTMLResponse)
async def salesperson_detail(request: Request, user_id: int, db: Session = Depends(get_db)):
    """Salesperson detail/dashboard"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    user = db.query(User).filter(User.id == user_id).first()
    if not user:
        return RedirectResponse(url="/salespeople?error=User not found", status_code=303)
    
    from services.deal_service import get_deals
    deals, _ = get_deals(db, skip=0, limit=100, assigned_to=user_id)
    
    performance = get_performance_by_salesperson(db)
    user_perf = next((p for p in performance if p["user_id"] == user_id), None)
    
    return templates.TemplateResponse(
        "salespeople/detail.html",
        {"request": request, "user": user, "deals": deals, "performance": user_perf}
    )

