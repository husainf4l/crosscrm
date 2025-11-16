# Quick Reference Guide

## 🚀 Quick Start Commands

### Local Development (5 min setup)
```bash
# Navigate to project
cd crosscrm/crm-fastapi/crm-agent

# Create and activate virtual environment
python -m venv .venv
source .venv/bin/activate  # macOS/Linux
.venv\Scripts\activate     # Windows

# Install dependencies
pip install -r requirements.txt

# Set up environment
echo "OPENAI_API_KEY=your_key_here" > ../.env
echo "DATABASE_URL=postgresql+asyncpg://user:pass@localhost:5432/crm_db" >> ../.env
echo "DEBUG=False" >> ../.env

# Run application
export PYTHONPATH=.
python -m uvicorn app.main:app --reload --port 8001

# Access
# Chat: http://localhost:8001/chat
# Docs: http://localhost:8001/docs
```

### Docker Setup (1 command)
```bash
cd crosscrm/crm-fastapi
docker-compose -f docker/docker-compose.yml up -d
# Access: http://localhost:8000/chat
```

---

## 📋 API Endpoints Quick Reference

### Chat
```bash
# Send message
curl -X POST http://localhost:8001/api/chat/message \
  -H "Content-Type: application/json" \
  -d '{"user_id": 1, "message": "Hello!"}'

# Get history
curl http://localhost:8001/api/chat/history/1
```

### Agents
```bash
# List agents
curl http://localhost:8001/api/agents/list

# Run agent
curl -X POST http://localhost:8001/api/agents/run \
  -H "Content-Type: application/json" \
  -d '{"user_id": 1, "agent_type": "REMINDER"}'
```

---

## 🔧 Essential Configuration

### `.env` File (Required)
```bash
# Get from https://platform.openai.com/api-keys
OPENAI_API_KEY=sk-...

# PostgreSQL connection
DATABASE_URL=postgresql+asyncpg://user:password@localhost:5432/crm_db

# Optional
DEBUG=False
OPENAI_MODEL=gpt-4
```

---

## 📚 Documentation Map

| Document | Purpose |
|----------|---------|
| `README.md` | Project overview and features |
| `QUICK_START.md` | This quick reference |
| `docs/SETUP.md` | Detailed installation guide |
| `docs/API.md` | Complete endpoint documentation |
| `docs/ARCHITECTURE.md` | System design and architecture |

---

## ✅ Verification Checklist

- [ ] `.env` file configured with OpenAI API key
- [ ] PostgreSQL running and connection string set
- [ ] Python dependencies installed
- [ ] Application starts without errors
- [ ] Chat interface accessible at http://localhost:8001/chat
- [ ] API docs visible at http://localhost:8001/docs
```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent
source venv/bin/activate
python check_openai_setup.py
```

If you see ✅, you're all set! The server will auto-reload.

## Test It Out

1. Open: http://localhost:8001/chat
2. Send a message: "Hello" or "What are my tasks?"
3. Enjoy natural, dynamic LLM responses! 🎉

## What Changed?

The LLM now:
- ✅ Responds naturally to what you say
- ✅ Asks relevant follow-up questions
- ✅ Provides contextual insights
- ✅ Never repeats the same greeting
- ✅ Uses conversation history for context

## Need Help?

See `SETUP_OPENAI.md` for detailed instructions and troubleshooting.

