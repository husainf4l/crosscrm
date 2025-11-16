# 📚 PROJECT STRUCTURE REFERENCE GUIDE

## Quick Navigation Map

### 🎯 For Different Roles/Tasks

#### Developer Starting Out
```
1. README.md                 (What is this project?)
2. CLEAN_STRUCTURE.md        (How is it organized?)
3. docs/guides/QUICK_START.md (How do I start?)
4. app/                      (Start coding here)
```

#### Operations/DevOps Engineer
```
1. docs/deployment/DEPLOYMENT_QUICK_START.md  (How do I deploy?)
2. docs/SETUP.md                               (Installation)
3. docker/docker-compose.yml                   (Container setup)
4. setup_postgresql.sh                         (Database setup)
```

#### API Integrator
```
1. docs/API.md                      (What endpoints exist?)
2. docs/ARCHITECTURE.md             (How does it work?)
3. app/api/routes.py                (API implementation)
4. tests/integration/test_endpoints.py (See it in action)
```

#### Project Manager/Team Lead
```
1. README.md                 (Project overview)
2. CLEAN_STRUCTURE.md        (Team structure info)
3. docs/ARCHITECTURE.md      (Technical overview)
4. docs/COMPLETION_SUMMARY.md (Project status)
```

#### Security/Compliance
```
1. docs/PRODUCTION_DEPLOYMENT.md  (Security section)
2. .env.production                (Secrets management)
3. docker/Dockerfile              (Container security)
4. docs/SETUP.md                  (Authorization setup)
```

---

## 📁 Complete Folder Breakdown

### `app/` - Application Code (Main Focus)
```
app/
├── __init__.py
├── main.py                  ← FastAPI entry point
│
├── api/                     ← REST API routes
│   ├── __init__.py
│   └── routes.py            ← All endpoints defined here
│
├── modules/                 ← Feature modules (best practice)
│   ├── __init__.py
│   ├── agent/               ← Agent feature
│   │   ├── __init__.py
│   │   ├── dto/             ← Data models
│   │   │   ├── __init__.py
│   │   │   └── agent_dto.py
│   │   └── services/        ← Business logic
│   │       ├── __init__.py
│   │       ├── agent_service.py
│   │       └── graphql_data_service.py
│   │
│   └── chat/                ← Chat feature
│       ├── __init__.py
│       ├── dto/             ← Data models
│       │   ├── __init__.py
│       │   └── chat_dto.py
│       └── services/        ← Business logic
│           ├── __init__.py
│           └── chat_service.py
│
├── core/                    ← Core infrastructure
│   ├── __init__.py
│   ├── middleware.py        ← CORS, error handling
│   ├── exceptions.py        ← Custom exception classes
│   ├── dependencies.py      ← FastAPI dependencies
│   └── graphql_client.py    ← GraphQL integration
│
├── config/                  ← Configuration
│   ├── __init__.py
│   └── settings.py          ← Environment & app settings
│
├── db/                      ← Database layer
│   ├── __init__.py
│   ├── database.py          ← DB connection setup
│   ├── models.py            ← SQLAlchemy ORM models
│   ├── crud.py              ← CRUD operations
│   └── models/              ← Model base classes
│       ├── __init__.py
│       └── base_model.py
│
├── agent/                   ← Legacy (being phased out)
│   ├── __init__.py
│   ├── orchestrator.py      ← Agent orchestration
│   ├── prompts.py           ← LLM prompts
│   └── scheduler.py         ← Task scheduling
│
└── integrations/            ← 3rd party integrations
    └── __init__.py
```

### `tests/` - Test Suite
```
tests/
├── __init__.py
├── conftest.py              ← Pytest configuration
│
├── unit/                    ← Unit tests
│   ├── __init__.py
│   └── services/
│       ├── __init__.py
│       ├── test_agent_service.py
│       └── test_chat_service.py
│
└── integration/             ← Integration tests
    ├── __init__.py
    ├── test_endpoints.py
    └── test_graphql_integration.py
```

### `docs/` - Documentation (Organized!)
```
docs/
├── API.md                   ← API endpoint reference (START HERE for API)
├── ARCHITECTURE.md          ← System design & components (START HERE for design)
├── SETUP.md                 ← Installation guide (START HERE to install)
├── PRODUCTION_DEPLOYMENT.md ← Production procedures (START HERE for prod)
├── RESTRUCTURE.md           ← Restructuring summary
├── COMPLETION_SUMMARY.md    ← Project status
│
├── deployment/              ← Deployment documentation
│   ├── DEPLOYMENT_README.md
│   ├── DEPLOYMENT_QUICK_START.md
│   ├── DEPLOYMENT_STATUS.md
│   ├── DEPLOYMENT_CHECKLIST.md
│   ├── DEPLOYMENT_MAP.md
│   ├── DEPLOYMENT_COMPLETE.md
│   └── DEPLOYMENT_FILES.txt
│
├── guides/                  ← Quick start guides
│   └── QUICK_START.md
│
└── archive/                 ← Legacy documentation (for reference only)
    ├── BACKEND_RESTRUCTURE.md
    ├── SETUP_BACKEND.md
    ├── SETUP_OPENAI.md
    ├── INTEGRATION_SUMMARY.md
    ├── LLM_*.md
    ├── ERROR_FIX*.md
    ├── REFACTORING_SUMMARY.md
    ├── TESTING.md
    └── TEST_RESULTS.md
```

