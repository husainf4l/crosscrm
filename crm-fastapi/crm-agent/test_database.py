#!/usr/bin/env python
"""
Database Connection Tester for CRM Agent
Tests PostgreSQL connection and migrations
"""

import asyncio
import sys
import os
from pathlib import Path

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent))

async def test_database_connection():
    """Test database connection and create tables"""
    
    print("=" * 70)
    print("CRM Agent - Database Connection Tester")
    print("=" * 70)
    print()
    
    try:
        # Import after path is set
        from app.db.database import init_db, engine
        from app.config.settings import settings
        
        print("📊 Configuration:")
        print(f"  Database URL: {settings.DATABASE_URL}")
        print(f"  Debug Mode: {settings.DEBUG}")
        print()
        
        print("🔄 Initializing database connection...")
        
        # Test connection
        async with engine.begin() as conn:
            result = await conn.execute("SELECT version();")
            version = await result.fetchone()
            if version:
                print(f"✅ Connected to PostgreSQL: {version[0]}")
        print()
        
        # Initialize tables
        print("📝 Creating database tables...")
        await init_db()
        print("✅ Database tables initialized")
        print()
        
        # Test query
        print("🧪 Running test query...")
        from sqlalchemy import text
        
        async with engine.connect() as conn:
            tables_result = await conn.execute(
                text("""
                SELECT table_name FROM information_schema.tables 
                WHERE table_schema = 'public'
                """)
            )
            tables = await tables_result.fetchall()
            
            if tables:
                print("✅ Found tables:")
                for table in tables:
                    print(f"   - {table[0]}")
            else:
                print("⚠️  No tables found yet")
        print()
        
        print("✅ Database connection test PASSED")
        print()
        
        return True
        
    except ImportError as e:
        print(f"❌ Import Error: {e}")
        print("   Make sure you're in the crm-agent directory")
        print("   Run: cd crm-agent && python test_database.py")
        return False
        
    except Exception as e:
        print(f"❌ Database Error: {e}")
        print()
        print("Troubleshooting tips:")
        print("  1. Check PostgreSQL is running")
        print("  2. Verify DATABASE_URL in .env")
        print("  3. Ensure user and database exist:")
        print("     psql -U postgres -c 'CREATE USER crm_user WITH PASSWORD \"crm_password\";'")
        print("     psql -U postgres -c 'CREATE DATABASE crm_db OWNER crm_user;'")
        print("  4. Check network connectivity to database")
        return False

async def main():
    """Main entry point"""
    try:
        success = await test_database_connection()
        sys.exit(0 if success else 1)
    except KeyboardInterrupt:
        print("\n\n⚠️  Test interrupted by user")
        sys.exit(1)

if __name__ == "__main__":
    # Run async function
    asyncio.run(main())
