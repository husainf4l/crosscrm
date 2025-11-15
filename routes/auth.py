from fastapi import APIRouter, Request, Depends, HTTPException, Form
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.user import User
from services.auth import authenticate_user, create_user, get_user_by_email
from schema.user import UserCreate, UserLogin
from typing import Optional

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user(request: Request, db: Session = Depends(get_db)):
    """Get current user from session"""
    user_id = request.session.get("user_id")
    if not user_id:
        return None
    user = db.query(User).filter(User.id == user_id).first()
    return user


@router.get("/login", response_class=HTMLResponse)
async def login_page(request: Request):
    """Login page"""
    return templates.TemplateResponse("auth/login.html", {"request": request})


@router.post("/login")
async def login(
    request: Request,
    email: str = Form(...),
    password: str = Form(...),
    db: Session = Depends(get_db)
):
    """Login handler"""
    user = authenticate_user(db, email, password)
    if not user:
        return templates.TemplateResponse(
            "auth/login.html",
            {"request": request, "error": "Invalid email or password"}
        )
    
    request.session["user_id"] = user.id
    request.session["user_email"] = user.email
    request.session["user_name"] = user.full_name
    request.session["user_role"] = user.role.value
    
    return RedirectResponse(url="/", status_code=303)


@router.post("/logout")
async def logout(request: Request):
    """Logout handler"""
    request.session.clear()
    return RedirectResponse(url="/auth/login", status_code=303)


@router.get("/register", response_class=HTMLResponse)
async def register_page(request: Request):
    """Registration page"""
    return templates.TemplateResponse("auth/register.html", {"request": request})


@router.post("/register")
async def register(
    request: Request,
    email: str = Form(...),
    username: str = Form(...),
    full_name: str = Form(...),
    password: str = Form(...),
    db: Session = Depends(get_db)
):
    """Registration handler"""
    # Check if user exists
    if get_user_by_email(db, email):
        return templates.TemplateResponse(
            "auth/register.html",
            {"request": request, "error": "Email already registered"}
        )
    
    # Validate password length before creating UserCreate object
    if len(password.encode('utf-8')) > 72:
        password = password.encode('utf-8')[:72].decode('utf-8', errors='ignore')
    
    if len(password) < 8:
        return templates.TemplateResponse(
            "auth/register.html",
            {"request": request, "error": "Password must be at least 8 characters long"}
        )
    
    user_data = UserCreate(
        email=email,
        username=username,
        full_name=full_name,
        password=password
    )
    
    try:
        user = create_user(db, user_data)
        request.session["user_id"] = user.id
        request.session["user_email"] = user.email
        request.session["user_name"] = user.full_name
        request.session["user_role"] = user.role.value
        return RedirectResponse(url="/", status_code=303)
    except Exception as e:
        return templates.TemplateResponse(
            "auth/register.html",
            {"request": request, "error": f"Registration failed: {str(e)}"}
        )

