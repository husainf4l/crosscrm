using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.User.Services;
using crm_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using FluentValidation;
using crm_backend.Modules.Customer.DTOs.Validators;
using crm_backend.Modules.User.DTOs.Validators;
using crm_backend.Modules.Company.DTOs.Validators;
using crm_backend.Modules.Opportunity.DTOs.Validators;
using crm_backend.Modules.Financial.DTOs.Validators;
using crm_backend.Modules.Communication.DTOs.Validators;
using crm_backend.Modules.Marketing.DTOs.Validators;
using crm_backend.Modules.Contract.DTOs.Validators;
using crm_backend.Modules.Collaboration.DTOs.Validators;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
    .AddTypeExtension<crm_backend.Modules.User.UserResolver>()
    .AddTypeExtension<crm_backend.Modules.User.UserMutation>()
    .AddTypeExtension<crm_backend.Modules.Company.CompanyResolver>()
    .AddTypeExtension<crm_backend.Modules.Company.CompanyMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerResolver>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.ContactResolver>()
    .AddTypeExtension<crm_backend.Modules.Customer.ContactMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.TicketResolver>()
    .AddTypeExtension<crm_backend.Modules.Customer.TicketMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerNoteResolver>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerNoteMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.FileAttachmentResolver>()
    .AddTypeExtension<crm_backend.Modules.Customer.FileAttachmentMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.ActivityLogMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.ActivityLogMutation.ActivityLogQuery>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerPreferenceMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerPreferenceMutation.CustomerPreferenceQuery>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerCategoryMutation>()
    .AddTypeExtension<crm_backend.Modules.Customer.CustomerCategoryMutation.CustomerCategoryQuery>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.OpportunityResolver>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.OpportunityMutation>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.PipelineStageResolver>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.PipelineStageMutation>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.LeadSourceResolver>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.LeadSourceMutation>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.ProductResolver>()
    .AddTypeExtension<crm_backend.Modules.Opportunity.ProductMutation>()
    .AddTypeExtension<crm_backend.Modules.Financial.QuoteResolver>()
    .AddTypeExtension<crm_backend.Modules.Financial.QuoteMutation>()
    .AddTypeExtension<crm_backend.Modules.Financial.InvoiceResolver>()
    .AddTypeExtension<crm_backend.Modules.Financial.InvoiceMutation>()
    .AddTypeExtension<crm_backend.Modules.Financial.PaymentResolver>()
    .AddTypeExtension<crm_backend.Modules.Financial.PaymentMutation>()
    .AddTypeExtension<crm_backend.Modules.Communication.EmailResolver>()
    .AddTypeExtension<crm_backend.Modules.Communication.EmailMutation>()
    .AddTypeExtension<crm_backend.Modules.Communication.AppointmentResolver>()
    .AddTypeExtension<crm_backend.Modules.Communication.AppointmentMutation>()
    .AddTypeExtension<crm_backend.Modules.Communication.TaskResolver>()
    .AddTypeExtension<crm_backend.Modules.Communication.TaskMutation>()
    .AddTypeExtension<crm_backend.Modules.Marketing.LeadResolver>()
    .AddTypeExtension<crm_backend.Modules.Marketing.LeadMutation>()
    .AddTypeExtension<crm_backend.Modules.Marketing.CampaignResolver>()
    .AddTypeExtension<crm_backend.Modules.Marketing.CampaignMutation>()
    .AddTypeExtension<crm_backend.Modules.Contract.ContractResolver>()
    .AddTypeExtension<crm_backend.Modules.Contract.ContractMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.TeamResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.TeamMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.RoleResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.RoleMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.AIAgentResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.AIAgentMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.CustomerWorkspaceResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.CustomerWorkspaceMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.CommunicationResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.CommunicationMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.CommunicationSubscription>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.ActivityTimelineResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.ActivityTimelineMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.AIAgentApiKeyResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.AIAgentApiKeyMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.NotificationResolver>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.NotificationMutation>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.NotificationSubscription>()
    .AddTypeExtension<crm_backend.Modules.Collaboration.SearchResolver>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();
