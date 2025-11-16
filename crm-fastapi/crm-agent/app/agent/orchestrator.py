"""LLM orchestrator for CRM agent operations."""
from typing import Optional, Dict, Any
from sqlalchemy.ext.asyncio import AsyncSession
from openai import AsyncOpenAI

from app.config.settings import settings
from app.db import crud
from app.db.models import BusinessProfile
from app.agent.prompts import (
    AgentType,
    build_reminder_prompt,
    build_follow_up_prompt,
    build_closure_prompt,
    build_nurture_prompt,
    build_upsell_prompt,
    get_system_prompt,
    get_message_prompt,
    get_task_analysis_prompt,
    get_sales_prompt,
    get_morning_message_prompt,
    get_followup_prompt
)


class AgentOrchestrator:
    """Orchestrates LLM-powered agent operations."""
    
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)
        self.model = settings.OPENAI_MODEL
    
    async def generate_response(
        self,
        db: AsyncSession,
        user_id: int,
        user_message: str,
        context: str = ""
    ) -> str:
        """
        Generate a response using LLM based on business profile.
        
        Args:
            db: Database session
            user_id: User ID to get business profile
            user_message: User's message
            context: Additional context
            
        Returns:
            Generated response string
        """
        # Check if OpenAI API key is configured
        if not settings.OPENAI_API_KEY or settings.OPENAI_API_KEY == "your_openai_api_key_here":
            error_msg = "OpenAI API key is not configured. Please set OPENAI_API_KEY in your .env file."
            print(f"Error: {error_msg}")
            return f"I'm sorry, but I'm not configured yet. {error_msg}"
        
        # Get business profile and recent context (handle None database)
        business_profile = None
        recent_runs = []
        tasks = []
        leads = []
        
        if db is not None:
            try:
                business_profile = await crud.get_business_profile(db, user_id)
                # Get recent conversation history for context
                recent_runs = await crud.get_recent_agent_runs(db, user_id, limit=5)
                # Get current tasks and leads for context
                tasks = await crud.get_today_tasks(db, user_id)
                leads = await crud.get_leads(db, user_id)
            except Exception as e:
                print(f"Warning: Could not fetch context data: {e}")
        
        # Build context string from recent conversation and data
        context_parts = []
        if recent_runs:
            context_parts.append("Recent conversation history:")
            for run in recent_runs[-3:]:  # Last 3 messages for context
                context_parts.append(f"- [{run.agent_type}] {run.message[:100]}")
        
        if tasks:
            context_parts.append(f"\nToday's tasks: {', '.join([t.title for t in tasks[:3]])}")
        
        if leads:
            context_parts.append(f"\nActive leads: {', '.join([l.customer_name for l in leads[:3]])}")
        
        full_context = "\n".join(context_parts) if context_parts else context
        
        # Build prompts
        system_prompt = get_system_prompt(business_profile)
        user_prompt = get_message_prompt(user_message, full_context)
        
        # Call OpenAI API with higher temperature for more dynamic responses
        try:
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": user_prompt}
                ],
                temperature=0.8,  # Increased from 0.7 for more dynamic responses
                max_tokens=500
            )
            
            return response.choices[0].message.content.strip()
        except Exception as e:
            error_msg = str(e)
            print(f"Error generating LLM response: {error_msg}")
            import traceback
            traceback.print_exc()
            
            # Provide helpful error messages
            if "api_key" in error_msg.lower() or "authentication" in error_msg.lower():
                return "I'm sorry, but there's an issue with my API configuration. Please check your OpenAI API key."
            elif "rate limit" in error_msg.lower():
                return "I'm currently experiencing high demand. Please try again in a moment."
            else:
                return f"I apologize, but I'm having trouble processing your request right now. Error: {error_msg[:100]}"
    
    async def process_user_message(
        self,
        db: AsyncSession,
        user_id: int,
        message: str
    ) -> str:
        """
        Process a user message and generate response.
        
        Args:
            db: Database session
            user_id: User ID
            message: User's message
            
        Returns:
            Generated response message
        """
        response = await self.generate_response(
            db=db,
            user_id=user_id,
            user_message=message,
            context=""
        )
        return response
    
    async def analyze_task(
        self,
        db: AsyncSession,
        user_id: int,
        task_title: str,
        task_details: str = ""
    ) -> str:
        """
        Analyze a task and generate action recommendation.
        
        Args:
            db: Database session
            user_id: User ID
            task_title: Task title
            task_details: Additional task details
            
        Returns:
            Analysis and recommendation
        """
        business_profile = await crud.get_business_profile(db, user_id)
        system_prompt = get_system_prompt(business_profile)
        user_prompt = get_task_analysis_prompt(task_title, task_details)
        
        try:
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": user_prompt}
                ],
                temperature=0.5,
                max_tokens=300
            )
            
            return response.choices[0].message.content.strip()
        except Exception as e:
            print(f"Error analyzing task: {e}")
            return "Unable to analyze task at this time."
    
    async def generate_sales_followup(
        self,
        db: AsyncSession,
        user_id: int,
        customer: str,
        product: str,
        sales_status: str
    ) -> str:
        """
        Generate a sales follow-up message.
        
        Args:
            db: Database session
            user_id: User ID
            customer: Customer name
            product: Product name
            sales_status: Current sales status
            
        Returns:
            Generated follow-up message
        """
        business_profile = await crud.get_business_profile(db, user_id)
        system_prompt = get_system_prompt(business_profile)
        user_prompt = get_sales_prompt(customer, product, sales_status)
        
        try:
            response = await self.client.chat.completions.create(
                model=self.model,
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": user_prompt}
                ],
                temperature=0.7,
                max_tokens=400
            )
            
            return response.choices[0].message.content.strip()
        except Exception as e:
            print(f"Error generating sales follow-up: {e}")
            return "Unable to generate follow-up message at this time."
    
