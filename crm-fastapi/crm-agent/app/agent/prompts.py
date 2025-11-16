"""Prompt templates for LLM agent interactions with multiple agent types."""
from typing import Literal

# Agent Type Definitions
AgentType = Literal["REMINDER", "FOLLOW_UP", "CLOSURE", "NURTURE", "UPSELL"]


def format_business_profile(business_profile) -> str:
    """
    Format business profile data for prompts.
    
    Args:
        business_profile: BusinessProfile model instance
        
    Returns:
        Formatted business profile string
    """
    if not business_profile:
        return "Not available"
    
    products_str = ", ".join(business_profile.products) if business_profile.products else "Not specified"
    keywords_str = ", ".join(business_profile.keywords) if business_profile.keywords else "Not specified"
    
    return f"""
- Business Type: {business_profile.business_type}
- Products: {products_str}
- Tone: {business_profile.tone or 'Professional'}
- Daily Goal: {business_profile.daily_goal or 'Not specified'}
- Keywords: {keywords_str}
"""


def format_tasks(tasks) -> str:
    """
    Format tasks list for prompts.
    
    Args:
        tasks: List of Task model instances
        
    Returns:
        Formatted tasks string
    """
    if not tasks:
        return "No tasks today"
    
    task_list = []
    for task in tasks:
        task_str = f"- {task.title}"
        if task.status:
            task_str += f" (Status: {task.status})"
        if task.due_date:
            task_str += f" (Due: {task.due_date})"
        task_list.append(task_str)
    
    return "\n".join(task_list)


def format_leads(leads) -> str:
    """
    Format leads list for prompts.
    
    Args:
        leads: List of Leads model instances
        
    Returns:
        Formatted leads string
    """
    if not leads:
        return "No leads"
    
    lead_list = []
    for lead in leads:
        lead_str = f"- {lead.customer_name}"
        if lead.stage:
            lead_str += f" (Stage: {lead.stage})"
        if lead.notes:
            lead_str += f" - {lead.notes[:50]}..." if len(lead.notes) > 50 else f" - {lead.notes}"
        lead_list.append(lead_str)
    
    return "\n".join(lead_list)


def format_sales(sales) -> str:
    """
    Format sales list for prompts.
    
    Args:
        sales: List of Sales model instances
        
    Returns:
        Formatted sales string
    """
    if not sales:
        return "No sales updates"
    
    sales_list = []
    for sale in sales:
        sale_str = f"- Customer: {sale.customer}, Product: {sale.product}"
        if sale.status:
            sale_str += f" (Status: {sale.status})"
        if sale.reason_failed:
            sale_str += f" - Reason: {sale.reason_failed}"
        sales_list.append(sale_str)
    
    return "\n".join(sales_list)


def format_recent_runs(recent_runs) -> str:
    """
    Format recent agent runs for prompts.
    
    Args:
        recent_runs: List of AgentRunLog model instances
        
    Returns:
        Formatted recent runs string
    """
    if not recent_runs:
        return "No previous messages"
    
    runs_list = []
    for run in recent_runs:
        run_str = f"- [{run.agent_type}] {run.created_at.strftime('%Y-%m-%d %H:%M')}: {run.message[:100]}..."
        if len(run.message) <= 100:
            run_str = f"- [{run.agent_type}] {run.created_at.strftime('%Y-%m-%d %H:%M')}: {run.message}"
        runs_list.append(run_str)
    
    return "\n".join(runs_list)