builder.Services.AddDbContext<CrmDbContext>(options => {
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var username = Environment.GetEnvironmentVariable("DB_USERNAME");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var database = Environment.GetEnvironmentVariable("DB_DATABASE");
    var connectionString = $"Host={host};Username={username};Password={password};Database={database}";
    options.UseNpgsql(connectionString);
});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<crm_backend.Modules.Company.Services.ICompanyService, crm_backend.Modules.Company.Services.CompanyService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.ICustomerService, crm_backend.Modules.Customer.Services.CustomerService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.IContactService, crm_backend.Modules.Customer.Services.ContactService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.ITicketService, crm_backend.Modules.Customer.Services.TicketService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.ICustomerNoteService, crm_backend.Modules.Customer.Services.CustomerNoteService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.IFileUploadService, crm_backend.Modules.Customer.Services.FileUploadService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.IS3Service, crm_backend.Modules.Customer.Services.S3Service>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.IActivityLogService, crm_backend.Modules.Customer.Services.ActivityLogService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.ICustomerPreferenceService, crm_backend.Modules.Customer.Services.CustomerPreferenceService>();
builder.Services.AddScoped<crm_backend.Modules.Customer.Services.ICustomerCategoryService, crm_backend.Modules.Customer.Services.CustomerCategoryService>();

// Phase 1: Register Opportunity services
builder.Services.AddScoped<crm_backend.Modules.Opportunity.Services.IOpportunityService, crm_backend.Modules.Opportunity.Services.OpportunityService>();
builder.Services.AddScoped<crm_backend.Modules.Opportunity.Services.IPipelineStageService, crm_backend.Modules.Opportunity.Services.PipelineStageService>();
builder.Services.AddScoped<crm_backend.Modules.Opportunity.Services.ILeadSourceService, crm_backend.Modules.Opportunity.Services.LeadSourceService>();
builder.Services.AddScoped<crm_backend.Modules.Opportunity.Services.IProductService, crm_backend.Modules.Opportunity.Services.ProductService>();

// Phase 2: Register Financial services
builder.Services.AddScoped<crm_backend.Modules.Financial.Services.IQuoteService, crm_backend.Modules.Financial.Services.QuoteService>();
builder.Services.AddScoped<crm_backend.Modules.Financial.Services.IInvoiceService, crm_backend.Modules.Financial.Services.InvoiceService>();
builder.Services.AddScoped<crm_backend.Modules.Financial.Services.IPaymentService, crm_backend.Modules.Financial.Services.PaymentService>();

// Phase 3: Register Communication services
builder.Services.AddScoped<crm_backend.Modules.Communication.Services.IEmailService, crm_backend.Modules.Communication.Services.EmailService>();
builder.Services.AddScoped<crm_backend.Modules.Communication.Services.IAppointmentService, crm_backend.Modules.Communication.Services.AppointmentService>();
builder.Services.AddScoped<crm_backend.Modules.Communication.Services.ITaskService, crm_backend.Modules.Communication.Services.TaskService>();

// Phase 4: Register Marketing services
builder.Services.AddScoped<crm_backend.Modules.Marketing.Services.ILeadService, crm_backend.Modules.Marketing.Services.LeadService>();
builder.Services.AddScoped<crm_backend.Modules.Marketing.Services.ICampaignService, crm_backend.Modules.Marketing.Services.CampaignService>();

// Phase 5: Register Contract services
builder.Services.AddScoped<crm_backend.Modules.Contract.Services.IContractService, crm_backend.Modules.Contract.Services.ContractService>();
builder.Services.AddScoped<crm_backend.Modules.Contract.Services.IContractWorkflowService, crm_backend.Modules.Contract.Services.ContractWorkflowService>();

