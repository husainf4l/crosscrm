from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from typing import Optional, List
from datetime import date

from app.db.models import Task, Targets, BusinessProfile, Leads, Sales, AgentRunLog


async def get_today_tasks(db: AsyncSession, user_id: int) -> List[Task]:
    """
    Get all tasks for a user with today's date.
    Tries GraphQL backend first, falls back to local database.
    
    Args:
        db: Database session
        user_id: User ID to filter tasks
        
    Returns:
        List of tasks with today's due_date
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    # Try GraphQL backend first
    try:
        from app.modules.agent.services.graphql_data_service import GraphQLDataService
        graphql_service = GraphQLDataService()
        graphql_tasks = await graphql_service.get_tasks_for_user(user_id)
        
        if graphql_tasks:
            # Convert GraphQL tasks to Task models
            tasks = []
            for gt in graphql_tasks:
                task = Task(
                    id=gt.get("id", 0),
                    user_id=user_id,
                    title=gt.get("title", ""),
                    status=gt.get("status", "pending"),
                    due_date=date.fromisoformat(gt.get("dueDate", date.today().isoformat()))
                )
                tasks.append(task)
            return tasks
    except Exception as e:
        print(f"GraphQL fetch failed, using local DB: {e}")
    
    # Fallback to local database
    today = date.today()
    result = await db.execute(
        select(Task)
        .where(Task.user_id == user_id)
        .where(Task.due_date == today)
    )
    return list(result.scalars().all())


async def get_progress(db: AsyncSession, user_id: int) -> float:
    """
    Calculate progress percentage for a user.
    Returns sum of (achieved/target * 100) for all targets.
    
    Args:
        db: Database session
        user_id: User ID to calculate progress for
        
    Returns:
        Total progress percentage (float)
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    result = await db.execute(
        select(Targets)
        .where(Targets.user_id == user_id)
    )
    targets = result.scalars().all()
    
    if not targets:
        return 0.0
    
    total_progress = 0.0
    for target in targets:
        if target.target > 0:  # Avoid division by zero
            progress = (target.achieved / target.target) * 100
            total_progress += progress
    
    return total_progress


async def get_user_phone(db: AsyncSession, user_id: int) -> str:
    """
    Get user phone number (dummy placeholder).
    
    Args:
        db: Database session
        user_id: User ID to get phone for
        
    Returns:
        Phone number string (dummy placeholder)
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    # Dummy placeholder: return a formatted phone number based on user_id
    phone = f"+1234567890{str(user_id).zfill(2)}"
    return phone


async def get_business_profile(db: AsyncSession, user_id: int) -> Optional[BusinessProfile]:
    """
    Get business profile for a user.
    
    Args:
        db: Database session
        user_id: User ID to get business profile for
        
    Returns:
        BusinessProfile or None if not found
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    if db is None:
        return None
    
    try:
        result = await db.execute(
            select(BusinessProfile)
            .where(BusinessProfile.user_id == user_id)
        )
        return result.scalar_one_or_none()
    except Exception as e:
        print(f"Error fetching business profile: {e}")
        return None


