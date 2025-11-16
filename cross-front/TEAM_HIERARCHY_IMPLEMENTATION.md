# Team Hierarchy & Assignment System Implementation

## Overview
This document describes the implementation of the Team Management system with user hierarchy and customer assignments for the Cross CRM application.

## System Architecture

### 1. User Hierarchy
**Structure:**
- Users can have a manager (ManagerId field)
- Users can have direct reports (subordinates)
- Hierarchy is recursive (managers can have managers)
- Example: VP Sales → Sales Manager → Team Manager → Sales Rep

**GraphQL Fields:**
```graphql
User {
  id: String
  name: String
  email: String
  managerId: String (nullable)
  manager: User (navigation)
  directReports: [User] (collection)
  roles: [String]
}
```

### 2. Teams
**Structure:**
- Teams have a type (SALES, SUPPORT, MANAGEMENT, CROSS_FUNCTIONAL)
- Teams have a manager user
- Teams contain members with roles (MANAGER, MEMBER, OBSERVER)

**GraphQL Fields:**
```graphql
Team {
  id: String
  name: String
  description: String
  type: TeamType
  managerUserId: String
  manager: User
  members: [TeamMember]
  companyId: String
}

TeamMember {
  id: String
  userId: String
  teamId: String
  role: TeamMemberRole
  user: User
  team: Team
}
```

### 3. Customer Assignments
**Structure:**
- Customers can be assigned to a team
- Customers can be assigned to a specific user (sales rep)
- Both assignments are optional

**GraphQL Fields:**
```graphql
Customer {
  assignedToTeamId: String (nullable)
  assignedToUserId: String (nullable)
  assignedTeam: Team
  assignedToUser: User
}
```

## Access Control Logic

### Sales Representative
**Can See:** Only customers where `assignedToUserId == their user ID`

```graphql
query GetMyCustomers($userId: Int!) {
  customers(where: { assignedToUserId: { eq: $userId } }) {
    id
    name
    assignedTeam { name }
  }
}
```

### Team Manager
**Can See:** 
- All customers assigned to teams they manage
- Can filter by specific team

```graphql
query GetTeamCustomers($teamId: Int!) {
  customers(where: { assignedToTeamId: { eq: $teamId } }) {
    id
    name
    assignedToUser { name email }
  }
}
```

### Sales Manager
**Can See:** All customers across all teams under their management (recursive)

Implementation uses:
1. Get all users reporting to manager (recursive)
2. Get teams managed by those users
3. Get customers for those teams

### VP Sales / Admin
**Can See:** All customers in the company

```graphql
query GetAllCustomers {
  customers {
    id
    name
    assignedTeam { name }
    assignedToUser { name }
  }
}
```

## Frontend Implementation

### Files Created

#### 1. `/src/app/services/team.service.ts`
**Purpose:** Service for team, user, and hierarchy management

**Key Methods:**
- `getTeams()` - Get all teams
- `getTeamById(id)` - Get single team with members
- `getTeamsByManager(userId)` - Get teams managed by user
- `getUsers()` - Get all users with hierarchy
- `getUserById(id)` - Get user with manager and reports
- `getDirectReports(managerId)` - Get direct reports
- `createTeam(input)` - Create new team
- `updateTeam(id, input)` - Update team details
- `deleteTeam(id)` - Delete team
- `addTeamMember(input)` - Add member to team
- `removeTeamMember(teamId, userId)` - Remove member
- `updateTeamMemberRole(teamId, userId, role)` - Change member role
- `updateUserManager(userId, managerId)` - Set reporting hierarchy
- `getCustomersByTeam(teamId)` - Get team's customers
- `getCustomersByUser(userId)` - Get user's customers
- `getAllReportsUnder(managerId, allUsers)` - Recursive reports helper

#### 2. `/src/app/admin/team-management.component.ts`
**Purpose:** Admin UI for managing teams, users, and hierarchy

**Features:**
- **Teams Tab:** View, create, edit, delete teams
- **Users Tab:** View users and their hierarchy relationships
- **Hierarchy Tab:** Visual representation of org chart
- **Assignments Tab:** Overview of customer assignments

**Modals:**
- Create/Edit Team Modal
- Edit User Manager Modal
- Team Members Modal

#### 3. Updated `/src/app/services/customer.service.ts`
**Changes:**
- Added `assignedToTeamId`, `assignedToUserId` to Customer interface
- Added `assignedTeam`, `assignedToUser` objects with details
- Updated `GetCustomers` query to include assignment fields
- Updated `GetCustomer` query to include assignment fields
- Updated `CreateCustomer` mutation to accept assignment inputs
- Updated `UpdateCustomer` mutation to accept assignment inputs

