# CRM Agent Architecture

## Overview

The CRM Agent is a FastAPI-based application that uses LLM (Large Language Models) to generate intelligent CRM communications. It features:

- **AI-Powered Agent Orchestration**: Uses OpenAI GPT-4 to generate contextual business communications
- **Scheduled Execution**: APScheduler runs agents at specific intervals for automated outreach
- **Database Persistence**: PostgreSQL stores user profiles, agent logs, and communication history
- **Real-time Chat Interface**: WebUI for testing and interacting with agents
- **Modular Design**: Clean separation of concerns with dedicated modules

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Frontend (Web UI)                      │
│         (HTML/CSS/JS - Static Files in /static)            │
└────────────────┬────────────────────────────────────────────┘
                 │ HTTP/REST
┌─────────────────┴────────────────────────────────────────────┐
│                    FastAPI Application                       │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         API Routes Layer (app/api/routes.py)        │   │
│  │  - Chat endpoints                                   │   │
│  │  - Agent control                                    │   │
│  │  - Profile management                              │   │
│  └────────────┬──────────────────────────────────────┘   │
│               │                                           │
│  ┌────────────▼──────────────────────────────────────┐   │
│  │  Orchestration Layer (app/agent/orchestrator.py)  │   │
│  │  - Agent coordination                             │   │
│  │  - Message processing                            │   │
│  │  - Context assembly                              │   │
│  └────────────┬──────────────────────────────────────┘   │
│               │                                           │
│  ┌────────────▼──────────────────────────────────────┐   │
│  │   AI Agent Layer (app/agent/)                      │   │
│  │  - Prompt templates (prompts.py)                  │   │
│  │  - Scheduler (scheduler.py)                       │   │
│  │  - OpenAI Integration                             │   │
│  └────────────┬──────────────────────────────────────┘   │
│               │                                           │
│  ┌────────────▼──────────────────────────────────────┐   │
│  │   Data Access Layer (app/db/)                      │   │
│  │  - Models (models.py)                             │   │
│  │  - CRUD operations (crud.py)                      │   │
│  │  - Database connection (database.py)              │   │
│  └────────────┬──────────────────────────────────────┘   │
│               │                                           │
└───────────────┼───────────────────────────────────────────┘
                │ SQL
┌───────────────▼───────────────────────────────────────────┐
│          PostgreSQL Database                              │
│  - User profiles                                          │
│  - Business configurations                               │
│  - Agent logs & communication history                    │
│  - Tasks & schedules                                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Module Descriptions

### `/app/api/` - API Routes
**Purpose**: Define all HTTP endpoints and request/response handling

**Key Files**:
- `routes.py`: All REST endpoints for chat, agents, profiles

**Endpoints Provided**:
- Chat messaging and history
- Agent control and listing
- Business profile management
- Task retrieval
- Progress tracking

---

### `/app/agent/` - AI Agent Logic
**Purpose**: Core AI agent functionality and orchestration

**Key Files**:
- `orchestrator.py`: Coordinates agent operations, assembles context
- `prompts.py`: Defines agent types and system prompts
- `scheduler.py`: APScheduler integration for automated agent runs

**Agent Types**:
1. **REMINDER**: Daily reminder messages (09:00)
2. **FOLLOW_UP**: Follow-up communications (13:00)
3. **CLOSURE**: Deal closure attempts (16:00)
4. **NURTURE**: Lead nurturing (11:00, every 2 days)
5. **UPSELL**: Upsell opportunities (10:00, Mondays)

---

### `/app/db/` - Data Layer
**Purpose**: Database models and data access operations

**Key Files**:
- `models.py`: SQLAlchemy ORM models
- `database.py`: AsyncIO database configuration and session management
- `crud.py`: Create, Read, Update, Delete operations

**Tables**:
- Users
- BusinessProfiles
- AgentRuns (logs)
- Tasks
- Communications

---

### `/app/config/` - Configuration
**Purpose**: Application settings and environment variables

**Key Files**:
- `settings.py`: Loads and validates environment configuration

