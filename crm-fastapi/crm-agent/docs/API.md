# API Documentation

## Base URL
```
http://localhost:8001
```

## Endpoints

### Health Check
- **GET** `/`
- **Description**: Check if the CRM Agent is running
- **Response**: 
```json
{
  "status": "ok",
  "message": "CRM Agent is running"
}
```

---

## Chat Interface

### Serve Chat Interface
- **GET** `/chat`
- **Description**: Serve the chat interface HTML
- **Response**: HTML page

---

## Chat Messages

### Send Chat Message
- **POST** `/chat/message`
- **Description**: Send a message to the CRM agent and get a response
- **Request Body**:
```json
{
  "user_id": 1,
  "message": "Hello, I need help with..."
}
```
- **Response**:
```json
{
  "user_id": 1,
  "message": "Agent response here...",
  "agent_type": "AGENT_RESPONSE"
}
```
- **Error Handling**: Returns 500 if database connection fails

### Get Chat History
- **GET** `/chat/history/{user_id}`
- **Query Parameters**:
  - `limit` (integer, default: 50): Maximum number of messages to retrieve
- **Response**:
```json
{
  "user_id": 1,
  "messages": [
    {
      "id": 123,
      "agent_type": "USER_MESSAGE",
      "message": "User message text",
      "created_at": "2025-11-16T10:30:00"
    },
    {
      "id": 124,
      "agent_type": "AGENT_RESPONSE",
      "message": "Agent response text",
      "created_at": "2025-11-16T10:30:05"
    }
  ]
}
```

---

## Business Profile

### Create or Update Business Profile
- **POST** `/business-profile/{user_id}`
- **Request Body**:
```json
{
  "business_type": "tech",
  "products": ["SaaS", "Consulting"],
  "tone": "professional",
  "daily_goal": "Generate 5 leads",
  "keywords": ["innovation", "digital", "solutions"]
}
```
- **Response**:
```json
{
  "status": "success",
  "profile_id": 42
}
```

### Get Business Profile
- **GET** `/business-profile/{user_id}`
- **Response**:
```json
{
  "id": 42,
  "user_id": 1,
  "business_type": "tech",
  "products": ["SaaS", "Consulting"],
  "tone": "professional",
  "daily_goal": "Generate 5 leads",
  "keywords": ["innovation", "digital", "solutions"]
}
```
- **Error Handling**: Returns 404 if profile not found

---

## Tasks

### Get Today's Tasks
- **GET** `/tasks/today/{user_id}`
- **Response**:
```json
{
  "user_id": 1,
  "tasks": [
    {
      "id": 1,
      "title": "Follow up with client",
      "status": "pending",
      "due_date": "2025-11-16"
    },
    {
      "id": 2,
      "title": "Send proposal",
      "status": "completed",
      "due_date": "2025-11-16"
    }
  ]
}
```

---

## Progress

### Get User Progress
- **GET** `/progress/{user_id}`
- **Response**:
```json
{
  "user_id": 1,
  "progress": {
    "leads_generated": 12,
    "meetings_completed": 3,
    "deals_closed": 2,
    "tasks_completed": 45
  }
}
```

---

## Agents

### Run Agent
- **POST** `/agents/run`
- **Request Body**:
```json
{
  "user_id": 1,
  "agent_type": "REMINDER"
}
```
- **Agent Types**:
  - `REMINDER`: Daily reminder agent (09:00)
  - `FOLLOW_UP`: Follow-up agent (13:00)
  - `CLOSURE`: Closure push agent (16:00)
  - `NURTURE`: Nurture agent (11:00, every 2 days)
  - `UPSELL`: Upsell agent (10:00, every Monday)

- **Response**:
```json
{
  "status": "ok",
  "agent_type": "REMINDER",
  "user_id": 1,
  "message": "Generated reminder message here..."
}
```
- **Error Handling**: Returns 500 if agent fails to run

### List Available Agents
- **GET** `/agents/list`
- **Response**:
```json
{
  "agents": [
    "REMINDER",
    "FOLLOW_UP",
    "CLOSURE",
    "NURTURE",
    "UPSELL"
  ]
}
```

---

## Error Responses

All error responses follow this format:

```json
{
  "detail": "Error message describing what went wrong"
}
```

### Common Error Codes
- **400**: Bad Request - Invalid input data
- **404**: Not Found - Resource doesn't exist
- **500**: Internal Server Error - Database or agent failure

---

## Authentication & Security

- CORS enabled for all origins (configure in production)
- No authentication required for current implementation
- Environment variables for sensitive configuration

---

## Rate Limiting

Currently no rate limiting implemented. To be added in future versions.

---

## WebSocket Support (Future)

Real-time chat support to be implemented using WebSockets for better performance.
