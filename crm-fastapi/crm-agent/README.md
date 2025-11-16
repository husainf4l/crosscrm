# CRM Agent - FastAPI with LLM Integration

An intelligent CRM agent powered by GPT-4 that generates contextual business communications via a modern web interface. Built with FastAPI, SQLAlchemy, and OpenAI API.

## ✨ Features

- **🤖 AI-Powered Agents**: 5 specialized agents (Reminder, Follow-up, Closure, Nurture, Upsell)
- **⏰ Automated Scheduling**: Scheduled agent execution via APScheduler
- **💬 Real-time Chat Interface**: Modern WebUI for agent interaction
- **📊 Business Profiles**: Customizable business context for personalized communications
- **🗄️ Database Persistence**: PostgreSQL with async SQLAlchemy ORM
- **📚 RESTful API**: Comprehensive API documentation with Swagger UI
- **🐳 Docker Ready**: Production-ready Docker and Docker Compose setup
- **⚡ Async Performance**: Full async/await implementation
- **🏗️ Clean Architecture**: Modular design with clear separation of concerns

## Setup

1. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Create a `.env` file in the project root (copy from `.env.example`):
```env
# Database Configuration
DATABASE_URL=postgresql+asyncpg://user:password@localhost:5432/crm

# OpenAI API Configuration
OPENAI_API_KEY=your_openai_api_key_here
OPENAI_MODEL=gpt-4o-mini

# GraphQL Backend (crm-backend) Configuration
GRAPHQL_URL=http://localhost:5000/graphql
GRAPHQL_API_KEY=your_graphql_api_key_here

# JWT Authentication (if needed for GraphQL)
JWT_SECRET=your_jwt_secret_here
JWT_ALGORITHM=HS256

# Application Configuration
DEBUG=False
API_VERSION=1.0.0
```

3. Run the application:
```bash
uvicorn app.main:app --reload
```

4. Open the chat interface:
   - Navigate to `http://localhost:8000/chat` in your browser
   - Or use the API endpoints directly

## Project Structure

```
crm-agent/
├── app/
│   ├── main.py              # FastAPI application entry point
│   ├── core/                # Shared utilities
│   │   ├── exceptions.py    # Custom exceptions
│   │   ├── middleware.py    # Custom middleware
│   │   ├── dependencies.py  # Dependency injection
│   │   └── graphql_client.py # GraphQL client for crm-backend
│   ├── modules/             # Domain modules (following crm-backend pattern)
│   │   ├── agent/           # Agent operations
│   │   │   ├── dto/         # Data Transfer Objects
│   │   │   └── services/    # Service layer
│   │   └── chat/            # Chat operations
│   │       ├── dto/
│   │       └── services/
│   ├── agent/               # LLM agent logic
│   │   ├── orchestrator.py  # Main agent orchestration
│   │   ├── scheduler.py     # Task scheduling
│   │   └── prompts.py       # LLM prompts
│   ├── db/                  # Database models and CRUD
│   └── config/              # Configuration settings
├── static/                  # Frontend chat interface
├── requirements.txt
├── .env.example             # Environment variables template
└── README.md
```

## Architecture

This project follows the same architectural patterns as `crm-backend`:

- **Modular Structure**: Code organized into domain modules
- **Service Layer**: Business logic in services with interfaces
- **DTOs**: Data Transfer Objects with validation
- **Dependency Injection**: Services injected via FastAPI's `Depends()`
- **Error Handling**: Custom exceptions with proper HTTP status codes
- **GraphQL Integration**: Client for connecting to crm-backend GraphQL API

## What is Business Profile?

A **Business Profile** is a configuration stored in the database that defines how the LLM agent should behave for a specific business. It contains:

- **business_type**: The type of business (e.g., "cheese", "chicken", "cars", "real estate")
- **products**: List of products/services offered (e.g., `["cheese", "labaneh", "milk"]`)
- **tone**: Communication style (e.g., "friendly", "strict", "professional")
- **daily_goal**: Daily objective (e.g., "sell 20 cheese blocks")
- **keywords**: Important terms for the agent to use (e.g., `["target", "follow-up", "closing"]`)

