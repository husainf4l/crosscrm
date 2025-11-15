"""Contact import/export routes"""
from fastapi import APIRouter, Request, Depends, UploadFile, File, Form
from fastapi.responses import HTMLResponse, RedirectResponse, StreamingResponse
from fastapi.templating import Jinja2Templates
from sqlalchemy.orm import Session
from db.database import get_db
from services.contact_service import create_contact, get_contacts
from schema.contact import ContactCreate
import csv
import io
from typing import Optional

router = APIRouter()
templates = Jinja2Templates(directory="templates")


def get_current_user_id(request: Request) -> Optional[int]:
    return request.session.get("user_id")


@router.get("/contacts/import", response_class=HTMLResponse)
async def import_contacts_page(request: Request):
    """Contact import page"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    return templates.TemplateResponse("contacts/import.html", {"request": request})


@router.post("/contacts/import")
async def import_contacts_handler(
    request: Request,
    file: UploadFile = File(...),
    db: Session = Depends(get_db)
):
    """Import contacts from CSV"""
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


@router.get("/contacts/export")
async def export_contacts(
    request: Request,
    format: str = "csv",
    db: Session = Depends(get_db)
):
    """Export contacts to CSV or Excel"""
    if not get_current_user_id(request):
        return RedirectResponse(url="/auth/login", status_code=303)
    
    contacts, _ = get_contacts(db, skip=0, limit=10000)
    
    if format == "csv":
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
    
    return RedirectResponse(url="/contacts?error=Unsupported format", status_code=303)