async def create_or_update_business_profile(
    db: AsyncSession,
    user_id: int,
    business_type: str,
    products: Optional[list] = None,
    tone: Optional[str] = None,
    daily_goal: Optional[str] = None,
    keywords: Optional[list] = None
) -> BusinessProfile:
    """
    Create or update a business profile for a user.
    
    Args:
        db: Database session
        user_id: User ID
        business_type: Type of business (ex: chicken, cheese, cars, real estate)
        products: List of products (ex: ["cheese", "labaneh", "milk"])
        tone: Communication tone (ex: friendly, strict, professional)
        daily_goal: Daily goal description (ex: "sell 20 cheese blocks")
        keywords: List of keywords (ex: ["target", "follow-up", "closing"])
        
    Returns:
        Created or updated BusinessProfile
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    profile = await get_business_profile(db, user_id)
    
    if profile:
        # Update existing profile
        profile.business_type = business_type
        if products is not None:
            profile.products = products
        if tone is not None:
            profile.tone = tone
        if daily_goal is not None:
            profile.daily_goal = daily_goal
        if keywords is not None:
            profile.keywords = keywords
    else:
        # Create new profile
        profile = BusinessProfile(
            user_id=user_id,
            business_type=business_type,
            products=products,
            tone=tone,
            daily_goal=daily_goal,
            keywords=keywords
        )
        db.add(profile)
    
    await db.commit()
    await db.refresh(profile)
    return profile


async def get_leads(db: AsyncSession, user_id: int) -> List[Leads]:
    """
    Get all leads for a user.
    
    Args:
        db: Database session
        user_id: User ID to filter leads
        
    Returns:
        List of leads for the user, ordered by most recently updated
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    result = await db.execute(
        select(Leads)
        .where(Leads.user_id == user_id)
        .order_by(Leads.updated_at.desc())
    )
    return list(result.scalars().all())


async def get_sales_updates(db: AsyncSession, user_id: int) -> List[Sales]:
    """
    Get sales updates for a user.
    Tries GraphQL backend first, falls back to local database.
    Returns all sales records for the user, ordered by most recent.
    
    Args:
        db: Database session
        user_id: User ID to filter sales
        
    Returns:
        List of sales records for the user, ordered by most recent
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    # Try GraphQL backend first
    try:
        from app.modules.agent.services.graphql_data_service import GraphQLDataService
        graphql_service = GraphQLDataService()
        graphql_opportunities = await graphql_service.get_opportunities_for_user(user_id)
        
        if graphql_opportunities:
            # Convert GraphQL opportunities to Sales models
            sales = []
            for go in graphql_opportunities:
                sale = Sales(
                    id=go.get("id", 0),
                    user_id=user_id,
                    customer=f"Customer {go.get('customerId', '')}",
                    product=go.get("name", ""),
                    status=go.get("stage", "pending"),
                    reason_failed=None if go.get("status") == "won" else "In progress"
                )
                sales.append(sale)
            return sales
    except Exception as e:
        print(f"GraphQL fetch failed, using local DB: {e}")
    
    # Fallback to local database
    result = await db.execute(
        select(Sales)
        .where(Sales.user_id == user_id)
        .order_by(Sales.id.desc())
    )
    return list(result.scalars().all())


async def log_agent_run(
    db: AsyncSession,
    user_id: int,
    agent_type: str,
    message: str
) -> AgentRunLog:
    """
    Log an agent run.
    
    Args:
        db: Database session
        user_id: User ID
        agent_type: Type of agent that ran
        message: Message that was sent
        
    Returns:
        Created AgentRunLog instance
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    log_entry = AgentRunLog(
        user_id=user_id,
        agent_type=agent_type,
        message=message
    )
    db.add(log_entry)
    await db.commit()
    await db.refresh(log_entry)
    return log_entry


async def get_recent_agent_runs(
    db: AsyncSession,
    user_id: int,
    limit: int = 5
) -> List[AgentRunLog]:
    """
    Get recent agent runs for a user.
    
    Args:
        db: Database session
        user_id: User ID to get runs for
        limit: Maximum number of runs to return (default: 5)
        
    Returns:
        List of recent AgentRunLog entries, ordered by most recent first
    """
    if not isinstance(user_id, int) or user_id <= 0:
        raise ValueError("user_id must be a positive integer")
    
    if db is None:
        return []
    
    try:
        result = await db.execute(
            select(AgentRunLog)
            .where(AgentRunLog.user_id == user_id)
            .order_by(AgentRunLog.created_at.desc())
            .limit(limit)
        )
        return list(result.scalars().all())
    except Exception as e:
        print(f"Error fetching recent agent runs: {e}")
        # Return empty list instead of raising error
        return []