# Global OpenAI client
client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)


async def run_agent(db: AsyncSession, user_id: int, agent_type: AgentType) -> str:
    """
    Generic function to run any agent type.
    
    Args:
        db: Database session
        user_id: User ID to run agent for
        agent_type: Type of agent to run (REMINDER, FOLLOW_UP, CLOSURE, NURTURE, UPSELL)
        
    Returns:
        Generated message string
    """
    try:
        # Load data
        profile = await crud.get_business_profile(db, user_id)
        tasks = await crud.get_today_tasks(db, user_id)
        leads = await crud.get_leads(db, user_id)
        sales = await crud.get_sales_updates(db, user_id)
        
        # Get recent agent runs for context
        recent_runs = await crud.get_recent_agent_runs(db, user_id, limit=5)
        
        # Choose prompt builder based on agent type
        if agent_type == "REMINDER":
            prompt = build_reminder_prompt(profile, tasks, leads, sales=None, recent_runs=recent_runs)
        elif agent_type == "FOLLOW_UP":
            prompt = build_follow_up_prompt(profile, tasks, leads, sales, recent_runs=recent_runs)
        elif agent_type == "CLOSURE":
            prompt = build_closure_prompt(profile, tasks, leads, sales, recent_runs=recent_runs)
        elif agent_type == "NURTURE":
            prompt = build_nurture_prompt(profile, tasks, leads, sales, recent_runs=recent_runs)
        elif agent_type == "UPSELL":
            prompt = build_upsell_prompt(profile, tasks, leads, sales, recent_runs=recent_runs)
        else:
            raise ValueError(f"Unknown agent type: {agent_type}")
        
        # Call OpenAI
        result = await client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[{"role": "system", "content": prompt}]
        )
        
        # Extract message
        message = result.choices[0].message.content
        
        # Log agent run
        await crud.log_agent_run(
            db=db,
            user_id=user_id,
            agent_type=agent_type if isinstance(agent_type, str) else str(agent_type),
            message=message
        )
        
        return message
    except Exception as e:
        print(f"Error running {agent_type} agent: {e}")
        return f"Error: {str(e)}"


# Convenience wrapper functions
async def run_morning_reminder(db: AsyncSession, user_id: int) -> str:
    """Run REMINDER agent - good morning + plan check."""
    return await run_agent(db, user_id, "REMINDER")


async def run_follow_up(db: AsyncSession, user_id: int) -> str:
    """Run FOLLOW_UP agent - ask what happened with tasks/leads."""
    return await run_agent(db, user_id, "FOLLOW_UP")


async def run_closure_push(db: AsyncSession, user_id: int) -> str:
    """Run CLOSURE agent - push to close deals / ask for commitment."""
    return await run_agent(db, user_id, "CLOSURE")


async def run_nurture(db: AsyncSession, user_id: int) -> str:
    """Run NURTURE agent - keep warm leads engaged, add value."""
    return await run_agent(db, user_id, "NURTURE")


async def run_upsell(db: AsyncSession, user_id: int) -> str:
    """Run UPSELL agent - suggest additional products to existing customers."""
    return await run_agent(db, user_id, "UPSELL")

