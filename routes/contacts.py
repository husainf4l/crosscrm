from fastapi import APIRouter, Request, Depends, Form, Query, UploadFile, File
from fastapi.responses import HTMLResponse, RedirectResponse, StreamingResponse
from fastapi.templating import Jinja2Templates
import csv
import io
from sqlalchemy.orm import Session
from db.database import get_db
from db.models.user import User
from services.contact_service import (
    get_contacts, get_contact_by_id, create_contact,
    update_contact, delete_contact
)
from schema.contact import ContactCreate, ContactUpdate
from typing import Optional

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request) -> Optional[int]:
    """Get current user ID from session"""
    return request.session.get("user_id")


@router.get("/import", response_class=HTMLResponse)
async def import_contacts_page(request: Request):
    """Contact import page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    return templates.TemplateResponse("contacts/import.html", {"request": request})


@router.get("/export")
async def export_contacts(
    request: Request,
    format: str = Query("csv"),
    db: Session = Depends(get_db)
):
    """Export contacts to CSV"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from fastapi.responses import StreamingResponse
    import io
    import csv
    
    contacts, _ = get_contacts(db, skip=0, limit=10000)
    
    output = io.StringIO()
    writer = csv.writer(output)
    writer.writerow(['first_name', 'last_name', 'email', 'phone', 'job_title', 'company_id'])
    
    for contact in contacts:
        writer.writerow([
            contact.first_name,
            contact.last_name,
            contact.email or '',
            contact.phone or '',
            contact.job_title or '',
            contact.company_id or ''
        ])
    
    output.seek(0)
    return StreamingResponse(
        iter([output.getvalue()]),
        media_type="text/csv",
        headers={"Content-Disposition": "attachment; filename=contacts_export.csv"}
    )


@router.post("/import")
async def import_contacts_handler(
    request: Request,
    file: UploadFile = File(...),
    db: Session = Depends(get_db)
):
    """Import contacts from CSV"""
    from fastapi import UploadFile, File
    import io
    import csv
    
    user_id = get_current_user_id(request)
    if not user_id:
        return RedirectResponse(url="/auth/login", status_code=303)
    
    imported = 0
    errors = []
    
    try:
        contents = await file.read()
        csv_file = io.StringIO(contents.decode('utf-8'))
        reader = csv.DictReader(csv_file)
        
        for row_num, row in enumerate(reader, start=2):
            try:
                first_name = row.get('first_name', '').strip()
                last_name = row.get('last_name', '').strip()
                email = row.get('email', '').strip() or None
                phone = row.get('phone', '').strip() or None
                job_title = row.get('job_title', '').strip() or None
                company_id = row.get('company_id', '').strip()
                
                if not first_name or not last_name:
                    errors.append(f"Row {row_num}: Missing first_name or last_name")
                    continue
                
                company_id_int = None
                if company_id:
                    try:
                        company_id_int = int(company_id)
                    except:
                        pass
                
                contact_data = ContactCreate(
                    first_name=first_name,
                    last_name=last_name,
                    email=email,
                    phone=phone,
                    job_title=job_title,
                    company_id=company_id_int
                )
                
                create_contact(db, contact_data, user_id)
                imported += 1
            except Exception as e:
                errors.append(f"Row {row_num}: {str(e)}")
        
        message = f"Imported {imported} contacts"
        if errors:
            message += f". {len(errors)} errors occurred."
        
        return RedirectResponse(
            url=f"/contacts?success={message}",
            status_code=303
        )
    except Exception as e:
        return RedirectResponse(
            url=f"/contacts/import?error=Import failed: {str(e)}",
            status_code=303
        )


