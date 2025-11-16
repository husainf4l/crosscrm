using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace crm_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuoteConvertedToInvoiceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuoteId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToTeamId",
                table: "Quotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSystemId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSystemType",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinancialIntegrationId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncError",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SyncStatus",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToTeamId",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertedFromLeadId",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadScore",
                table: "Leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToTeamId",
                table: "Invoices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSystemId",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSystemType",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinancialIntegrationId",
                table: "Invoices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncError",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SyncStatus",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "Emails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuoteId",
                table: "Emails",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToTeamId",
                table: "Customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertedFromLeadId",
                table: "Customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "Contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuoteId",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityTimelines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "integer", nullable: true),
                    AIAgentId = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTimelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTimelines_AIAgents_AIAgentId",
                        column: x => x.AIAgentId,
                        principalTable: "AIAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActivityTimelines_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityTimelines_Users_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AIAgentApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    KeyName = table.Column<string>(type: "text", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "text", nullable: false),
                    KeyPrefix = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "integer", nullable: true),
                    RateLimitPerHour = table.Column<int>(type: "integer", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAgentApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAgentApiKeys_AIAgents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AIAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIAgentApiKeys_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIAgentApiKeys_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIAgentTools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    ToolName = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Parameters = table.Column<string>(type: "text", nullable: false),
                    Permissions = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAgentTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAgentTools_AIAgents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AIAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIAgentTools_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIAgentTools_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SystemType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ConnectionSettings = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SyncFrequency = table.Column<string>(type: "text", nullable: false),
                    SyncEntities = table.Column<List<string>>(type: "text[]", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncError = table.Column<string>(type: "text", nullable: true),
                    LastSyncCount = table.Column<int>(type: "integer", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisconnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialIntegrations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    NotificationType = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    ActionUrl = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityFeeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityFeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityFeeds_ActivityTimelines_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "ActivityTimelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityFeeds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIAgentApiKeyUsageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiKeyId = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: true),
                    ResponseStatus = table.Column<int>(type: "integer", nullable: false),
                    ResponseTime = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAgentApiKeyUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAgentApiKeyUsageLogs_AIAgentApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "AIAgentApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIAgentToolUsageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ToolId = table.Column<int>(type: "integer", nullable: false),
                    ApiKeyId = table.Column<int>(type: "integer", nullable: false),
                    Parameters = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExecutionTime = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAgentToolUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAgentToolUsageLogs_AIAgentApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "AIAgentApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIAgentToolUsageLogs_AIAgentTools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "AIAgentTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FinancialIntegrationId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    ExternalEntityId = table.Column<string>(type: "text", nullable: true),
                    SyncStatus = table.Column<int>(type: "integer", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SyncDetails = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncHistories_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncHistories_FinancialIntegrations_FinancialIntegrationId",
                        column: x => x.FinancialIntegrationId,
                        principalTable: "FinancialIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ContractId",
                table: "Tasks",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_InvoiceId",
                table: "Tasks",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_QuoteId",
                table: "Tasks",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_AssignedToTeamId",
                table: "Quotes",
                column: "AssignedToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedByUserId",
                table: "Payments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FinancialIntegrationId",
                table: "Payments",
                column: "FinancialIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AssignedToTeamId",
                table: "Opportunities",
                column: "AssignedToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ConvertedFromLeadId",
                table: "Opportunities",
                column: "ConvertedFromLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AssignedToTeamId",
                table: "Invoices",
                column: "AssignedToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_FinancialIntegrationId",
                table: "Invoices",
                column: "FinancialIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_InvoiceId",
                table: "Emails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_QuoteId",
                table: "Emails",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AssignedToTeamId",
                table: "Customers",
                column: "AssignedToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ConvertedFromLeadId",
                table: "Customers",
                column: "ConvertedFromLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_InvoiceId",
                table: "Contracts",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_QuoteId",
                table: "Appointments",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityFeeds_ActivityId",
                table: "ActivityFeeds",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityFeeds_UserId_ActivityId",
                table: "ActivityFeeds",
                columns: new[] { "UserId", "ActivityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTimelines_AIAgentId",
                table: "ActivityTimelines",
                column: "AIAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTimelines_CompanyId",
                table: "ActivityTimelines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTimelines_PerformedByUserId",
                table: "ActivityTimelines",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentApiKeys_AgentId",
                table: "AIAgentApiKeys",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentApiKeys_CompanyId",
                table: "AIAgentApiKeys",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentApiKeys_CreatedByUserId",
                table: "AIAgentApiKeys",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentApiKeyUsageLogs_ApiKeyId",
                table: "AIAgentApiKeyUsageLogs",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentTools_AgentId_ToolName",
                table: "AIAgentTools",
                columns: new[] { "AgentId", "ToolName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentTools_CompanyId",
                table: "AIAgentTools",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentTools_CreatedByUserId",
                table: "AIAgentTools",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentToolUsageLogs_ApiKeyId",
                table: "AIAgentToolUsageLogs",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentToolUsageLogs_ToolId",
                table: "AIAgentToolUsageLogs",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialIntegrations_CompanyId",
                table: "FinancialIntegrations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_CompanyId",
                table: "NotificationPreferences",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_NotificationType_Channel",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "NotificationType", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncHistories_CompanyId",
                table: "SyncHistories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncHistories_FinancialIntegrationId",
                table: "SyncHistories",
                column: "FinancialIntegrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Quotes_QuoteId",
                table: "Appointments",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Invoices_InvoiceId",
                table: "Contracts",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Leads_ConvertedFromLeadId",
                table: "Customers",
                column: "ConvertedFromLeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Teams_AssignedToTeamId",
                table: "Customers",
                column: "AssignedToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Invoices_InvoiceId",
                table: "Emails",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Quotes_QuoteId",
                table: "Emails",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_FinancialIntegrations_FinancialIntegrationId",
                table: "Invoices",
                column: "FinancialIntegrationId",
                principalTable: "FinancialIntegrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Teams_AssignedToTeamId",
                table: "Invoices",
                column: "AssignedToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Leads_ConvertedFromLeadId",
                table: "Opportunities",
                column: "ConvertedFromLeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Teams_AssignedToTeamId",
                table: "Opportunities",
                column: "AssignedToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_FinancialIntegrations_FinancialIntegrationId",
                table: "Payments",
                column: "FinancialIntegrationId",
                principalTable: "FinancialIntegrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_CreatedByUserId",
                table: "Payments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Teams_AssignedToTeamId",
                table: "Quotes",
                column: "AssignedToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Contracts_ContractId",
                table: "Tasks",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Invoices_InvoiceId",
                table: "Tasks",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Quotes_QuoteId",
                table: "Tasks",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Quotes_QuoteId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Invoices_InvoiceId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Leads_ConvertedFromLeadId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Teams_AssignedToTeamId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Invoices_InvoiceId",
                table: "Emails");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Quotes_QuoteId",
                table: "Emails");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_FinancialIntegrations_FinancialIntegrationId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Teams_AssignedToTeamId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Leads_ConvertedFromLeadId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Teams_AssignedToTeamId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_FinancialIntegrations_FinancialIntegrationId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_CreatedByUserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Teams_AssignedToTeamId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Contracts_ContractId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Invoices_InvoiceId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Quotes_QuoteId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "ActivityFeeds");

            migrationBuilder.DropTable(
                name: "AIAgentApiKeyUsageLogs");

            migrationBuilder.DropTable(
                name: "AIAgentToolUsageLogs");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SyncHistories");

            migrationBuilder.DropTable(
                name: "ActivityTimelines");

            migrationBuilder.DropTable(
                name: "AIAgentApiKeys");

            migrationBuilder.DropTable(
                name: "AIAgentTools");

            migrationBuilder.DropTable(
                name: "FinancialIntegrations");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ContractId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_InvoiceId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_QuoteId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_AssignedToTeamId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedByUserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_FinancialIntegrationId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_AssignedToTeamId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_ConvertedFromLeadId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AssignedToTeamId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_FinancialIntegrationId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Emails_InvoiceId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Emails_QuoteId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AssignedToTeamId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ConvertedFromLeadId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_InvoiceId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_QuoteId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignedToTeamId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExternalSystemId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExternalSystemType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FinancialIntegrationId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SyncError",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AssignedToTeamId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ConvertedFromLeadId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LeadScore",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AssignedToTeamId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExternalSystemId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExternalSystemType",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FinancialIntegrationId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SyncError",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "AssignedToTeamId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ConvertedFromLeadId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "Appointments");
        }
    }
}
