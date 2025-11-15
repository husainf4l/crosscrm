# Cross CRM

AI-powered CRM and Sales Management Application built with FastAPI.

## Features

- **Contact & Company Management** - Full CRUD operations for contacts and companies
- **Sales Pipeline** - Visual pipeline with deal stages and kanban board
- **Activity Tracking** - Log calls, meetings, emails, notes, and tasks
- **Salesperson Management** - Track individual performance and metrics
- **Analytics Dashboard** - Sales metrics, trends, and performance analytics
- **Market Intelligence** - Market trends, competitor analysis, and insights
- **AI Agents** - Sales assistance, forecasting, insights, and recommendations
- **Accounting Integration** - Import sales history from accounting systems

## Tech Stack

- **Backend**: FastAPI with SQLAlchemy ORM
- **Database**: PostgreSQL
- **Frontend**: Jinja2 templates with Bootstrap 5
- **AI**: OpenAI API (optional) for AI agents
- **Authentication**: Session-based with password hashing

## Setup

1. **Create virtual environment:**
```bash
uv venv
source .venv/bin/activate
```

2. **Install dependencies:**
```bash
uv pip install -r requirements.txt
```

3. **Configure environment:**
Create a `.env` file (or use the default config):
```
DATABASE_URL=postgresql://tt55oo77:cross@149.200.251.12:5432/husain
SECRET_KEY=your-secret-key-here
OPENAI_API_KEY=your-openai-key-optional
DEBUG=True
```

4. **Run database migrations:**
```bash
alembic upgrade head
```

Or create tables directly:
```bash
python -c "from db.database import engine, Base; from db.models import *; Base.metadata.create_all(bind=engine)"
```

5. **Seed sample data (optional):**
```bash
python seed_data.py
```

This creates:
- Admin user: `admin@crosscrm.com` / `admin123`
- Salesperson: `sales@crosscrm.com` / `sales123`
- Sample companies, contacts, deals, and activities

## Run

```bash
uvicorn main:app --reload
```

The application will be available at:
- **Web Interface**: `http://localhost:8000`
- **API Documentation**: `http://localhost:8000/docs`
- **Health Check**: `http://localhost:8000/health`

## Project Structure

```
cross/
├── main.py              # FastAPI application entry point
├── config.py            # Configuration settings
├── alembic/             # Database migrations
├── db/
│   ├── database.py      # Database connection
│   └── models/          # SQLAlchemy models
├── schema/              # Pydantic schemas
├── services/            # Business logic services
├── routes/              # Route handlers
├── agent/               # AI agent implementations
├── templates/           # Jinja2 HTML templates
└── static/              # CSS, JS, images
```

## Main Routes

- `/` - Redirects to dashboard or login
- `/auth/login` - Login page
- `/auth/register` - Registration page
- `/contacts` - Contact list
- `/deals` - Deal list
- `/deals/pipeline` - Pipeline kanban view
- `/activities` - Activity timeline
- `/salespeople` - Salesperson list
- `/analytics/dashboard` - Main dashboard
- `/market/insights` - Market intelligence

## Database Models

- **User** - Users/salespeople with roles
- **Contact** - Customer contacts
- **Company** - Companies/organizations
- **Deal** - Sales opportunities/deals
- **Activity** - Calls, meetings, emails, notes
- **Task** - Tasks and follow-ups
- **Product** - Product catalog
- **MarketData** - Market intelligence data

## AI Agents

- **Sales Assistant** - Conversation starters, email sentiment, follow-up suggestions
- **Forecasting Agent** - Sales forecasting, deal close prediction
- **Insight Agent** - Market insights, competitor analysis
- **Recommendation Agent** - Next best actions, upsell opportunities

## Development

To run in development mode with auto-reload:
```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

## License

Copyright 2024 Cross CRM
