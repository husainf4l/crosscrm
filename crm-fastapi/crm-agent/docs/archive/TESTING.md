# Testing Guide

## Running Tests

### Install Test Dependencies
```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent
source venv/bin/activate
pip install -r requirements.txt
```

### Run All Tests
```bash
pytest
```

### Run Unit Tests Only
```bash
pytest tests/unit/
```

### Run Integration Tests Only
```bash
pytest tests/integration/
```

### Run with Coverage
```bash
pytest --cov=app --cov-report=html
```

### Run Specific Test File
```bash
pytest tests/unit/services/test_agent_service.py -v
```

## Test Structure

```
tests/
├── conftest.py              # Pytest fixtures
├── unit/                    # Unit tests
│   └── services/           # Service layer tests
│       ├── test_agent_service.py
│       └── test_chat_service.py
└── integration/            # Integration tests
    └── test_endpoints.py   # API endpoint tests
```

## Test Coverage

### Unit Tests
- ✅ AgentService - Agent operations
- ✅ ChatService - Chat operations
- ✅ DTO validation
- ✅ Error handling

### Integration Tests
- ✅ Health check endpoint
- ✅ Agent list endpoint
- ✅ Chat interface endpoint
- ✅ Endpoint validation

## Manual Testing

### 1. Test Health Check
```bash
curl http://localhost:8000/
```

### 2. Test Agent List
```bash
curl http://localhost:8000/agents/list
```

### 3. Test Chat Interface
```bash
curl http://localhost:8000/chat
```

### 4. Test Send Message
```bash
curl -X POST http://localhost:8000/chat/message \
  -H "Content-Type: application/json" \
  -d '{"user_id": 1, "message": "Hello"}'
```

### 5. Test Run Agent
```bash
curl -X POST http://localhost:8000/agents/run \
  -H "Content-Type: application/json" \
  -d '{"user_id": 1, "agent_type": "REMINDER"}'
```

## GraphQL Integration Testing

### Test GraphQL Client
```python
from app.core.graphql_client import GraphQLClient

client = GraphQLClient()
result = await client.query("""
    query {
        getMyTasks {
            id
            title
        }
    }
""")
```

## Best Practices

1. **Mock External Dependencies**: Use mocks for database, GraphQL, and OpenAI
2. **Test Error Cases**: Test validation errors, network errors, etc.
3. **Test Edge Cases**: Empty data, null values, boundary conditions
4. **Keep Tests Fast**: Use mocks instead of real API calls
5. **Test Coverage**: Aim for >80% code coverage

