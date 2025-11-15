from fastapi import APIRouter, Request, Depends, Query
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from services.analytics_service import (
    get_sales_metrics, get_pipeline_metrics, get_sales_trends,
    get_performance_by_salesperson, get_win_rate
)
from typing import Optional

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request):
    return request.session.get("user_id")


@router.get("/dashboard", response_class=HTMLResponse)
async def dashboard(
    request: Request,
    start_date: Optional[str] = None,
    end_date: Optional[str] = None,
    period: str = "30",
    db: Session = Depends(get_db)
):
    """Main dashboard"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from datetime import datetime, timedelta
    
    # Parse dates
    start = None
    end = None
    if start_date:
        try:
            start = datetime.strptime(start_date, '%Y-%m-%d')
        except:
            pass
    if end_date:
        try:
            end = datetime.strptime(end_date, '%Y-%m-%d')
        except:
            pass
    
    period_days = int(period) if period.isdigit() else 30
    
    sales_metrics = get_sales_metrics(db, start_date=start, end_date=end)
    pipeline_metrics = get_pipeline_metrics(db)
    trends = get_sales_trends(db, period_days=period_days)
    performance = get_performance_by_salesperson(db)
    win_rate = get_win_rate(db)
    
    # Get recent deals
    from services.deal_service import get_deals
    recent_deals, _ = get_deals(db, skip=0, limit=5)
    
    # Calculate comparison periods
    if start and end:
        period_days_compare = (end - start).days
        prev_start = start - timedelta(days=period_days_compare)
        prev_end = start
        prev_metrics = get_sales_metrics(db, start_date=prev_start, end_date=prev_end)
    else:
        prev_metrics = None
    
    return templates.TemplateResponse(
        "analytics/dashboard.html",
        {
            "request": request,
            "sales_metrics": sales_metrics,
            "pipeline_metrics": pipeline_metrics,
            "trends": trends,
            "performance": performance,
            "win_rate": win_rate,
            "recent_deals": recent_deals,
            "start_date": start_date,
            "end_date": end_date,
            "period": period,
            "prev_metrics": prev_metrics
        }
    )

