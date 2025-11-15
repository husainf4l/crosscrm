"""Test database connection"""
from db.database import engine, Base
from db.models import *
from sqlalchemy import inspect

try:
    # Test connection
    with engine.connect() as conn:
        print("✓ Database connection successful")
    
    # Check if tables exist
    inspector = inspect(engine)
    tables = inspector.get_table_names()
    print(f"✓ Found {len(tables)} tables in database")
    
    if tables:
        print("Tables:", ", ".join(tables))
    else:
        print("No tables found. Run migrations to create tables.")
        
except Exception as e:
    print(f"✗ Error: {e}")

