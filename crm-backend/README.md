# CRM Backend

This is an ASP.NET Core Web API project for the CRM backend.

## Prerequisites

- .NET 10.0 SDK

## Getting Started

1. Restore dependencies:
   ```
   dotnet restore
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

The API will be available at `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP).

## GraphQL

The project includes GraphQL support using Hot Chocolate.

- GraphQL endpoint: `/graphql`
- Banana Cake Pop UI: `/graphql` (in development mode)

Example query:
```graphql
{
  hello
}
```

## Authentication

The API uses JWT (JSON Web Tokens) for authentication, compatible with both web and Flutter applications.

- **Token Expiration**: 24 hours
- **Header**: `Authorization: Bearer {token}`
- **Claims**: User ID, email, name, company ID

### For Flutter Integration

Include the JWT token in your HTTP headers:
```dart
headers: {
  'Authorization': 'Bearer $token',
  'Content-Type': 'application/json',
}
```

### Security Features

- BCrypt password hashing
- JWT token validation
- Email uniqueness validation
- Company-based user isolation

## Development

- Use Visual Studio Code with the C# Dev Kit extension for the best development experience.
- The project includes Swagger UI for API testing at `/swagger` when running in development mode.

## Project Structure

- `Modules/User/`: User module with entities, DTOs, and services
- `Modules/Company/`: Company module with entity
- `Data/`: Database context and configurations
- `GraphQL/`: GraphQL queries and mutations (resolvers)

## API Endpoints

### GraphQL

- Endpoint: `/graphql`
- Banana Cake Pop UI: `/graphql` (in development mode)
- Queries: `GetUsers`, `GetUser`, `GetCompanies`, `GetCompany`
- Mutations: `Register`, `Login`, `CreateUser`, `UpdateUser`, `DeleteUser`, `CreateCompany`, `UpdateCompany`, `DeleteCompany`, `AddUserToCompany`, `RemoveUserFromCompany`, `SetActiveCompany`

Example authentication queries:
```graphql
# Register a new user
mutation {
  register(input: {
    name: "John Doe"
    email: "john@example.com"
    password: "securepassword"
    companyId: 1
  }) {
    token
    user {
      id
      name
      email
      userCompanies {
        companyId
        companyName
        isActive
        joinedAt
      }
    }
  }
}

# Login
mutation {
  login(input: {
    email: "john@example.com"
    password: "securepassword"
  }) {
    token
    user {
      id
      name
      email
      company {
        name
      }
      userCompanies {
        companyId
        companyName
        isActive
        joinedAt
      }
    }
  }
}

Example queries:
```graphql
# Get all users
query {
  getUsers {
    id
    name
    email
    phone
    createdAt
    company {
      name
    }
    userCompanies {
      companyId
      companyName
      isActive
      joinedAt
    }
  }
}

# Create a user (without company initially)
mutation {
  createUser(input: { name: "John Doe", email: "john@example.com" }) {
    id
    name
    email
    userCompanies {
      companyId
      companyName
      isActive
      joinedAt
    }
  }
}

# Add user to a company
mutation {
  addUserToCompany(userId: 1, companyId: 1)
}

# Set active company for user
mutation {
  setActiveCompany(userId: 1, companyId: 1)
}

# Create a company
mutation {
  createCompany(name: "Acme Corp", description: "A sample company") {
    id
    name
    description
  }
}
```