### Example Business Profile

```json
{
  "business_type": "cheese",
  "products": ["cheese", "labaneh", "milk"],
  "tone": "friendly",
  "daily_goal": "sell 20 cheese blocks",
  "keywords": ["target", "follow-up", "closing"]
}
```

The agent uses this profile to:
- Generate messages that match the business context
- Use appropriate language and terminology
- Focus on relevant products and goals
- Adapt communication style to the specified tone

## How LLM Generates the Tone

The LLM generates messages with the appropriate tone through **dynamic prompt engineering**:

### 1. **Prompt Construction**
The system builds prompts that include:
- Business profile data (type, products, tone, goals, keywords)
- Current CRM data (tasks, leads, sales)
- Specific instructions about tone and style

### 2. **Tone Adaptation**
The LLM receives explicit instructions like:
```
Adapt your communication tone to match: friendly
Focus on the daily goal: sell 20 cheese blocks
Use the provided business information and keywords to give relevant responses.
```

### 3. **Context-Aware Generation**
For different scenarios:
- **Morning messages**: Asks about tasks and priorities with appropriate tone
- **Follow-up messages**: Asks about sales outcomes professionally

### 4. **Example Flow**

```python
# System prompt includes:
BUSINESS PROFILE:
- Type: cheese
- Products: cheese, labaneh, milk
- Tone: friendly
- Daily Goal: sell 20 cheese blocks
- Keywords: target, follow-up, closing

# LLM generates message matching:
- Friendly tone
- Cheese business context
- Daily goal awareness
- Appropriate keywords
```

The LLM interprets these instructions and generates messages that naturally match the specified tone while staying contextually relevant.

## How to Add Any Business Type

The system is designed to work with **any business type** without code changes. Here's how:

### 1. **Create Business Profile via API**

```bash
POST /business-profile/{user_id}
Content-Type: application/json

{
  "business_type": "real estate",
  "products": ["apartments", "villas", "commercial spaces"],
  "tone": "professional",
  "daily_goal": "schedule 5 property viewings",
  "keywords": ["viewing", "contract", "deposit", "closing"]
}
```

### 2. **The Agent Automatically Adapts**

Once the profile is created, the agent will:
- Use "real estate" terminology in messages
- Focus on apartments, villas, and commercial spaces
- Maintain a professional tone
- Reference property viewings and contracts
- Use keywords like "viewing", "contract", "deposit"

### 3. **No Code Changes Required**

The system uses:
- **Generic data models**: No hardcoded business logic
- **Dynamic prompts**: LLM interprets any business type
- **Flexible fields**: Status, stage, and other fields are generic strings

### 4. **Example: Adding a New Business**

```python
# For a "chicken" business:
{
  "business_type": "chicken",
  "products": ["whole chicken", "chicken pieces", "marinated chicken"],
  "tone": "casual",
  "daily_goal": "deliver 50 orders",
  "keywords": ["order", "delivery", "fresh"]
}

# The agent will automatically:
# - Talk about chicken products
# - Use casual language
# - Focus on delivery goals
# - Reference orders and freshness
```

The LLM's flexibility allows it to adapt to any business context without requiring code modifications.

## Chat Interface

The CRM Agent provides a RESTful chat interface for interacting with the LLM-powered agent.

### 1. **Send a Message**

```bash
POST /chat/message
Content-Type: application/json

{
  "user_id": 1,
  "message": "What are my tasks for today?"
}
```

**Response:**
```json
{
  "user_id": 1,
  "message": "Based on your business profile, here are your tasks...",
  "agent_type": "AGENT_RESPONSE"
}
```

### 2. **Get Chat History**

```bash
GET /chat/history/{user_id}?limit=50
```

**Response:**
```json
{
  "user_id": 1,
  "messages": [
    {
      "id": 1,
      "agent_type": "USER_MESSAGE",
      "message": "What are my tasks?",
      "created_at": "2024-01-15T09:00:00"
    },
    {
      "id": 2,
      "agent_type": "AGENT_RESPONSE",
      "message": "Here are your tasks...",
      "created_at": "2024-01-15T09:00:01"
    }
  ]
}
```