def build_reminder_prompt(
    business_profile,
    tasks,
    leads,
    sales=None,
    recent_runs=None
) -> str:
    """
    Build prompt for REMINDER agent - good morning + plan check.
    
    Args:
        business_profile: BusinessProfile model instance
        tasks: List of Task model instances
        leads: List of Leads model instances
        sales: Optional list of Sales model instances
        recent_runs: Optional list of AgentRunLog instances
        
    Returns:
        Reminder prompt string
    """
    business_type = business_profile.business_type if business_profile else "Not specified"
    products = ", ".join(business_profile.products) if business_profile and business_profile.products else "Not specified"
    tone = business_profile.tone if business_profile and business_profile.tone else "Professional"
    daily_goal = business_profile.daily_goal if business_profile and business_profile.daily_goal else "Not specified"
    keywords = ", ".join(business_profile.keywords) if business_profile and business_profile.keywords else "Not specified"
    
    tasks_str = format_tasks(tasks)
    leads_str = format_leads(leads)
    recent_runs_str = format_recent_runs(recent_runs) if recent_runs else "No previous messages"
    
    prompt = f"""You are a CRM sales agent for a business, talking to one salesperson (user).
Do not mention that you are a language model.

BUSINESS PROFILE:
- Business Type: {business_type}
- Products: {products}
- Tone: {tone}
- Daily Goal: {daily_goal}
- Keywords: {keywords}

DATA:
- Tasks today: {tasks_str}
- Leads: {leads_str}

HISTORY OF LAST MESSAGES:
{recent_runs_str}

TASK:
Write a "good morning" message that fits this business.

Ask about today's tasks and priorities.
Ask if anything changed.
Add light motivation.
Follow the tone guidelines specified.
Avoid repeating the same previous messages - be natural and varied.
"""
    
    return prompt.strip()


def build_follow_up_prompt(
    business_profile,
    tasks,
    leads,
    sales=None,
    recent_runs=None
) -> str:
    """
    Build prompt for FOLLOW_UP agent - ask what happened with tasks/leads.
    
    Args:
        business_profile: BusinessProfile model instance
        tasks: List of Task model instances
        leads: List of Leads model instances
        sales: Optional list of Sales model instances
        recent_runs: Optional list of AgentRunLog instances
        
    Returns:
        Follow-up prompt string
    """
    business_profile_str = format_business_profile(business_profile)
    tasks_str = format_tasks(tasks)
    leads_str = format_leads(leads)
    sales_str = format_sales(sales) if sales else "No sales"
    recent_runs_str = format_recent_runs(recent_runs) if recent_runs else "No previous messages"
    
    prompt = f"""You are a CRM sales agent for a business, talking to one salesperson (user).
Do not mention that you are a language model.

BUSINESS PROFILE:
{business_profile_str}

DATA:
- Tasks: {tasks_str}
- Leads: {leads_str}
- Sales: {sales_str}

HISTORY OF LAST MESSAGES:
{recent_runs_str}

TASK:
Follow up with the salesperson on tasks and leads.

Ask:
- "What happened with {tasks_str}?"
- "Where did you reach with them?"
- "What's the latest update?"

If there are failed or overdue sales, ask why in a gentle and professional way.
Use a professional and friendly tone.
Avoid repeating the same questions from previous messages - be natural and varied.
"""
    
    return prompt.strip()


def build_closure_prompt(
    business_profile,
    tasks,
    leads,
    sales=None,
    recent_runs=None
) -> str:
    """
    Build prompt for CLOSURE agent - push to close deals / ask for commitment.
    
    Args:
        business_profile: BusinessProfile model instance
        tasks: List of Task model instances
        leads: List of Leads model instances
        sales: Optional list of Sales model instances
        recent_runs: Optional list of AgentRunLog instances
        
    Returns:
        Closure prompt string
    """
    business_profile_str = format_business_profile(business_profile)
    leads_str = format_leads(leads)
    sales_str = format_sales(sales) if sales else "No sales"
    recent_runs_str = format_recent_runs(recent_runs) if recent_runs else "No previous messages"
    
    prompt = f"""You are a CRM sales agent for a business, talking to one salesperson (user).
Do not mention that you are a language model.

BUSINESS PROFILE:
{business_profile_str}

DATA:
- Leads: {leads_str}
- Sales: {sales_str}

HISTORY OF LAST MESSAGES:
{recent_runs_str}

TASK:
Focus on closing deals.

Ask for specific actions:
- "Call customer {customer_name}"
- "Visit {customer_name}"
- "Send a quote to {customer_name}"

Encourage the salesperson to move from "thinking" to "closing".
Be respectful and realistic.
Use a professional tone.
Avoid repeating the same previous messages - be natural and varied.
"""
    
    return prompt.strip()


