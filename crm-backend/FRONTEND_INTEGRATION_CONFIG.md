# Frontend Integration Configuration

## Backend Configuration
The backend is now configured to work with your Angular frontend running on port 4200.

### Backend Ports
- **HTTP**: `http://localhost:5196`
- **HTTPS**: `https://localhost:7162`

### Frontend Port
- **Angular Dev Server**: `http://localhost:4200`

## CORS Configuration
Updated CORS policy to specifically allow your frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200", 
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Allows cookies and auth headers
    });
});
```

## GraphQL Endpoints
- **GraphQL Playground**: `http://localhost:5196/graphql/` or `https://localhost:7162/graphql/`
- **GraphQL API**: `http://localhost:5196/graphql` or `https://localhost:7162/graphql`

## Frontend Configuration
For your Angular application, configure the GraphQL client to point to:

### Apollo Client Configuration (Angular)
```typescript
import { NgModule } from '@angular/core';
import { ApolloModule, APOLLO_OPTIONS } from 'apollo-angular';
import { ApolloClientOptions, InMemoryCache } from '@apollo/client/core';
import { HttpLink } from 'apollo-angular/http';

const uri = 'http://localhost:5196/graphql'; // Backend GraphQL endpoint

export function createApollo(httpLink: HttpLink): ApolloClientOptions<any> {
  return {
    link: httpLink.create({ 
      uri,
      // Include credentials for authentication
      withCredentials: true 
    }),
    cache: new InMemoryCache(),
  };
}

@NgModule({
  exports: [ApolloModule],
  providers: [
    {
      provide: APOLLO_OPTIONS,
      useFactory: createApollo,
      deps: [HttpLink],
    },
  ],
})
export class GraphQLModule {}
```

### HTTP Client Configuration (Angular)
```typescript
// In your HTTP interceptor or service
const apiUrl = 'http://localhost:5196';

// Configure HTTP client with credentials
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }),
  withCredentials: true // Important for auth cookies
};
```

## Authentication Integration
If using JWT authentication, include the token in requests:

### GraphQL with Authentication
```typescript
import { setContext } from '@apollo/client/link/context';

const authLink = setContext((_, { headers }) => {
  const token = localStorage.getItem('token'); // or however you store tokens
  return {
    headers: {
      ...headers,
      authorization: token ? `Bearer ${token}` : "",
    }
  };
});

// Combine with HTTP link
const link = authLink.concat(httpLink.create({ uri }));
```

## Sample GraphQL Queries for Testing

### Test Connection
```graphql
query TestConnection {
  __schema {
    types {
      name
    }
  }
}
```

### Get Companies
```graphql
query GetCompanies {
  companies {
    id
    name
    email
    logo
    website
    industry
    size
  }
}
```

### Invite User
```graphql
mutation InviteUser($input: InviteUserDtoInput!) {
  inviteUser(input: $input) {
    id
    email
    status
    companyName
    roleName
  }
}
```

## Development Workflow

### 1. Start Backend
```bash
cd /Users/husain/Desktop/cross/crm-backend
dotnet run
```
Backend will be available at:
- HTTP: http://localhost:5196
- HTTPS: https://localhost:7162

### 2. Start Frontend
```bash
cd your-frontend-directory
ng serve
# or
npm start
```
Frontend will be available at: http://localhost:4200

### 3. Test GraphQL
Visit: http://localhost:5196/graphql/ to test queries in GraphQL Playground

## Troubleshooting

### CORS Issues
If you encounter CORS errors:
1. Verify frontend is running on port 4200
2. Check browser developer tools for specific error messages
3. Ensure `withCredentials: true` is set in your HTTP client if using authentication

### Connection Issues
1. Verify backend is running: `curl http://localhost:5196/graphql`
2. Check firewall settings
3. Ensure no other services are using ports 5196/7162

### Authentication Issues
1. Verify JWT token format and expiration
2. Check authentication headers in browser dev tools
3. Ensure CORS allows credentials

## Production Configuration
For production, update CORS to include your production domain:

```csharp
policy.WithOrigins(
    "https://yourdomain.com",
    "https://www.yourdomain.com"
)
```

## Health Check Endpoint
The backend includes a health check at: `/health` (if configured)

## API Documentation
- GraphQL Schema: Available at `/graphql/` endpoint
- Email System API: See `EMAIL_SYSTEM_API_DOCUMENTATION.md`
- User Invitation API: See `USER_INVITATION_API_DOCUMENTATION.md`