### 3. **Automated Agents**

The scheduler runs automated agents that generate messages and log them to the chat history:

#### **Morning Agent (9:00 AM)**
- Fetches business profile, today's tasks, and leads
- Generates a "good morning" message
- Asks about tasks and priority changes
- Logs message to chat history

#### **Follow-up Agent (1:00 PM)**
- Fetches business profile, tasks, and sales updates
- Generates a follow-up message
- Asks what happened, if customer bought, why if not
- Uses professional and friendly tone
- Logs message to chat history

### 4. **Message Flow**

```
User Message → API → LLM Processing → Agent Response → Chat History
     ↓                ↓            ↓                  ↓
  Frontend      FastAPI      Business Profile    Database
```

### 5. **Technical Implementation**

```python
# Chat endpoint (main.py)
@app.post("/chat/message")
async def send_chat_message(body: ChatMessageRequest, db: AsyncSession):
    orchestrator = AgentOrchestrator()
    response = await orchestrator.process_user_message(
        db=db,
        user_id=body.user_id,
        message=body.message
    )
    # Log both user message and agent response
    await crud.log_agent_run(...)
    return response

# Scheduler (scheduler.py)
scheduler.add_job(
    reminder_wrapper,
    trigger=CronTrigger(hour=9, minute=0),
    args=[1]  # user_id
)
```

### 6. **Automation Schedule**

| Time | Agent | Purpose |
|------|-------|---------|
| 9:00 AM | Morning Agent | Good morning message, task check |
| 1:00 PM | Follow-up Agent | Sales follow-up, status check |

All messages are:
- **Context-aware**: Based on business profile and CRM data
- **Tone-appropriate**: Matches business communication style
- **Natural**: Uses appropriate language and tone for the business
- **Goal-focused**: References daily goals and priorities
- **Logged**: Stored in database for chat history

## Chat Interface

The application includes a modern, responsive web-based chat interface accessible at `/chat`.

### Features
- **Real-time messaging** with the CRM agent
- **Quick action buttons** for common agent types (Reminder, Follow-up, Closure, Nurture, Upsell)
- **Chat history** - View and load previous conversations
- **User management** - Switch between different user IDs
- **Responsive design** - Works on desktop, tablet, and mobile devices
- **Accessibility** - ARIA labels, keyboard navigation, screen reader support
- **Error handling** - Clear error messages for failed requests
- **Loading states** - Visual feedback during API calls

### Access the Interface
1. Start the server: `uvicorn app.main:app --reload`
2. Open your browser: `http://localhost:8000/chat`
3. Start chatting!

### Testing
A comprehensive test suite is available at `test_chat_interface.html`:
- Manual testing checklist
- API endpoint testing
- Best practices verification
- Live interface preview

## API Endpoints

### Chat Interface
- `GET /chat` - Serve the chat interface (HTML)
- `POST /chat/message` - Send a message to the agent
- `GET /chat/history/{user_id}` - Get chat history for a user

### Business Profile
- `POST /business-profile/{user_id}` - Create/update business profile
- `GET /business-profile/{user_id}` - Get business profile

### Tasks & Progress
- `GET /tasks/today/{user_id}` - Get today's tasks
- `GET /progress/{user_id}` - Get user progress

### Agent Control
- `POST /agents/run` - Manually trigger an agent
- `GET /agents/list` - List available agent types

### Health Check
- `GET /` - Health check endpoint

## How It Works - Summary

1. **Business Profile** defines the agent's behavior and context
2. **LLM** generates tone-appropriate messages using dynamic prompts
3. **Any Business Type** can be added via API without code changes
4. **Chat Interface** provides RESTful API for real-time interactions
5. **Automated Scheduling** generates messages at specific times
6. **Chat History** stores all messages for context and retrieval

The system is fully flexible and adapts to any business through configuration rather than code changes.