@router.get("/", response_class=HTMLResponse)
async def list_contacts(
    request: Request,
    page: int = Query(1, ge=1),
    page_size: int = Query(20, ge=1, le=100),
    search: Optional[str] = Query(None),
    company_id: Optional[int] = Query(None),
    db: Session = Depends(get_db)
):
    """List all contacts"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    skip = (page - 1) * page_size
    contacts, total = get_contacts(db, skip=skip, limit=page_size, search=search, company_id=company_id)
    
    # Get companies for filter dropdown
    from services.company_service import get_companies
    companies, _ = get_companies(db, skip=0, limit=1000)
    
    total_pages = (total + page_size - 1) // page_size
    
    return templates.TemplateResponse(
        "contacts/list.html",
        {
            "request": request,
            "contacts": contacts,
            "page": page,
            "page_size": page_size,
            "total": total,
            "total_pages": total_pages,
            "search": search,
            "company_id": company_id,
            "companies": companies
        }
    )


@router.get("/{contact_id}", response_class=HTMLResponse)
async def contact_detail(
    request: Request,
    contact_id: int,
    db: Session = Depends(get_db)
):
    """Contact detail page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    contact = get_contact_by_id(db, contact_id)
    if not contact:
        return RedirectResponse(url="/contacts?error=Contact not found", status_code=303)
    
    # Get related activities
    from services.activity_service import get_activities
    activities, _ = get_activities(db, skip=0, limit=10, contact_id=contact_id)
    
    return templates.TemplateResponse(
        "contacts/detail.html",
        {"request": request, "contact": contact, "activities": activities}
    )


@router.get("/new", response_class=HTMLResponse)
async def new_contact_form(request: Request, db: Session = Depends(get_db)):
    """Create contact form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    from services.company_service import get_companies
    companies, _ = get_companies(db, skip=0, limit=1000)
    
    return templates.TemplateResponse(
        "contacts/form.html",
        {"request": request, "contact": None, "companies": companies}
    )


@router.post("/")
async def create_contact_handler(
    request: Request,
    first_name: str = Form(...),
    last_name: str = Form(...),
    email: Optional[str] = Form(None),
    phone: Optional[str] = Form(None),
    job_title: Optional[str] = Form(None),
    company_id: Optional[int] = Form(None),
    db: Session = Depends(get_db)
):
    """Create contact handler"""
    user_id = get_current_user_id(request)
    if not user_id:
        return RedirectResponse(url="/auth/login", status_code=303)
    
    contact_data = ContactCreate(
        first_name=first_name,
        last_name=last_name,
        email=email,
        phone=phone,
        job_title=job_title,
        company_id=company_id
    )
    
    contact = create_contact(db, contact_data, user_id)
    return RedirectResponse(url=f"/contacts/{contact.id}?success=Contact created", status_code=303)


@router.get("/{contact_id}/edit", response_class=HTMLResponse)
async def edit_contact_form(
    request: Request,
    contact_id: int,
    db: Session = Depends(get_db)
):
    """Edit contact form"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    contact = get_contact_by_id(db, contact_id)
    if not contact:
        return RedirectResponse(url="/contacts?error=Contact not found", status_code=303)
    
    from services.company_service import get_companies
    companies, _ = get_companies(db, skip=0, limit=1000)
    
    return templates.TemplateResponse(
        "contacts/form.html",
        {"request": request, "contact": contact, "companies": companies}
    )


@router.post("/{contact_id}")
async def update_contact_handler(
    request: Request,
    contact_id: int,
    first_name: str = Form(...),
    last_name: str = Form(...),
    email: Optional[str] = Form(None),
    phone: Optional[str] = Form(None),
    job_title: Optional[str] = Form(None),
    company_id: Optional[int] = Form(None),
    db: Session = Depends(get_db)
):
    """Update contact handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    contact_data = ContactUpdate(
        first_name=first_name,
        last_name=last_name,
        email=email,
        phone=phone,
        job_title=job_title,
        company_id=company_id
    )
    
    contact = update_contact(db, contact_id, contact_data)
    if not contact:
        return RedirectResponse(url="/contacts?error=Contact not found", status_code=303)
    
    return RedirectResponse(url=f"/contacts/{contact.id}?success=Contact updated", status_code=303)


@router.post("/{contact_id}/delete")
async def delete_contact_handler(
    request: Request,
    contact_id: int,
    db: Session = Depends(get_db)
):
    """Delete contact handler"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    success = delete_contact(db, contact_id)
    if not success:
        return RedirectResponse(url="/contacts?error=Contact not found", status_code=303)
    
    return RedirectResponse(url="/contacts?success=Contact deleted", status_code=303)