def build_nurture_prompt(
    business_profile,
    tasks,
    leads,
    sales=None,
    recent_runs=None
) -> str:
    """
    Build prompt for NURTURE agent - keep warm leads engaged, add value.
    
    Args:
        business_profile: BusinessProfile model instance
        tasks: List of Task model instances
        leads: List of Leads model instances
        sales: Optional list of Sales model instances
        recent_runs: Optional list of AgentRunLog instances
        
    Returns:
        Nurture prompt string
    """
    business_profile_str = format_business_profile(business_profile)
    leads_str = format_leads(leads)
    recent_runs_str = format_recent_runs(recent_runs) if recent_runs else "No previous messages"
    
    prompt = f"""You are a CRM sales agent for a business, talking to one salesperson (user).
Do not mention that you are a language model.

BUSINESS PROFILE:
{business_profile_str}

DATA:
- Leads: {leads_str}

HISTORY OF LAST MESSAGES:
{recent_runs_str}

TASK:
Focus on building relationships with leads.

Suggest:
- Sending helpful tips
- Follow-up messages
- Value-based content

Avoid pushing too hard for sales.
Tone: Friendly, building long-term trust.
Use a friendly and professional tone.
Avoid repeating the same previous messages - be natural and varied.
"""
    
    return prompt.strip()


def build_upsell_prompt(
    business_profile,
    tasks,
    leads,
    sales=None,
    recent_runs=None
) -> str:
    """
    Build prompt for UPSELL agent - suggest additional products to existing customers.
    
    Args:
        business_profile: BusinessProfile model instance
        tasks: List of Task model instances
        leads: List of Leads model instances
        sales: Optional list of Sales model instances
        recent_runs: Optional list of AgentRunLog instances
        
    Returns:
        Upsell prompt string
    """
    business_profile_str = format_business_profile(business_profile)
    products = ", ".join(business_profile.products) if business_profile and business_profile.products else "Not specified"
    sales_str = format_sales(sales) if sales else "No sales"
    recent_runs_str = format_recent_runs(recent_runs) if recent_runs else "No previous messages"
    
    prompt = f"""You are a CRM sales agent for a business, talking to one salesperson (user).
Do not mention that you are a language model.

BUSINESS PROFILE:
{business_profile_str}

DATA:
- Available Products: {products}
- Previous Sales: {sales_str}

HISTORY OF LAST MESSAGES:
{recent_runs_str}

TASK:
Focus on selling more to existing customers.

Use business products: {products}

Example: If customer bought Product A, suggest Product B or C.

Tone: Advisory, not pushy.
Be a consultant, not an aggressive seller.
Use a professional and polite tone.
Avoid repeating the same previous messages - be natural and varied.
"""
    
    return prompt.strip()


# Agent type to function mapping
AGENT_PROMPT_BUILDERS = {
    "REMINDER": build_reminder_prompt,
    "FOLLOW_UP": build_follow_up_prompt,
    "CLOSURE": build_closure_prompt,
    "NURTURE": build_nurture_prompt,
    "UPSELL": build_upsell_prompt,
}


def get_agent_prompt(
    agent_type: AgentType,
    business_profile,
    tasks,
    leads,
    sales=None,
    recent_runs=None
) -> str:
    """
    Get prompt for a specific agent type.
    
    Args:
        agent_type: Type of agent (REMINDER, FOLLOW_UP, CLOSURE, NURTURE, UPSELL)
        business_profile: BusinessProfile model instance
        tasks: List of Task model instances
        leads: List of Leads model instances
        sales: Optional list of Sales model instances
        recent_runs: Optional list of AgentRunLog instances
        
    Returns:
        Prompt string for the specified agent type
    """
    builder = AGENT_PROMPT_BUILDERS.get(agent_type)
    if not builder:
        raise ValueError(f"Unknown agent type: {agent_type}")
    
    return builder(business_profile, tasks, leads, sales, recent_runs)


