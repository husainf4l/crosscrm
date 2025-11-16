# User Invitation System - GraphQL API Documentation

## Overview

The Cross CRM backend now includes a comprehensive user invitation system that allows company administrators to invite new users via email. This document provides complete GraphQL API documentation for frontend integration.

## Base Configuration

- **GraphQL Endpoint**: `http://localhost:5196/graphql` (development)
- **Authentication**: JWT Bearer tokens required for most operations
- **Content-Type**: `application/json`

## GraphQL Schema Types

### InvitationStatus Enum
```graphql
enum InvitationStatus {
  PENDING
  ACCEPTED
  DECLINED
  EXPIRED
  CANCELLED
}
```

### Input Types

#### InviteUserDto
```graphql
input InviteUserDtoInput {
  email: String!           # Required: Valid email address
  companyId: Int!          # Required: Company ID (must match authenticated user's company)
  roleId: Int              # Optional: Role to assign to the invited user
  teamId: Int              # Optional: Team to add the invited user to
  notes: String            # Optional: Additional notes (max 500 characters)
}
```

#### AcceptInvitationDto
```graphql
input AcceptInvitationDtoInput {
  invitationToken: String! # Required: Unique invitation token from email
  name: String!            # Required: Full name (min 2, max 100 characters)
  password: String!        # Required: Password (min 6, max 100 characters)
  phone: String            # Optional: Phone number (max 20 characters)
}
```

### Response Types

#### UserInvitationDto
```graphql
type UserInvitationDto {
  id: Int!
  email: String!
  invitationToken: String!
  companyId: Int!
  companyName: String!
  invitedByUserId: Int!
  invitedByUserName: String!
  roleId: Int
  roleName: String
  teamId: Int
  teamName: String
  status: InvitationStatus!
  createdAt: DateTime!
  expiresAt: DateTime!
  acceptedAt: DateTime
  acceptedByUserId: Int
  acceptedByUserName: String
  notes: String
}
```

#### InvitationResponseDto
```graphql
type InvitationResponseDto {
  success: Boolean!
  message: String!
  invitation: UserInvitationDto
  authResponse: AuthResponseDto  # Contains JWT token and user info when successful
}
```

## GraphQL Queries

### 1. Get Company Invitations
Retrieves all invitations for the authenticated user's company.

```graphql
query GetCompanyInvitations {
  companyInvitations {
    id
    email
    status
    companyName
    invitedByUserName
    roleName
    teamName
    createdAt
    expiresAt
    acceptedAt
    acceptedByUserName
    notes
  }
}
```

**Authentication**: Required  
**Permissions**: User must belong to the company

**Example cURL:**
```bash
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"query": "query { companyInvitations { id email status companyName invitedByUserName createdAt } }"}'
```

### 2. Get My Invitations
Retrieves all invitations sent by the authenticated user.

```graphql
query GetMyInvitations {
  myInvitations {
    id
    email
    status
    companyName
    roleName
    teamName
    createdAt
    expiresAt
    acceptedAt
    notes
  }
}
```

**Authentication**: Required

### 3. Get Invitation by Token (Public)
Retrieves invitation details by token. No authentication required.

```graphql
query GetInvitationByToken($token: String!) {
  getInvitationByToken(token: $token) {
    email
    companyName
    invitedByUserName
    roleName
    teamName
    createdAt
    expiresAt
    status
  }
}
```

**Authentication**: Not required  
**Use case**: Public invitation acceptance page

**Example cURL:**
```bash
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "query GetInvitationByToken($token: String!) { getInvitationByToken(token: $token) { email companyName invitedByUserName status expiresAt } }", "variables": {"token": "INVITATION_TOKEN"}}'
```

## GraphQL Mutations

### 1. Invite User
Sends an invitation email to a new user.

```graphql
mutation InviteUser($input: InviteUserDtoInput!) {
  inviteUser(input: $input) {
    id
    email
    companyName
    invitedByUserName
    roleName
    teamName
    status
    createdAt
    expiresAt
  }
}
```