#### 4. Updated `/src/app/components/layouts/sidebar/sidebar.component.ts`
**Changes:**
- Added "Team Management" menu item with users icon
- Route: `/team-management`

#### 5. Updated `/src/app/app.routes.ts`
**Changes:**
- Imported `TeamManagementComponent`
- Added route: `/team-management` as child of DashboardComponent

## Usage Guide

### For Admins

#### Creating a Team
1. Navigate to "Team Management" in sidebar
2. Click "Create Team" button
3. Fill in:
   - Team Name (required)
   - Description (optional)
   - Type: Sales, Support, Management, or Cross Functional
   - Team Manager (select from users)
4. Click "Save"

#### Adding Team Members
1. In Teams tab, click "Members" button on a team card
2. Select user from dropdown
3. Choose role: Manager, Member, or Observer
4. Click "Add"

#### Setting User Hierarchy
1. Navigate to "Users" tab
2. Click "Edit Hierarchy" for a user
3. Select their manager from dropdown (or "None" for top-level)
4. Click "Save"

#### Assigning Customers (in Customer module)
When creating or editing a customer:
1. Select "Assigned Team" from dropdown
2. Select "Assigned User" from dropdown (filtered by team)
3. Save customer

### For Backend Implementation

#### Required GraphQL Schema Updates

```graphql
# User Type
type User {
  id: Int!
  name: String!
  email: String!
  managerId: Int
  manager: User
  directReports: [User!]!
  roles: [String!]!
}

# Team Type
type Team {
  id: Int!
  name: String!
  description: String
  type: TeamType!
  managerUserId: Int
  manager: User
  members: [TeamMember!]!
  companyId: Int!
  createdAt: DateTime!
}

enum TeamType {
  SALES
  SUPPORT
  MANAGEMENT
  CROSS_FUNCTIONAL
}

# TeamMember Type
type TeamMember {
  id: Int!
  userId: Int!
  teamId: Int!
  role: TeamMemberRole!
  user: User!
  team: Team!
}

enum TeamMemberRole {
  MANAGER
  MEMBER
  OBSERVER
}

# Updated Customer Type
type Customer {
  # ... existing fields ...
  assignedToTeamId: Int
  assignedToUserId: Int
  assignedTeam: Team
  assignedToUser: User
}

# Input Types
input CreateTeamInput {
  name: String!
  description: String
  type: TeamType!
  managerUserId: Int
  companyId: Int!
}

input UpdateTeamInput {
  name: String
  description: String
  type: TeamType
  managerUserId: Int
}

input AddTeamMemberInput {
  teamId: Int!
  userId: Int!
  role: TeamMemberRole!
}

# Queries
type Query {
  teams: [Team!]!
  team(id: Int!): Team
  users: [User!]!
  user(id: Int!): User
}

# Mutations
type Mutation {
  createTeam(input: CreateTeamInput!): Team!
  updateTeam(id: Int!, input: UpdateTeamInput!): Team!
  deleteTeam(id: Int!): Boolean!
  addTeamMember(input: AddTeamMemberInput!): TeamMember!
  removeTeamMember(teamId: Int!, userId: Int!): Boolean!
  updateTeamMemberRole(teamId: Int!, userId: Int!, role: TeamMemberRole!): TeamMember!
  updateUser(input: UpdateUserInput!): User!
}
```

#### Backend Filtering Logic

```csharp
// Example: Filter customers based on user role
public async Task<List<Customer>> GetCustomersForUser(int userId, string userRole)
{
    var user = await _context.Users
        .Include(u => u.Manager)
        .Include(u => u.DirectReports)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (userRole == "Admin" || userRole == "VPSales")
    {
        // Admin sees all
        return await _context.Customers
            .Include(c => c.AssignedTeam)
            .Include(c => c.AssignedToUser)
            .ToListAsync();
    }
    else if (userRole == "SalesManager")
    {
        // Get all reporting users recursively
        var reportingUserIds = GetAllReportingUsers(userId);
        
        // Get teams managed by those users
        var teamIds = await _context.TeamMembers
            .Where(tm => reportingUserIds.Contains(tm.UserId) && 
                        tm.Role == TeamMemberRole.Manager)
            .Select(tm => tm.TeamId)
            .ToListAsync();
        
        // Get customers for those teams
        return await _context.Customers
            .Where(c => c.AssignedToTeamId.HasValue && 
                       teamIds.Contains(c.AssignedToTeamId.Value))
            .Include(c => c.AssignedTeam)
            .Include(c => c.AssignedToUser)
            .ToListAsync();
    }
    else if (userRole == "TeamManager")
    {
        // Get managed teams
        var teamIds = await _context.TeamMembers
            .Where(tm => tm.UserId == userId && tm.Role == TeamMemberRole.Manager)
            .Select(tm => tm.TeamId)
            .ToListAsync();
        
        return await _context.Customers
            .Where(c => c.AssignedToTeamId.HasValue && 
                       teamIds.Contains(c.AssignedToTeamId.Value))
            .Include(c => c.AssignedTeam)
            .Include(c => c.AssignedToUser)
            .ToListAsync();
    }
    else // SalesRep
    {
        // Only their assigned customers
        return await _context.Customers
            .Where(c => c.AssignedToUserId == userId)
            .Include(c => c.AssignedTeam)
            .Include(c => c.AssignedToUser)
            .ToListAsync();
    }
}

private List<int> GetAllReportingUsers(int managerId)
{
    var directReports = _context.Users
        .Where(u => u.ManagerId == managerId)
        .Select(u => u.Id)
        .ToList();
    
    var allReports = new List<int>(directReports);
    
    foreach (var reportId in directReports)
    {
        allReports.AddRange(GetAllReportingUsers(reportId));
    }
    
    return allReports;
}
```

