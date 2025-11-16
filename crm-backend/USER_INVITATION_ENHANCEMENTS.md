# User Invitation System Enhancements

## Overview
Enhanced the user invitation system with better error handling, validation, and automatic role assignment to provide clearer feedback when invitation operations fail.

## Key Enhancements

### 1. Enhanced Error Handling
- **Database Constraint Violations**: Added specific error messages for different types of foreign key constraint violations
- **PostgreSQL Integration**: Added specific handling for Npgsql.PostgresException to provide meaningful error messages
- **Error Categorization**: Different error messages for:
  - Missing roles (FK_UserInvitations_Roles_RoleId)
  - Missing teams (FK_UserInvitations_Teams_TeamId)
  - Missing companies (FK_UserInvitations_Companies_CompanyId)
  - Missing inviting users (FK_UserInvitations_Users_InvitedByUserId)

### 2. Pre-validation Checks
Added validation before attempting to save invitations:
- **Role Validation**: Checks if the specified RoleId exists before creating invitation
- **Team Validation**: Verifies TeamId exists if specified
- **Company Validation**: Ensures CompanyId is valid

### 3. System Roles Management
- **Automatic Role Creation**: Added `EnsureSystemRolesExistAsync()` method to create system roles if they don't exist
- **Default Role Assignment**: If no role is specified in invitation, automatically assigns the SalesRep role
- **System Roles Available**:
  - Admin: System Administrator with full access
  - Manager: Manager with team oversight capabilities
  - SalesRep: Sales Representative (default for invitations)
  - SupportAgent: Customer Support Agent
  - AIAgent: AI Agent for automated processes

### 4. Database Migration
Created migration `20251116204148_SeedSystemRoles` to populate the database with default system roles:
```sql
INSERT INTO "Roles" ("Name", "Description", "IsSystemRole", "CompanyId", "CreatedAt") 
SELECT 'SalesRep', 'Sales Representative', true, NULL, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Roles" WHERE "Name" = 'SalesRep' AND "IsSystemRole" = true);
```

## Error Response Examples

### Before Enhancement
```json
{
  "errors": [
    {
      "message": "Failed to invite user: An error occurred while saving the entity changes. See the inner exception for details.",
      "path": ["inviteUser"]
    }
  ],
  "data": null
}
```

### After Enhancement
```json
{
  "errors": [
    {
      "message": "Failed to invite user: The specified role does not exist",
      "path": ["inviteUser"]
    }
  ],
  "data": null
}
```

## Implementation Details

### Enhanced Service Method
```csharp
public async Task<UserInvitationDto> InviteUserAsync(InviteUserDto inviteDto, int invitedByUserId)
{
    try
    {
        // ... existing validation ...
        
        // Validate role exists
        if (inviteDto.RoleId.HasValue)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == inviteDto.RoleId.Value);
            if (!roleExists)
            {
                throw new InvalidOperationException($"Role with ID {inviteDto.RoleId.Value} not found");
            }
        }
        else
        {
            // If no role specified, assign default SalesRep role
            await EnsureSystemRolesExistAsync();
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == SystemRoles.SalesRep && r.IsSystemRole);
            if (defaultRole != null)
            {
                inviteDto.RoleId = defaultRole.Id;
            }
        }
        
        // ... rest of the method ...
    }
    catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
    {
        var errorMessage = pgEx.SqlState switch
        {
            "23503" => pgEx.ConstraintName switch
            {
                "FK_UserInvitations_Roles_RoleId" => "The specified role does not exist",
                "FK_UserInvitations_Teams_TeamId" => "The specified team does not exist", 
                "FK_UserInvitations_Companies_CompanyId" => "The specified company does not exist",
                "FK_UserInvitations_Users_InvitedByUserId" => "The inviting user does not exist",
                _ => "A referenced entity does not exist"
            },
            "23505" => "An invitation with these details already exists",
            _ => "A database constraint was violated"
        };
        
        _logger.LogError(ex, "Database constraint violation when inviting user {Email} to company {CompanyId}: {Error}", 
            inviteDto.Email, inviteDto.CompanyId, errorMessage);
        throw new InvalidOperationException($"Failed to invite user: {errorMessage}");
    }
}
```

## Benefits

1. **Better User Experience**: Clear, actionable error messages instead of generic database errors
2. **Automatic Recovery**: System automatically ensures required roles exist
3. **Simplified Frontend**: Frontend doesn't need to handle missing role scenarios
4. **Robust Error Handling**: Specific error messages for different constraint violations
5. **Logging**: Enhanced logging for troubleshooting and monitoring

## Usage

The invitation system now handles missing roles gracefully:

```graphql
mutation InviteUser($input: InviteUserDtoInput!) {
  inviteUser(input: $input) {
    id
    email
    status
    roleName
    companyName
  }
}
```

Variables:
```json
{
  "input": {
    "email": "user@example.com",
    "companyId": 1,
    // roleId is optional - will default to SalesRep if not provided
    "notes": "Welcome to the team!"
  }
}
```

## Future Enhancements

1. **Permission-based Role Assignment**: Allow only certain roles to assign specific roles
2. **Role Hierarchy**: Implement role hierarchy for permission inheritance
3. **Bulk Invitations**: Support for inviting multiple users at once
4. **Custom Roles**: Allow companies to create their own custom roles
5. **Invitation Templates**: Predefined invitation templates for different roles