# Legacy functions for backward compatibility
def get_morning_message_prompt(business_profile, tasks, leads) -> str:
    """Legacy function - maps to REMINDER agent."""
    return build_reminder_prompt(business_profile, tasks, leads, sales=None, recent_runs=None)


def get_followup_prompt(business_profile, tasks, sales) -> str:
    """Legacy function - maps to FOLLOW_UP agent."""
    return build_follow_up_prompt(business_profile, tasks, [], sales, recent_runs=None)


def get_system_prompt(business_profile) -> str:
    """
    Generate system prompt based on business profile.
    
    Args:
        business_profile: BusinessProfile model instance
        
    Returns:
        System prompt string for OpenAI API
    """
    base_prompt = """You are a CRM sales agent assistant for a business, having a natural conversation with a salesperson (user).
Do not mention that you are a language model or AI.

IMPORTANT INSTRUCTIONS:
- Have a natural, conversational dialogue - respond to what the user actually says
- Be dynamic and varied - never repeat the same greeting or response
- Ask follow-up questions based on the user's messages
- Provide helpful, actionable advice related to sales and CRM
- Be professional but friendly and approachable
- Adapt your responses to the context of the conversation"""
    
    if business_profile:
        profile_context = format_business_profile(business_profile)
        tone = business_profile.tone if business_profile.tone else "professional"
        base_prompt += f"""

BUSINESS CONTEXT:
{profile_context}

COMMUNICATION STYLE:
- Tone: {tone}
- Focus on the business type: {business_profile.business_type}
- Reference products/services when relevant: {', '.join(business_profile.products) if business_profile.products else 'N/A'}
- Keep daily goal in mind: {business_profile.daily_goal or 'Not specified'}
- Use relevant keywords naturally: {', '.join(business_profile.keywords) if business_profile.keywords else 'N/A'}

Adapt your communication style to match the specified tone while maintaining a natural conversation flow."""
    else:
        base_prompt += "\n\nUse a professional and friendly tone. Be helpful and conversational."
    
    base_prompt += "\n\nRemember: This is a real conversation. Respond naturally to what the user says, ask questions, provide insights, and help them with their sales tasks."
    
    return base_prompt


def get_message_prompt(user_message: str, context: str = "") -> str:
    """
    Generate user message prompt with optional context.
    
    Args:
        user_message: The user's message
        context: Additional context (e.g., task information, sales data)
        
    Returns:
        Formatted prompt string
    """
    # For chat interface, we want natural conversation
    # The user message is the main input - respond naturally to it
    
    if context:
        prompt = f"""Here is some relevant context about the salesperson's current situation:

{context}

Now, the salesperson sent you this message:
"{user_message}"

Respond naturally to their message. Use the context to provide relevant, helpful responses. Ask follow-up questions if appropriate. Be conversational and engaging."""
    else:
        # No context - just have a natural conversation
        prompt = f"""The salesperson sent you this message:
"{user_message}"

Respond naturally and helpfully. This is a conversation - engage with what they're saying, ask questions, provide insights, and help them with their sales work. Be dynamic and avoid generic responses."""
    
    return prompt


def get_task_analysis_prompt(task_title: str, task_details: str = "") -> str:
    """
    Generate prompt for analyzing and responding to tasks.
    
    Args:
        task_title: Title of the task
        task_details: Additional task details
        
    Returns:
        Prompt for task analysis
    """
    prompt = f"""Analyze the following task and provide a recommendation or response:

Task: {task_title}"""
    
    if task_details:
        prompt += f"\nDetails: {task_details}"
    
    prompt += "\n\nProvide a concise, actionable response or recommendation."
    
    return prompt


def get_sales_prompt(customer: str, product: str, sales_status: str) -> str:
    """
    Generate prompt for sales-related interactions.
    
    Args:
        customer: Customer name
        product: Product name
        sales_status: Current sales status
        
    Returns:
        Prompt for sales interaction
    """
    return f"""You are following up on a sales opportunity:
- Customer: {customer}
- Product: {product}
- Status: {sales_status}

Generate an appropriate follow-up message or action recommendation."""