## Database Migration

The backend should have already applied:
```
20251116111937_AddSalesHierarchyAndAssignments
```

This migration adds:
- `Users.ManagerId` column with foreign key to Users table
- `Customers.AssignedToUserId` column with foreign key to Users table
- `Customers.AssignedToTeamId` column with foreign key to Teams table
- Appropriate indexes for performance

## Testing Checklist

### Admin Testing
- [ ] Create a new team (Sales, Support, Management, Cross Functional)
- [ ] Add members to team with different roles
- [ ] Remove member from team
- [ ] Edit team details (name, description, manager)
- [ ] Delete team
- [ ] Set user's manager (create hierarchy)
- [ ] View organizational hierarchy tree
- [ ] Assign customer to team and user
- [ ] Verify customer appears in correct user's view

### Role-Based Access Testing
- [ ] Login as Sales Rep - verify only sees assigned customers
- [ ] Login as Team Manager - verify sees all team customers
- [ ] Login as Sales Manager - verify sees all subordinate team customers
- [ ] Login as Admin - verify sees all customers

### GraphQL Testing
```graphql
# Test: Create Team
mutation {
  createTeam(input: {
    name: "Pink Team"
    description: "Sales team for East region"
    type: SALES
    managerUserId: 5
    companyId: 1
  }) {
    id
    name
    manager { name }
  }
}

# Test: Add Team Member
mutation {
  addTeamMember(input: {
    teamId: 1
    userId: 10
    role: MEMBER
  }) {
    id
    user { name email }
    role
  }
}

# Test: Assign Customer
mutation {
  updateCustomer(input: {
    id: 100
    assignedToTeamId: 1
    assignedToUserId: 10
  }) {
    id
    name
    assignedTeam { name }
    assignedToUser { name email }
  }
}

# Test: Get Team Customers
query {
  customers(where: { assignedToTeamId: { eq: 1 } }) {
    id
    name
    assignedToUser { name }
  }
}
```

## Next Steps

1. **Authentication Integration:** Update TeamManagementComponent to get companyId from auth service instead of hardcoded value
2. **Role-Based UI:** Show/hide Team Management menu based on user role (Admin only)
3. **Customer UI Enhancement:** Add team/user assignment fields to customer create/edit forms
4. **Dashboard Widgets:** Add team performance and hierarchy overview widgets
5. **Notifications:** Notify users when customers are assigned to them
6. **Reporting:** Add reports for team performance and customer distribution
7. **Bulk Actions:** Add ability to bulk assign customers to teams/users
8. **Team Templates:** Create team templates for quick setup

## UI Screenshots Description

### Teams Tab
- Grid of team cards showing:
  - Team name and description
  - Team type badge (color-coded)
  - Manager name
  - Member count
  - Edit, Members, Delete buttons

### Users Tab
- Table showing:
  - User name and email
  - Current manager
  - Roles (badges)
  - Direct report count
  - Edit Hierarchy button

### Hierarchy Tab
- Tree view showing:
  - Top-level users (no manager)
  - Indented direct reports
  - User avatars with initials
  - Report counts

### Modals
- Clean, modern design matching minimal aesthetic
- Form validation
- Loading states
- Error handling

## Design System Consistency

All components follow the established design patterns:
- Tailwind CSS utility classes
- Minimal, modern aesthetic
- Small font sizes (text-xs, text-sm)
- Compact spacing (py-2.5, px-4)
- Rounded corners (rounded-xl, rounded-lg)
- Subtle borders and shadows
- Blue accent color for primary actions
- Gray tones for neutral elements
- Color-coded badges for status/type indicators

## Support

For questions or issues with this implementation, refer to:
- GraphQL schema documentation in backend
- Customer assignment examples in this document
- Access control logic examples above
