from fastapi import APIRouter, Request, Depends
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.market_data import MarketData

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request):
    return request.session.get("user_id")


@router.get("/insights", response_class=HTMLResponse)
async def market_insights(request: Request, db: Session = Depends(get_db)):
    """Market insights page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    # Get market data
    market_data = db.query(MarketData).order_by(MarketData.created_at.desc()).limit(20).all()
    
    # Group by type
    trends = [d for d in market_data if d.data_type.value == "trend"]
    competitors = [d for d in market_data if d.data_type.value == "competitor"]
    news = [d for d in market_data if d.data_type.value == "news"]
    
    return templates.TemplateResponse(
        "market/insights.html",
        {
            "request": request,
            "trends": trends,
            "competitors": competitors,
            "news": news
        }
    )