**Authentication**: Required  
**Permissions**: User must be able to invite to their own company

**Variables Example:**
```json
{
  "input": {
    "email": "newuser@example.com",
    "companyId": 6,
    "roleId": 2,
    "teamId": 1,
    "notes": "Welcome to our team!"
  }
}
```

**Example cURL:**
```bash
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "query": "mutation InviteUser($input: InviteUserDtoInput!) { inviteUser(input: $input) { id email status companyName } }",
    "variables": {
      "input": {
        "email": "newuser@example.com",
        "companyId": 6,
        "notes": "Welcome!"
      }
    }
  }'
```

### 2. Accept Invitation
Allows an invited user to accept their invitation and create an account.

```graphql
mutation AcceptInvitation($input: AcceptInvitationDtoInput!) {
  acceptInvitation(input: $input) {
    success
    message
    invitation {
      id
      email
      companyName
      status
      acceptedAt
    }
    authResponse {
      token
      user {
        id
        name
        email
        companyId
        companyName
      }
    }
  }
}
```

**Authentication**: Not required  
**Use case**: Public invitation acceptance

**Variables Example:**
```json
{
  "input": {
    "invitationToken": "abc123def456",
    "name": "John Smith",
    "password": "securepassword123",
    "phone": "+1234567890"
  }
}
```

### 3. Cancel Invitation
Cancels a pending invitation.

```graphql
mutation CancelInvitation($invitationId: Int!) {
  cancelInvitation(invitationId: $invitationId)
}
```

**Authentication**: Required  
**Permissions**: User must be the inviter or have company admin rights

### 4. Resend Invitation
Resends the invitation email and extends expiration.

```graphql
mutation ResendInvitation($invitationId: Int!) {
  resendInvitation(invitationId: $invitationId)
}
```

**Authentication**: Required  
**Permissions**: User must be the inviter

## Email Testing

### Send Test Email
```graphql
mutation SendTestEmail($toEmail: String!) {
  sendTestEmail(toEmail: $toEmail)
}
```

**Authentication**: Required

**Example:**
```bash
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"query": "mutation { sendTestEmail(toEmail: \"test@example.com\") }"}'
```

## Frontend Integration Examples

### React/TypeScript Integration

#### 1. Invite User Hook
```typescript
import { useMutation, gql } from '@apollo/client';

const INVITE_USER = gql`
  mutation InviteUser($input: InviteUserDtoInput!) {
    inviteUser(input: $input) {
      id
      email
      status
      companyName
      createdAt
    }
  }
`;

export const useInviteUser = () => {
  return useMutation(INVITE_USER);
};

// Usage in component
const [inviteUser, { loading, error }] = useInviteUser();

const handleInvite = async (email: string, roleId?: number) => {
  try {
    const result = await inviteUser({
      variables: {
        input: {
          email,
          companyId: user.companyId,
          roleId,
          notes: 'Welcome to our team!'
        }
      }
    });
    console.log('Invitation sent:', result.data.inviteUser);
  } catch (err) {
    console.error('Failed to send invitation:', err);
  }
};
```

#### 2. Accept Invitation Hook
```typescript
const ACCEPT_INVITATION = gql`
  mutation AcceptInvitation($input: AcceptInvitationDtoInput!) {
    acceptInvitation(input: $input) {
      success
      message
      authResponse {
        token
        user {
          id
          name
          email
          companyId
        }
      }
    }
  }
`;

export const useAcceptInvitation = () => {
  return useMutation(ACCEPT_INVITATION);
};

// Usage in invitation acceptance page
const [acceptInvitation, { loading }] = useAcceptInvitation();

const handleAcceptInvitation = async (formData: AcceptInvitationForm) => {
  try {
    const result = await acceptInvitation({
      variables: {
        input: {
          invitationToken: token,
          name: formData.name,
          password: formData.password,
          phone: formData.phone
        }
      }
    });
    
    if (result.data.acceptInvitation.success) {
      // Store JWT token
      localStorage.setItem('token', result.data.acceptInvitation.authResponse.token);
      // Redirect to dashboard
      router.push('/dashboard');
    }
  } catch (err) {
    setError('Failed to accept invitation');
  }
};
```

