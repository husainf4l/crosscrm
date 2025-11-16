# Setting Up crm-backend for GraphQL Integration

## Prerequisites

### Install .NET SDK
```bash
# Option 1: Using snap (recommended)
sudo snap install dotnet-sdk

# Option 2: Using apt
sudo apt install dotnet-host-8.0

# Verify installation
dotnet --version
```

## Starting crm-backend Server

### 1. Navigate to crm-backend directory
```bash
cd /home/husain/crosscrm/crm-backend
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Build the project
```bash
dotnet build
```

### 4. Set up environment variables
Create a `.env` file in crm-backend directory:
```env
DB_HOST=localhost
DB_USERNAME=your_db_user
DB_PASSWORD=your_db_password
DB_DATABASE=crm
JWT_KEY=your_jwt_secret_key_here
JWT_ISSUER=crm-backend
JWT_AUDIENCE=crm-client
```

### 5. Run the server on port 5196
```bash
dotnet run --urls "http://localhost:5196"
```

Or set it in `Properties/launchSettings.json`:
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5196"
    }
  }
}
```

## Testing GraphQL Endpoint

### Test with curl
```bash
# Test hello query
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"{ hello }"}'

# Test with authentication (if required)
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"query":"{ getMyTasks { id title } }"}'
```

### Test in browser
Open: `http://localhost:5196/graphql` (Banana Cake Pop UI in development mode)

## Updating crm-agent .env

Once the backend is running, update `/home/husain/crosscrm/crm-fastapi/crm-agent/.env`:

```env
# GraphQL Backend (crm-backend)
GRAPHQL_URL=http://localhost:5196/graphql
GRAPHQL_API_KEY=your_api_key_if_needed

# JWT Authentication (if needed for GraphQL)
JWT_SECRET=your_jwt_secret_key_here
JWT_ALGORITHM=HS256
```

## Authentication

If crm-backend requires authentication:

1. **Get JWT Token**:
   - Register/login via GraphQL mutation
   - Extract token from response
   - Add to `GRAPHQL_API_KEY` in `.env`

2. **Or use API Key**:
   - If using API key authentication
   - Add to `GRAPHQL_API_KEY` in `.env`

## Verifying Connection

```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent
source venv/bin/activate

python -c "
from app.core.graphql_client import GraphQLClient
import asyncio

async def test():
    client = GraphQLClient()
    result = await client.query('query { hello }')
    print('✅ Connected:', result)

asyncio.run(test())
"
```

