from fastapi import FastAPI, Request
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from fastapi.staticfiles import StaticFiles
from starlette.middleware.sessions import SessionMiddleware
from config import settings

app = FastAPI(
    title="Cross CRM",
    description="AI CRM and Sales Management Application",
    version="0.1.0"
)

# Session middleware
app.add_middleware(SessionMiddleware, secret_key=settings.SECRET_KEY)

# Templates configuration
templates = Jinja2Templates(directory="templates")

# Mount static files
app.mount("/static", StaticFiles(directory="static"), name="static")

# Import routers
from routes import auth, contacts, deals, activities, analytics, salespeople, market, companies, tasks
from typing import Optional

# Include routers
app.include_router(auth.router, prefix="/auth", tags=["auth"])
app.include_router(contacts.router, prefix="/contacts", tags=["contacts"])
app.include_router(companies.router, prefix="/companies", tags=["companies"])
app.include_router(deals.router, prefix="/deals", tags=["deals"])
app.include_router(activities.router, prefix="/activities", tags=["activities"])
app.include_router(tasks.router, prefix="/tasks", tags=["tasks"])
app.include_router(analytics.router, prefix="/analytics", tags=["analytics"])
app.include_router(salespeople.router, prefix="/salespeople", tags=["salespeople"])
app.include_router(market.router, prefix="/market", tags=["market"])


def get_current_user_id(request: Request):
    """Get current user ID from session"""
    return request.session.get("user_id")


@app.get("/", response_class=HTMLResponse)
async def root(request: Request):
    """Root route - redirect to dashboard or login"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    return RedirectResponse(url="/analytics/dashboard", status_code=303)


@app.get("/health")
async def health():
    """Health check endpoint"""
    return {"status": "healthy"}