#### 3. Company Invitations Query
```typescript
const GET_COMPANY_INVITATIONS = gql`
  query GetCompanyInvitations {
    companyInvitations {
      id
      email
      status
      invitedByUserName
      roleName
      teamName
      createdAt
      expiresAt
      acceptedAt
    }
  }
`;

export const useCompanyInvitations = () => {
  return useQuery(GET_COMPANY_INVITATIONS);
};

// Usage in admin panel
const { data, loading, refetch } = useCompanyInvitations();

const invitations = data?.companyInvitations || [];
```

### Vue.js Integration

#### Composable for Invitations
```typescript
import { useMutation, useQuery } from '@vue/apollo-composable';
import { gql } from 'graphql-tag';

export const useInvitations = () => {
  const INVITE_USER = gql`
    mutation InviteUser($input: InviteUserDtoInput!) {
      inviteUser(input: $input) {
        id
        email
        status
        companyName
      }
    }
  `;

  const GET_INVITATIONS = gql`
    query GetCompanyInvitations {
      companyInvitations {
        id
        email
        status
        createdAt
      }
    }
  `;

  const { mutate: inviteUser, loading: inviteLoading } = useMutation(INVITE_USER);
  const { result: invitationsResult, refetch } = useQuery(GET_INVITATIONS);

  const sendInvitation = async (email: string, companyId: number) => {
    try {
      await inviteUser({ input: { email, companyId } });
      refetch();
    } catch (error) {
      console.error('Invitation failed:', error);
    }
  };

  return {
    invitations: computed(() => invitationsResult.value?.companyInvitations || []),
    sendInvitation,
    inviteLoading,
    refetch
  };
};
```

## Error Handling

### Common Error Responses

1. **Validation Errors**
```json
{
  "errors": [
    {
      "message": "Validation failed: Email is required, Password must be at least 6 characters long"
    }
  ]
}
```

2. **Authentication Errors**
```json
{
  "errors": [
    {
      "message": "User not authenticated"
    }
  ]
}
```

3. **Business Logic Errors**
```json
{
  "errors": [
    {
      "message": "User with this email already exists"
    }
  ]
}
```

### Frontend Error Handling Pattern
```typescript
const handleGraphQLError = (error: ApolloError) => {
  if (error.graphQLErrors?.length > 0) {
    const message = error.graphQLErrors[0].message;
    if (message.includes('not authenticated')) {
      // Redirect to login
      router.push('/login');
    } else {
      // Show user-friendly error
      setErrorMessage(message);
    }
  } else {
    setErrorMessage('An unexpected error occurred');
  }
};
```

## Best Practices

### 1. Token Storage
- Store JWT tokens securely (httpOnly cookies preferred)
- Include tokens in Authorization header: `Bearer YOUR_JWT_TOKEN`

### 2. Error Handling
- Always handle GraphQL errors gracefully
- Provide user-friendly error messages
- Implement proper loading states

### 3. Invitation Flow
1. **Admin Panel**: Use `inviteUser` mutation to send invitations
2. **Email Link**: Include invitation token in URL (`/accept-invitation/{token}`)
3. **Acceptance Page**: Use `getInvitationByToken` to show invitation details
4. **Account Creation**: Use `acceptInvitation` to create user account
5. **Automatic Login**: Use returned JWT token to authenticate user

### 4. State Management
- Refetch invitation lists after mutations
- Update local state optimistically
- Handle real-time updates if using subscriptions

### 5. URL Structure
```
/admin/invitations          # List company invitations
/admin/invite-user          # Send new invitation
/accept-invitation/:token   # Public invitation acceptance page
```

This documentation provides everything your frontend team needs to integrate the user invitation system with the GraphQL API.