// Workflow Services
builder.Services.AddScoped<crm_backend.Modules.Opportunity.Services.IOpportunityWorkflowService, crm_backend.Modules.Opportunity.Services.OpportunityWorkflowService>();
builder.Services.AddScoped<crm_backend.Modules.Financial.Services.IQuoteWorkflowService, crm_backend.Modules.Financial.Services.QuoteWorkflowService>();
builder.Services.AddScoped<crm_backend.Modules.Marketing.Services.ILeadConversionService, crm_backend.Modules.Marketing.Services.LeadConversionService>();

// Collaboration: Register Team & Role services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.ITeamService, crm_backend.Modules.Collaboration.Services.TeamService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IRoleService, crm_backend.Modules.Collaboration.Services.RoleService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IUserRoleService, crm_backend.Modules.Collaboration.Services.UserRoleService>();

// Collaboration: Register AI Agent services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IAIAgentService, crm_backend.Modules.Collaboration.Services.AIAgentService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IAIAgentInteractionService, crm_backend.Modules.Collaboration.Services.AIAgentInteractionService>();
builder.Services.AddHttpClient<crm_backend.Modules.Collaboration.Services.IAIAgentClientService, crm_backend.Modules.Collaboration.Services.AIAgentClientService>();

// Collaboration: Register Customer Workspace services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.ICustomerWorkspaceService, crm_backend.Modules.Collaboration.Services.CustomerWorkspaceService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.ICustomerStrategyService, crm_backend.Modules.Collaboration.Services.CustomerStrategyService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.ICustomerIdeaService, crm_backend.Modules.Collaboration.Services.CustomerIdeaService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.INoteCommentService, crm_backend.Modules.Collaboration.Services.NoteCommentService>();

// Collaboration: Register Communication services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IChannelService, crm_backend.Modules.Collaboration.Services.ChannelService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IMessageService, crm_backend.Modules.Collaboration.Services.MessageService>();

// Collaboration: Register Activity Timeline services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IActivityTimelineService, crm_backend.Modules.Collaboration.Services.ActivityTimelineService>();

// Collaboration: Register AI Agent API Key & Tool services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IAIAgentApiKeyService, crm_backend.Modules.Collaboration.Services.AIAgentApiKeyService>();
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.IAIAgentToolService, crm_backend.Modules.Collaboration.Services.AIAgentToolService>();

// Collaboration: Register Notification services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.INotificationService, crm_backend.Modules.Collaboration.Services.NotificationService>();

// Collaboration: Register Search services
builder.Services.AddScoped<crm_backend.Modules.Collaboration.Services.ISearchService, crm_backend.Modules.Collaboration.Services.SearchService>();

// Register error handling service
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCompanyDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOpportunityDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuoteDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEmailDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateLeadDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateContractDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTeamDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<crm_backend.Modules.Collaboration.DTOs.Validators.CreateAIAgentDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<crm_backend.Modules.Collaboration.DTOs.Validators.CreateCustomerWorkspaceDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<crm_backend.Modules.Collaboration.DTOs.Validators.CreateChannelDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<crm_backend.Modules.Collaboration.DTOs.Validators.CreateActivityTimelineDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<crm_backend.Modules.Collaboration.DTOs.Validators.CreateAIAgentApiKeyDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<crm_backend.Modules.Collaboration.DTOs.Validators.CreateNotificationDtoValidator>();

// Configure JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "default-secret-key-for-development";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "crm-backend";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "crm-client";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// Add CORS to allow all domains
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseWebSockets(); // Required for GraphQL subscriptions

// API Key authentication middleware (before JWT auth for API routes)
app.UseMiddleware<crm_backend.Modules.Collaboration.Middleware.AIAgentApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

// Apply automatic migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    try
    {
        Console.WriteLine("🔄 Applying automatic database migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations applied successfully.");
        
        // Seed permissions
        Console.WriteLine("🔄 Seeding permissions...");
        var permissionSeeder = new crm_backend.Modules.Collaboration.Services.PermissionSeeder(dbContext);
        await permissionSeeder.SeedPermissionsAsync();
        Console.WriteLine("✅ Permissions seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
        Console.WriteLine("⚠️  Application will continue. Database may not be fully up to date.");
    }
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
