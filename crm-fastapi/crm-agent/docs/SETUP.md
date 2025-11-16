# Setup & Deployment Guide

## Table of Contents
1. [Local Development Setup](#local-development-setup)
2. [Docker Setup](#docker-setup)
3. [Environment Configuration](#environment-configuration)
4. [Database Setup](#database-setup)
5. [Running the Application](#running-the-application)
6. [Troubleshooting](#troubleshooting)

---

## Local Development Setup

### Prerequisites
- Python 3.10 or higher
- PostgreSQL 13+ (for database)
- pip (Python package manager)
- Git

### Step 1: Clone Repository
```bash
git clone https://github.com/husainf4l/crosscrm.git
cd crosscrm/crm-fastapi
```

### Step 2: Create Virtual Environment
```bash
python -m venv .venv

# On Windows
.venv\Scripts\activate

# On macOS/Linux
source .venv/bin/activate
```

### Step 3: Install Dependencies
```bash
pip install --upgrade pip
pip install -r crm-agent/requirements.txt
```

Or using pyproject.toml:
```bash
pip install -e .[dev]
```

### Step 4: Configure Environment
See [Environment Configuration](#environment-configuration) section

### Step 5: Initialize Database
```bash
cd crm-agent
python -m app.db.database  # Run migrations if exists
```

### Step 6: Run Application
```bash
cd crm-agent
export PYTHONPATH=.
python -m uvicorn app.main:app --reload --port 8001
```

Access the app at: `http://localhost:8001`

---

## Docker Setup

### Prerequisites
- Docker Desktop or Docker Engine
- Docker Compose

### Step 1: Build and Run with Docker Compose
```bash
# From project root
cd crm-fastapi
docker-compose -f docker/docker-compose.yml up -d
```

This will:
- Create PostgreSQL container with database
- Create FastAPI application container
- Set up networking between containers
- Expose port 8000 for the application

### Step 2: Access the Application
- Chat Interface: `http://localhost:8000/chat`
- API Docs: `http://localhost:8000/docs`
- ReDoc: `http://localhost:8000/redoc`

### Step 3: View Logs
```bash
docker-compose -f docker/docker-compose.yml logs -f app
```

### Step 4: Stop Containers
```bash
docker-compose -f docker/docker-compose.yml down
```

---

## Environment Configuration

### Create .env File

Create a file named `.env` in the project root:

```bash
# OpenAI Configuration
OPENAI_API_KEY=your_openai_api_key_here
OPENAI_MODEL=gpt-4

# Database Configuration
# Local PostgreSQL
DATABASE_URL=postgresql+asyncpg://user:password@localhost:5432/crm_db

# Docker PostgreSQL
# DATABASE_URL=postgresql+asyncpg://crm_user:crm_password@postgres:5432/crm_db

# Application Settings
DEBUG=False
APP_NAME=CRM Agent
APP_VERSION=1.0.0
```

### Environment Variables Reference

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `OPENAI_API_KEY` | Yes | - | Your OpenAI API key |
| `OPENAI_MODEL` | Yes | gpt-4 | Model to use (gpt-4, gpt-3.5-turbo) |
| `DATABASE_URL` | Yes | - | PostgreSQL connection string |
| `DEBUG` | No | False | Enable debug mode |
| `APP_NAME` | No | CRM Agent | Application name |
| `APP_VERSION` | No | 1.0.0 | Application version |

### Getting OpenAI API Key

1. Go to [OpenAI API Keys](https://platform.openai.com/api-keys)
2. Sign up or log in to your account
3. Click "Create new secret key"
4. Copy the key and save it securely
5. Add to `.env` file

### PostgreSQL Connection String Format

```
postgresql+asyncpg://username:password@host:port/database_name
```

Examples:
- Local: `postgresql+asyncpg://postgres:password@localhost:5432/crm_db`
- Docker: `postgresql+asyncpg://crm_user:crm_password@postgres:5432/crm_db`
- Remote (AWS RDS): `postgresql+asyncpg://user:pass@crm-db.xxxxxx.us-east-1.rds.amazonaws.com:5432/crm_db`

---

## Database Setup

### Local PostgreSQL Setup

#### On macOS (using Homebrew)
```bash
brew install postgresql@15
brew services start postgresql@15
createuser crm_user -P  # Enter password when prompted
createdb -U crm_user crm_db
```

#### On Linux (Ubuntu/Debian)
```bash
sudo apt-get install postgresql postgresql-contrib
sudo -u postgres psql
# In PostgreSQL shell:
CREATE USER crm_user WITH PASSWORD 'your_password';
CREATE DATABASE crm_db OWNER crm_user;
\q
```

#### On Windows
1. Download PostgreSQL installer from [postgresql.org](https://www.postgresql.org/download/windows/)
2. Run installer and follow prompts
3. Remember the password you set for `postgres` user
4. Open pgAdmin and create:
   - User: `crm_user` with password
   - Database: `crm_db` owned by `crm_user`

### Docker PostgreSQL (Recommended)

Docker Compose already provides this. Connection details:
- Host: `postgres` (inside Docker) or `localhost` (from host)
- Port: `5432`
- User: `crm_user`
- Password: `crm_password`
- Database: `crm_db`

### Initialize Schema

When the app starts for the first time, it will create necessary tables.

To manually create tables:
```bash
cd crm-agent
python -c "from app.db.database import init_db; import asyncio; asyncio.run(init_db())"
```

---

## Running the Application

### Development Mode (with auto-reload)

```bash
cd crm-agent
export PYTHONPATH=.
python -m uvicorn app.main:app --reload --port 8001
```

### Production Mode

```bash
cd crm-agent
python -m uvicorn app.main:app --host 0.0.0.0 --port 8000 --workers 4
```

### Using Gunicorn (Production)

```bash
pip install gunicorn
gunicorn app.main:app -w 4 -k uvicorn.workers.UvicornWorker --bind 0.0.0.0:8000
```

### With PM2 (Node.js Process Manager)

```bash
npm install -g pm2
pm2 start "python -m uvicorn app.main:app" --name "crm-agent"
pm2 save
pm2 startup
```

---

## Testing the API

### Using cURL

```bash
# Health check
curl http://localhost:8001/

# List agents
curl http://localhost:8001/api/agents/list

# Send message
curl -X POST http://localhost:8001/api/chat/message \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": 1,
    "message": "Hello!"
  }'

# Run agent
curl -X POST http://localhost:8001/api/agents/run \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": 1,
    "agent_type": "REMINDER"
  }'
```

### Using Python

```python
import requests

url = "http://localhost:8001"

# Send message
response = requests.post(f"{url}/api/chat/message", json={
    "user_id": 1,
    "message": "Hello, what can you do?"
})
print(response.json())

# Run agent
response = requests.post(f"{url}/api/agents/run", json={
    "user_id": 1,
    "agent_type": "REMINDER"
})
print(response.json())
```

### Using Swagger UI

Open browser to: `http://localhost:8001/docs`

Interactive API documentation and testing interface.

---

## Troubleshooting

### Issue: Database Connection Failed

**Error**: `password authentication failed for user "user"`

**Solution**:
1. Verify PostgreSQL is running
2. Check credentials in `.env` file
3. Create user/database if missing
```bash
sudo -u postgres psql
CREATE USER crm_user WITH PASSWORD 'your_password';
CREATE DATABASE crm_db OWNER crm_user;
```

### Issue: OpenAI API Key Invalid

**Error**: `Invalid API Key`

**Solution**:
1. Verify key is correct in `.env`
2. Check key has not expired
3. Ensure key has API access enabled
4. Try creating a new key

### Issue: Port Already in Use

**Error**: `Address already in use`

**Solution**:
```bash
# Find process using port 8001
lsof -i :8001

# Kill process (Linux/macOS)
kill -9 <PID>

# On Windows
netstat -ano | findstr :8001
taskkill /PID <PID> /F

# Or use different port
python -m uvicorn app.main:app --port 8002
```

### Issue: Module Not Found

**Error**: `ModuleNotFoundError: No module named 'app'`

**Solution**:
```bash
# Ensure PYTHONPATH is set
export PYTHONPATH=.

# Or run from correct directory
cd crm-agent
python -m uvicorn app.main:app --reload
```

### Issue: AsyncIO Event Loop Error

**Error**: `RuntimeError: Event loop is closed`

**Solution**:
```bash
# Update Python to latest version
python --version

# Reinstall SQLAlchemy
pip install --upgrade sqlalchemy

# Or use with uvicorn
python -m uvicorn app.main:app --reload
```

---

## Performance Tips

1. **Use Connection Pooling**: SQLAlchemy async handles this automatically
2. **Enable Redis**: Cache frequent queries
3. **Use CDN**: For static assets in production
4. **Database Indexing**: Add indexes on frequently queried columns
5. **Rate Limiting**: Implement to prevent abuse
6. **Monitoring**: Set up application monitoring with Sentry or similar

---

## Security Checklist

- [ ] Never commit `.env` file to Git
- [ ] Use strong passwords for database
- [ ] Restrict CORS to specific domains in production
- [ ] Implement JWT authentication
- [ ] Use HTTPS in production
- [ ] Rotate API keys regularly
- [ ] Enable database backups
- [ ] Monitor application logs

---

## Next Steps

1. Create business profile for testing
2. Run agents manually via `/api/agents/run`
3. Test chat interface at `/chat`
4. Set up scheduled agent runs
5. Configure production deployment
