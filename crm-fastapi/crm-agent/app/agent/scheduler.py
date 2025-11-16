"""Scheduler for automated CRM agent tasks."""
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.interval import IntervalTrigger
from apscheduler.triggers.cron import CronTrigger
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.database import AsyncSessionLocal
from app.db import crud
from app.agent.orchestrator import (
    AgentOrchestrator,
    run_morning_reminder,
    run_follow_up,
    run_closure_push,
    run_nurture,
    run_upsell
)


scheduler = AsyncIOScheduler(timezone="UTC")
orchestrator = AgentOrchestrator()


async def process_due_tasks():
    """
    Scheduled job to process tasks due today.
    Uses LLM to analyze tasks and generate messages (logged for chat interface).
    """
    async with AsyncSessionLocal() as db:
        try:
            # Get all users with tasks due today
            # For simplicity, we'll process tasks for user_id=1
            # In production, you'd iterate through all users
            user_id = 1
            
            tasks = await crud.get_today_tasks(db, user_id)
            
            for task in tasks:
                # Process tasks - LLM will interpret status values
                if task.status and task.status.lower() in ["pending", "open", "in progress"]:
                    # Analyze task using LLM
                    analysis = await orchestrator.analyze_task(
                        db=db,
                        user_id=user_id,
                        task_title=task.title,
                        task_details=f"Status: {task.status}, Due: {task.due_date}"
                    )
                    
                    # Log the message (available in chat interface)
                    message = f"Task Reminder: {task.title}\n\n{analysis}"
                    await crud.log_agent_run(
                        db=db,
                        user_id=user_id,
                        agent_type="TASK_REMINDER",
                        message=message
                    )
        except Exception as e:
            print(f"Error processing due tasks: {e}")


async def process_sales_followups():
    """
    Scheduled job to process sales follow-ups.
    Uses LLM to generate personalized follow-up messages.
    """
    async with AsyncSessionLocal() as db:
        try:
            # Get pending sales and generate follow-ups
            # This is a placeholder - implement based on your sales model
            user_id = 1
            
            # Example: Get sales that need follow-up
            # sales = await crud.get_pending_sales(db, user_id)
            # for sale in sales:
            #     followup = await orchestrator.generate_sales_followup(
            #         db=db,
            #         user_id=user_id,
            #         customer=sale.customer,
            #         product=sale.product,
            #         sales_status=sale.status.value
            #     )
            #     ...
            pass
        except Exception as e:
            print(f"Error processing sales follow-ups: {e}")


# Wrapper functions for each agent type that handle database sessions
async def reminder_wrapper(user_id: int):
    """Wrapper for REMINDER agent."""
    async with AsyncSessionLocal() as db:
        await run_morning_reminder(db, user_id)


async def follow_up_wrapper(user_id: int):
    """Wrapper for FOLLOW_UP agent."""
    async with AsyncSessionLocal() as db:
        await run_follow_up(db, user_id)


async def closure_wrapper(user_id: int):
    """Wrapper for CLOSURE agent."""
    async with AsyncSessionLocal() as db:
        await run_closure_push(db, user_id)


async def nurture_wrapper(user_id: int):
    """Wrapper for NURTURE agent."""
    async with AsyncSessionLocal() as db:
        await run_nurture(db, user_id)


async def upsell_wrapper(user_id: int):
    """Wrapper for UPSELL agent."""
    async with AsyncSessionLocal() as db:
        await run_upsell(db, user_id)


def start_scheduler():
    """Start the APScheduler with configured jobs."""
    # Morning reminder - 9:00 AM daily
    scheduler.add_job(
        reminder_wrapper,
        trigger=CronTrigger(hour=9, minute=0),
        args=[1],  # user_id, later configurable
        id="morning_reminder",
        name="Morning Reminder - Good Morning + Plan Check",
        replace_existing=True
    )
    
    # Midday follow-up - 1:00 PM daily
    scheduler.add_job(
        follow_up_wrapper,
        trigger=CronTrigger(hour=13, minute=0),
        args=[1],
        id="follow_up",
        name="Follow-up - Ask What Happened",
        replace_existing=True
    )
    
    # Late afternoon closure push - 4:00 PM daily
    scheduler.add_job(
        closure_wrapper,
        trigger=CronTrigger(hour=16, minute=0),
        args=[1],
        id="closure_push",
        name="Closure Push - Close Deals",
        replace_existing=True
    )
    
    # Nurture every 2 days at 11:00 AM
    scheduler.add_job(
        nurture_wrapper,
        trigger=CronTrigger(hour=11, minute=0, day="*/2"),
        args=[1],
        id="nurture",
        name="Nurture - Keep Leads Engaged",
        replace_existing=True
    )
    
    # Upsell every Monday at 10:00 AM
    scheduler.add_job(
        upsell_wrapper,
        trigger=CronTrigger(day_of_week="mon", hour=10, minute=0),
        args=[1],
        id="upsell",
        name="Upsell - Suggest Additional Products",
        replace_existing=True
    )
    
    scheduler.start()
    print("Scheduler started with multi-agent timeline:")
    print("- 09:00 → REMINDER (daily)")
    print("- 13:00 → FOLLOW_UP (daily)")
    print("- 16:00 → CLOSURE push (daily)")
    print("- 11:00 → NURTURE (every 2 days)")
    print("- 10:00 → UPSELL (every Monday)")


def shutdown_scheduler():
    """Shutdown the scheduler gracefully."""
    scheduler.shutdown()
    print("Scheduler shut down")