**Configured Via**: `.env` file with variables:
- `DATABASE_URL`: PostgreSQL connection string
- `OPENAI_API_KEY`: API key for OpenAI
- `OPENAI_MODEL`: Model selection (gpt-4, etc.)
- `DEBUG`: Debug mode toggle

---

### `/app/core/` - Core Infrastructure
**Purpose**: Shared utilities and middleware

**Key Files**:
- `middleware.py`: CORS and request/response handlers

---

### `/static/` - Frontend
**Purpose**: User-facing web interface

**Files**:
- `index.html`: Chat interface layout
- `app.js`: Frontend logic and API communication
- `styles.css`: UI styling

---

## Data Flow

### 1. Chat Message Flow
```
User Input (UI)
    ↓
POST /chat/message
    ↓
ChatMessageRequest validation
    ↓
Orchestrator.process_user_message()
    ↓
Fetch business profile (context)
    ↓
Generate prompt with context
    ↓
Call OpenAI API (gpt-4)
    ↓
Log message to database
    ↓
ChatResponse to frontend
    ↓
Display in UI
```

### 2. Agent Execution Flow
```
Scheduler triggers at time T
    ↓
Find eligible users
    ↓
For each user:
    ├─ Fetch business profile
    ├─ Get recent context (tasks, progress)
    ├─ Generate agent prompt
    ├─ Call OpenAI API
    ├─ Create communication record
    └─ Log agent run
    ↓
Completed
```

---

## Database Schema (Simplified)

```
BusinessProfile
├── id (PK)
├── user_id (FK)
├── business_type
├── products (JSON)
├── tone
├── daily_goal
└── keywords (JSON)

AgentRun
├── id (PK)
├── user_id (FK)
├── agent_type
├── message
├── created_at
└── status

Task
├── id (PK)
├── user_id (FK)
├── title
├── status
└── due_date
```

---

## Key Technologies

| Component | Technology | Version |
|-----------|------------|---------|
| **Framework** | FastAPI | 0.100+ |
| **Server** | Uvicorn | 0.22+ |
| **Database** | PostgreSQL | 13+ |
| **ORM** | SQLAlchemy | 2.0+ |
| **AI/LLM** | OpenAI (gpt-4) | - |
| **Async Driver** | asyncpg | 0.28+ |
| **Task Scheduling** | APScheduler | 3.10+ |
| **Language** | Python | 3.10+ |

---

## Deployment Architecture

### Local Development
```
Python venv → FastAPI → SQLite/PostgreSQL Local
```

### Docker Containerized
```
Docker Container
├── Python 3.12
├── FastAPI app
└── Connects to → PostgreSQL Container
```

### Production (Recommended)
```
Load Balancer
    ↓
Multiple FastAPI Instances (K8s/Docker)
    ↓
PostgreSQL Database (Managed/RDS)
    ↓
Object Storage (S3 for files)
```

---

## Security Considerations

1. **Environment Variables**: Sensitive data in `.env` (not in git)
2. **CORS**: Currently open - restrict in production
3. **Authentication**: To be implemented (JWT recommended)
4. **Database**: Use strong passwords and encryption
5. **API Keys**: Never commit OpenAI keys to repository

---

## Performance Optimization

1. **Async Operations**: Full async/await throughout
2. **Connection Pooling**: SQLAlchemy async session management
3. **Caching**: Consider Redis for profile/context caching
4. **Rate Limiting**: To be implemented
5. **Batch Processing**: Agents process users efficiently via scheduler

---

## Error Handling

- Try-catch blocks around database operations
- Graceful degradation when database unavailable
- Detailed error logging for debugging
- User-friendly error responses via HTTP

---

## Future Enhancements

1. **WebSocket Support**: Real-time chat updates
2. **Vector DB**: Semantic search for similar past communications
3. **Multi-LLM Support**: Support multiple AI models
4. **Advanced Analytics**: Campaign performance tracking
5. **Integrations**: CRM systems, email providers, Slack
6. **Advanced Scheduling**: Cron-like rules for triggers
7. **A/B Testing**: Test different agent strategies
