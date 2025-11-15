from fastapi import APIRouter, Request, Depends, Form, Query
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.task import TaskStatus, TaskPriority
from services.task_service import (
    get_tasks, get_task_by_id, create_task,
    update_task, complete_task, delete_task
)
from schema.task import TaskCreate, TaskUpdate
from typing import Optional
from datetime import datetime

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request) -> Optional[int]:
    return request.session.get("user_id")


@router.get("/", response_class=HTMLResponse)
async def list_tasks(
    request: Request,
    page: int = Query(1, ge=1),
    status: Optional[str] = Query(None),
    priority: Optional[str] = Query(None),
    db: Session = Depends(get_db)
):
    """List all tasks"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    user_id = get_current_user_id(request)
    task_status = TaskStatus(status) if status else None
    task_priority = TaskPriority(priority) if priority else None
    
    tasks, total = get_tasks(
        db, skip=(page-1)*50, limit=50,
        assigned_to=user_id, status=task_status, priority=task_priority
    )
    
    return templates.TemplateResponse(
        "tasks/list.html",
        {
            "request": request,
            "tasks": tasks,
            "page": page,
            "total": total,
            "status": status,
            "priority": priority,
            "statuses": TaskStatus,
            "priorities": TaskPriority
        }
    )


@router.get("/{task_id}", response_class=HTMLResponse)
async def task_detail(request: Request, task_id: int, db: Session = Depends(get_db)):
    """Task detail page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    task = get_task_by_id(db, task_id)
    if not task:
        return RedirectResponse(url="/tasks?error=Task not found", status_code=303)
    
    return templates.TemplateResponse(
        "tasks/detail.html",
        {"request": request, "task": task}
    )


@router.get("/new", response_class=HTMLResponse)
async def new_task_form(
    request: Request,
    contact_id: Optional[int] = Query(None),
    deal_id: Optional[int] = Query(None),
    db: Session = Depends(get_db)
):
    """Create task form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from services.contact_service import get_contacts
    from services.deal_service import get_deals
    from db.models.user import User
    
    contacts, _ = get_contacts(db, skip=0, limit=1000)
    deals, _ = get_deals(db, skip=0, limit=1000)
    users = db.query(User).all()
    
    return templates.TemplateResponse(
        "tasks/form.html",
        {
            "request": request,
            "task": None,
            "contacts": contacts,
            "deals": deals,
            "users": users,
            "contact_id": contact_id,
            "deal_id": deal_id,
            "statuses": TaskStatus,
            "priorities": TaskPriority
        }
    )


@router.post("/")
async def create_task_handler(
    request: Request,
    title: str = Form(...),
    description: Optional[str] = Form(None),
    priority: str = Form(...),
    status: str = Form(...),
    due_date: Optional[str] = Form(None),
    assigned_to: int = Form(...),
    related_contact_id: Optional[int] = Form(None),
    related_deal_id: Optional[int] = Form(None),
    db: Session = Depends(get_db)
):
    """Create task handler"""
    user_id = get_current_user_id(request)
    if not user_id:
        return RedirectResponse(url="/auth/login", status_code=303)
    
    due = None
    if due_date:
        try:
            due = datetime.strptime(due_date, '%Y-%m-%d').date()
        except:
            pass
    
    task_data = TaskCreate(
        title=title,
        description=description,
        priority=TaskPriority(priority),
        status=TaskStatus(status),
        due_date=due,
        assigned_to=assigned_to,
        related_contact_id=related_contact_id,
        related_deal_id=related_deal_id
    )
    
    task = create_task(db, task_data, user_id)
    
    redirect_url = "/tasks"
    if related_contact_id:
        redirect_url = f"/contacts/{related_contact_id}"
    elif related_deal_id:
        redirect_url = f"/deals/{related_deal_id}"
    
    return RedirectResponse(url=f"{redirect_url}?success=Task created", status_code=303)


@router.post("/{task_id}/complete")
async def complete_task_handler(
    request: Request,
    task_id: int,
    db: Session = Depends(get_db)
):
    """Complete task handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    task = complete_task(db, task_id)
    if not task:
        return RedirectResponse(url="/tasks?error=Task not found", status_code=303)
    
    return RedirectResponse(url=f"/tasks/{task.id}?success=Task completed", status_code=303)


@router.post("/{task_id}/delete")
async def delete_task_handler(
    request: Request,
    task_id: int,
    db: Session = Depends(get_db)
):
    """Delete task handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    success = delete_task(db, task_id)
    if not success:
        return RedirectResponse(url="/tasks?error=Task not found", status_code=303)
    
    return RedirectResponse(url="/tasks?success=Task deleted", status_code=303)

