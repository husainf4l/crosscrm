from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine, async_sessionmaker
from sqlalchemy.orm import DeclarativeBase
from typing import Optional
from app.config.settings import settings


class Base(DeclarativeBase):
    """Base class for all database models."""
    pass


# Create async engine with connection pool settings for better error handling
try:
    engine = create_async_engine(
        settings.DATABASE_URL,
        echo=settings.DEBUG,
        future=True,
        pool_pre_ping=True,  # Verify connections before using
        pool_recycle=3600,   # Recycle connections after 1 hour
    )
except Exception as e:
    print(f"Warning: Database engine creation failed: {e}")
    print("Server will start but database features may not work.")
    engine = None

# Create async session factory (only if engine is available)
if engine:
    AsyncSessionLocal = async_sessionmaker(
        engine,
        class_=AsyncSession,
        expire_on_commit=False,
        autocommit=False,
        autoflush=False
    )
else:
    AsyncSessionLocal = None


async def get_db() -> Optional[AsyncSession]:
    """
    Dependency function to get database session.
    Yields an async database session and ensures it's closed after use.
    Returns None if database is not configured.
    """
    if AsyncSessionLocal is None:
        # Return None instead of raising error - let services handle it gracefully
        yield None
        return
    
    try:
        async with AsyncSessionLocal() as session:
            try:
                yield session
            finally:
                await session.close()
    except Exception as e:
        print(f"Error creating database session: {e}")
        # Return None on error - let services handle it gracefully
        yield None


async def init_db():
    """Initialize database tables."""
    if not engine:
        print("Warning: Database engine not available. Skipping table creation.")
        return
    try:
        async with engine.begin() as conn:
            await conn.run_sync(Base.metadata.create_all)
    except Exception as e:
        print(f"Warning: Database initialization failed: {e}")
        print("Server will continue but database features may not work.")

