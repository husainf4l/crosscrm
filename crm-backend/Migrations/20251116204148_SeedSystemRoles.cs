using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crm_backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""Name"", ""Description"", ""IsSystemRole"", ""CompanyId"", ""CreatedAt"") 
                SELECT 'Admin', 'System Administrator with full access', true, NULL, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'Admin' AND ""IsSystemRole"" = true);
                
                INSERT INTO ""Roles"" (""Name"", ""Description"", ""IsSystemRole"", ""CompanyId"", ""CreatedAt"") 
                SELECT 'Manager', 'Manager with team oversight capabilities', true, NULL, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'Manager' AND ""IsSystemRole"" = true);
                
                INSERT INTO ""Roles"" (""Name"", ""Description"", ""IsSystemRole"", ""CompanyId"", ""CreatedAt"") 
                SELECT 'SalesRep', 'Sales Representative', true, NULL, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'SalesRep' AND ""IsSystemRole"" = true);
                
                INSERT INTO ""Roles"" (""Name"", ""Description"", ""IsSystemRole"", ""CompanyId"", ""CreatedAt"") 
                SELECT 'SupportAgent', 'Customer Support Agent', true, NULL, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'SupportAgent' AND ""IsSystemRole"" = true);
                
                INSERT INTO ""Roles"" (""Name"", ""Description"", ""IsSystemRole"", ""CompanyId"", ""CreatedAt"") 
                SELECT 'AIAgent', 'AI Agent for automated processes', true, NULL, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'AIAgent' AND ""IsSystemRole"" = true);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""Roles"" 
                WHERE ""IsSystemRole"" = true 
                AND ""Name"" IN ('Admin', 'Manager', 'SalesRep', 'SupportAgent', 'AIAgent');
            ");
        }
    }
}
