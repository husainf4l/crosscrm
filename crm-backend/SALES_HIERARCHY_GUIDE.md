# Sales Hierarchy & Team Assignment Guide

## Overview
Your CRM now supports a complete sales hierarchy with team-based customer assignments and role-based access control.

## Data Structure

### 1. User Hierarchy
**User Model** (`Modules/User/User.cs`):
- `ManagerId`: Points to the user's manager
- `Manager`: Navigation property to manager
- `DirectReports`: Collection of users reporting to this person

**Example Hierarchy:**
```
VP Sales (ManagerId: null)
  ├── Sales Manager 1 (ManagerId: VP's Id)
  │    ├── Team Manager - Pink Team (ManagerId: Sales Manager 1's Id)
  │    │    ├── Sales Rep 1 (ManagerId: Team Manager's Id)
  │    │    └── Sales Rep 2 (ManagerId: Team Manager's Id)
  │    └── Team Manager - Yellow Team
  │         ├── Sales Rep 3
  │         └── Sales Rep 4
  └── Sales Manager 2
       └── Team Manager - Blue Team
            ├── Sales Rep 5
            └── Sales Rep 6
```

### 2. Teams
**Team Model** (`Modules/Collaboration/Team.cs`):
- `Id`: Unique identifier
- `Name`: Team name (e.g., "Pink Team", "Yellow Team")
- `Type`: TeamType enum (Sales, Support, Management, CrossFunctional)
- `ManagerUserId`: Team manager's user ID
- `CompanyId`: Multi-tenant support
- `Members`: Collection of TeamMember

**TeamMember Model** (`Modules/Collaboration/TeamMember.cs`):
- `UserId`: The team member's user ID
- `TeamId`: The team they belong to
- `Role`: TeamMemberRole enum (Manager, Member, Observer)

### 3. Customer Assignment
**Customer Model** (`Modules/Customer/Customer.cs`):
- `AssignedToTeamId`: Which team owns this customer (nullable)
- `AssignedToUserId`: Which sales rep handles this customer (nullable)
- `CompanyId`: Multi-tenant isolation

## Access Control Logic

### Sales Representative
- **Can See:** Only customers where `AssignedToUserId == their user ID`
- **Query Example:**
```csharp
var customers = context.Customers
    .Where(c => c.CompanyId == userCompanyId && c.AssignedToUserId == userId)
    .ToList();
```

### Team Manager
- **Can See:** 
  - All customers assigned to their team (`AssignedToTeamId == their team`)
  - Can filter to see only their team vs all teams
  
- **Query Example:**
```csharp
// Get user's managed teams
var managedTeamIds = context.TeamMembers
    .Where(tm => tm.UserId == userId && tm.Role == TeamMemberRole.Manager)
    .Select(tm => tm.TeamId)
    .ToList();

// Get customers for those teams
var teamCustomers = context.Customers
    .Where(c => c.CompanyId == userCompanyId && 
                managedTeamIds.Contains(c.AssignedToTeamId.Value))
    .ToList();

// Optional: Filter by specific team
var pinkTeamCustomers = teamCustomers
    .Where(c => c.AssignedToTeamId == pinkTeamId)
    .ToList();
```

### Sales Manager
- **Can See:** All customers across all teams under their management
- **Query Example:**
```csharp
// Get all users reporting to this manager (recursively)
var reportingUserIds = GetAllDirectReports(managerId);

// Get teams managed by those users
var managedTeamIds = context.TeamMembers
    .Where(tm => reportingUserIds.Contains(tm.UserId) && 
                 tm.Role == TeamMemberRole.Manager)
    .Select(tm => tm.TeamId)
    .ToList();

// Get customers
var customers = context.Customers
    .Where(c => c.CompanyId == userCompanyId && 
                managedTeamIds.Contains(c.AssignedToTeamId.Value))
    .ToList();
```

### VP Sales / Admin
- **Can See:** All customers in the company
- **Query Example:**
```csharp
var allCustomers = context.Customers
    .Where(c => c.CompanyId == userCompanyId)
    .ToList();
```

## Roles System
**Role Model** (`Modules/Collaboration/Role.cs`):
- Predefined system roles: Admin, Manager, SalesRep, SupportAgent
- Custom roles per company
- Role-based permissions via `RolePermission`
- User-to-Role mapping via `UserRole`

**Checking User Role:**
```csharp
var userRoles = context.UserRoles
    .Where(ur => ur.UserId == userId)
    .Include(ur => ur.Role)
    .Select(ur => ur.Role.Name)
    .ToList();

bool isSalesRep = userRoles.Contains(SystemRoles.SalesRep);
bool isManager = userRoles.Contains(SystemRoles.Manager);
```

## Implementation Steps

### 1. Create Teams
```graphql
mutation {
  createTeam(input: {
    name: "Pink Team"
    description: "Sales team for region A"
    type: SALES
    managerUserId: 5  # Team Manager's ID
    companyId: 1
  }) {
    id
    name
  }
}
```

### 2. Add Team Members
```graphql
mutation {
  addTeamMember(input: {
    teamId: 1
    userId: 10  # Sales Rep's ID
    role: MEMBER
  }) {
    id
  }
}
```

### 3. Assign Customer to Team and Sales Rep
```graphql
mutation {
  updateCustomer(input: {
    id: 100
    assignedToTeamId: 1  # Pink Team
    assignedToUserId: 10  # Sales Rep
  }) {
    id
    assignedTeam { name }
    assignedToUser { name }
  }
}
```

### 4. Set User Hierarchy
```graphql
mutation {
  updateUser(input: {
    id: 10  # Sales Rep
    managerId: 5  # Reports to Team Manager
  }) {
    id
    manager { name }
  }
}
```

## Query Examples

### Get My Customers (as Sales Rep)
```graphql
query {
  customers(where: { assignedToUserId: { eq: 10 } }) {
    id
    name
    assignedTeam { name }
  }
}
```

### Get Team Customers (as Team Manager)
```graphql
query {
  customers(where: { assignedToTeamId: { eq: 1 } }) {
    id
    name
    assignedToUser { name email }
  }
}
```

### Get Team with Members
```graphql
query {
  team(id: 1) {
    name
    manager { 
      name 
      email 
    }
    members {
      userName
      userEmail
      role
      isActive
    }
  }
}
```

## Migration Applied
✅ **20251116111937_AddSalesHierarchyAndAssignments**
- Added `Users.ManagerId` column
- Added `Customers.AssignedToUserId` column
- Created necessary foreign keys and indexes

## Next Steps
1. Update `CustomerService` to filter by user role and assignments
2. Add GraphQL resolvers for team-based queries
3. Implement permission checks in mutations
4. Add team and user assignment to customer creation/update DTOs
5. Create dashboard queries showing hierarchy (my team, my customers, etc.)
