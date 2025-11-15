"""Sample data seeder for demo"""
from db.database import SessionLocal
from db.models.user import User, UserRole
from db.models.company import Company
from db.models.contact import Contact
from db.models.deal import Deal, DealStage
from db.models.activity import Activity, ActivityType
from services.auth import get_password_hash
from datetime import datetime, timedelta
from decimal import Decimal

db = SessionLocal()

try:
    # Create admin user
    admin = db.query(User).filter(User.email == "admin@crosscrm.com").first()
    if not admin:
        admin = User(
            email="admin@crosscrm.com",
            username="admin",
            hashed_password=get_password_hash("admin123"),
            full_name="Admin User",
            role=UserRole.ADMIN
        )
        db.add(admin)
        db.flush()
        print("✓ Created admin user (admin@crosscrm.com / admin123)")
    
    # Create salesperson
    salesperson = db.query(User).filter(User.email == "sales@crosscrm.com").first()
    if not salesperson:
        salesperson = User(
            email="sales@crosscrm.com",
            username="salesperson",
            hashed_password=get_password_hash("sales123"),
            full_name="John Salesperson",
            role=UserRole.SALESPERSON
        )
        db.add(salesperson)
        db.flush()
        print("✓ Created salesperson user (sales@crosscrm.com / sales123)")
    
    # Create companies
    companies_data = [
        {"name": "Acme Corp", "industry": "Technology", "employee_count": 500},
        {"name": "Global Industries", "industry": "Manufacturing", "employee_count": 1000},
        {"name": "Tech Solutions Inc", "industry": "Software", "employee_count": 200},
    ]
    
    companies = []
    for comp_data in companies_data:
        company = db.query(Company).filter(Company.name == comp_data["name"]).first()
        if not company:
            company = Company(**comp_data)
            db.add(company)
            db.flush()
            companies.append(company)
            print(f"✓ Created company: {company.name}")
        else:
            companies.append(company)
    
    # Create contacts
    contacts_data = [
        {"first_name": "John", "last_name": "Doe", "email": "john@acme.com", "company": companies[0] if companies else None, "job_title": "CEO"},
        {"first_name": "Jane", "last_name": "Smith", "email": "jane@global.com", "company": companies[1] if len(companies) > 1 else None, "job_title": "CTO"},
        {"first_name": "Bob", "last_name": "Johnson", "email": "bob@tech.com", "company": companies[2] if len(companies) > 2 else None, "job_title": "VP Sales"},
    ]
    
    contacts = []
    for contact_data in contacts_data:
        company = contact_data.pop("company")
        contact = db.query(Contact).filter(Contact.email == contact_data["email"]).first()
        if not contact:
            contact = Contact(**contact_data, company_id=company.id if company else None, created_by=admin.id)
            db.add(contact)
            db.flush()
            contacts.append(contact)
            print(f"✓ Created contact: {contact.first_name} {contact.last_name}")
        else:
            contacts.append(contact)
    
    # Create deals
    deals_data = [
        {"title": "Enterprise License", "value": Decimal("50000"), "stage": DealStage.QUALIFICATION, "contact": contacts[0] if contacts else None, "company": companies[0] if companies else None},
        {"title": "Annual Support Contract", "value": Decimal("25000"), "stage": DealStage.PROPOSAL, "contact": contacts[1] if len(contacts) > 1 else None, "company": companies[1] if len(companies) > 1 else None},
        {"title": "Custom Development", "value": Decimal("75000"), "stage": DealStage.NEGOTIATION, "contact": contacts[2] if len(contacts) > 2 else None, "company": companies[2] if len(companies) > 2 else None},
        {"title": "Closed Deal", "value": Decimal("30000"), "stage": DealStage.CLOSED_WON, "contact": contacts[0] if contacts else None, "company": companies[0] if companies else None, "actual_close_date": datetime.now().date() - timedelta(days=5)},
    ]
    
    for deal_data in deals_data:
        contact = deal_data.pop("contact")
        company = deal_data.pop("company")
        actual_close = deal_data.pop("actual_close_date", None)
        
        deal = Deal(
            **deal_data,
            contact_id=contact.id if contact else None,
            company_id=company.id if company else None,
            assigned_to=salesperson.id,
            probability=75 if deal_data["stage"] != DealStage.CLOSED_WON else 100,
            actual_close_date=actual_close
        )
        db.add(deal)
        db.flush()
        print(f"✓ Created deal: {deal.title}")
    
    # Create activities
    if contacts:
        activity = Activity(
            type=ActivityType.CALL,
            subject="Initial discovery call",
            description="Discussed business needs and requirements",
            contact_id=contacts[0].id if contacts else None,
            user_id=salesperson.id,
            completed_at=datetime.now() - timedelta(days=2)
        )
        db.add(activity)
        print("✓ Created sample activity")
    
    db.commit()
    print("\n✓ Sample data seeded successfully!")
    
except Exception as e:
    db.rollback()
    print(f"✗ Error seeding data: {e}")
    import traceback
    traceback.print_exc()
finally:
    db.close()