### `docker/` - Container Configuration
```
docker/
├── Dockerfile               ← Container image definition
│                            (Python 3.12, FastAPI ready)
└── docker-compose.yml       ← Multi-container orchestration
                             (FastAPI + PostgreSQL setup)
```

### `static/` - Frontend Assets
```
static/
├── index.html              ← Chat interface
├── app.js                  ← Frontend logic
└── styles.css              ← Styling
```

### Root Level - Configuration & Setup
```
Root/
├── README.md               ← Project overview (START HERE)
├── INDEX.md                ← Documentation index
├── CLEAN_STRUCTURE.md      ← Structure explanation (THIS FILE)
├── PROJECT_CLEANUP_REPORT.md ← Cleanup summary
├── STRUCTURE_ANALYSIS.md   ← Structure analysis
│
├── .env                    ← Development configuration
├── .env.production         ← Production template
├── .gitignore              ← Git ignore rules
│
├── pyproject.toml          ← Python project metadata
├── requirements.txt        ← Python dependencies
│
├── setup_postgresql.sh     ← Database setup script
├── test_database.py        ← Database connection test
│
└── .venv/                  ← Virtual environment (don't edit)
```

---

## 🎯 Common Tasks & Where to Look

### "I want to add a new API endpoint"
1. Check existing: `app/api/routes.py`
2. Add endpoint in routes.py
3. Create/update DTO: `app/modules/*/dto/`
4. Create/update service: `app/modules/*/services/`
5. Test it: `tests/integration/test_endpoints.py`

### "I need to add a database table"
1. Define model: `app/db/models.py`
2. Create CRUD: `app/db/crud.py`
3. Run migration: `setup_postgresql.sh`
4. Update API: `app/api/routes.py`

### "How do I deploy this?"
1. Read: `docs/deployment/DEPLOYMENT_QUICK_START.md`
2. Setup DB: `setup_postgresql.sh`
3. Test connection: `test_database.py`
4. Deploy: `docker-compose up -d`

### "How does the chat feature work?"
1. Read: `docs/ARCHITECTURE.md`
2. Check DTOs: `app/modules/chat/dto/chat_dto.py`
3. Check service: `app/modules/chat/services/chat_service.py`
4. Check endpoints: `app/api/routes.py`

### "I need to understand the API"
1. Read: `docs/API.md`
2. Check routes: `app/api/routes.py`
3. See tests: `tests/integration/test_endpoints.py`
4. View swagger: `/docs` endpoint

### "How do I run tests?"
1. Setup: `pip install -r requirements.txt`
2. Run: `pytest tests/`
3. Coverage: `pytest --cov tests/`

### "I need to configure the app"
1. Development: Edit `.env`
2. Production: Use `.env.production` as template
3. Settings file: `app/config/settings.py`

---

## 📋 File Purposes At A Glance

| File/Folder | Purpose | Edit? | When? |
|-------------|---------|-------|-------|
| `app/main.py` | FastAPI entry point | Yes | Setup changes |
| `app/api/routes.py` | API endpoints | Yes | Adding endpoints |
| `app/modules/` | Feature modules | Yes | Adding features |
| `app/core/` | Infrastructure | Maybe | Advanced customization |
| `app/db/` | Database layer | Yes | Schema changes |
| `tests/` | Test suite | Yes | Adding tests |
| `docs/` | Documentation | Yes | Keeping docs current |
| `docker/` | Container setup | Yes | Deployment changes |
| `.env` | Dev config | Yes | Local setup |
| `requirements.txt` | Dependencies | Yes | Adding packages |
| `README.md` | Project info | Rarely | Major updates |

---

## 🔍 How to Find Code

### By Feature
- **Agent functionality**: `app/modules/agent/`
- **Chat functionality**: `app/modules/chat/`
- **Database operations**: `app/db/`
- **API endpoints**: `app/api/routes.py`
- **Configuration**: `app/config/settings.py`

### By Layer
- **API Layer**: `app/api/`
- **Service Layer**: `app/modules/*/services/`
- **Data Layer**: `app/db/`
- **Presentation**: `static/`

### By Type
- **Models**: `app/db/models.py`
- **DTOs**: `app/modules/*/dto/`
- **Services**: `app/modules/*/services/`
- **Routes**: `app/api/routes.py`

---

## 💡 Best Practices in This Project

✅ **Modular Architecture**: Features in `app/modules/`
✅ **DTO Pattern**: Data transfer objects in `dto/` folders
✅ **Service Pattern**: Business logic in `services/` folders
✅ **Separation of Concerns**: Clear layer separation
✅ **Organized Tests**: Unit and integration tests separated
✅ **Configuration Management**: `.env` + `settings.py`
✅ **Documentation**: Organized by purpose and audience
✅ **Containerization**: Docker ready for production

---

## 🚀 Getting Started Checklist

- [ ] Read `README.md` (5 min)
- [ ] Read `CLEAN_STRUCTURE.md` (10 min)
- [ ] Install dependencies: `pip install -r requirements.txt`
- [ ] Setup .env: Copy `.env` template and fill values
- [ ] Setup database: Run `setup_postgresql.sh`
- [ ] Test database: Run `python test_database.py`
- [ ] Run app: `python app/main.py`
- [ ] Check API docs: Visit `http://localhost:8000/docs`
- [ ] Try endpoint: Use Swagger UI to test endpoints
- [ ] Run tests: `pytest tests/`

---

**Status**: 🟢 **READY TO USE**

Everything is organized and ready for development! 🎉
