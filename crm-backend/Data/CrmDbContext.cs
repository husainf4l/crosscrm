using Microsoft.EntityFrameworkCore;
using crm_backend.Modules.User;
using crm_backend.Modules.Company;
using crm_backend.Modules.Customer;
using crm_backend.Modules.Opportunity;
using crm_backend.Modules.Financial;
using crm_backend.Modules.Communication;
using crm_backend.Modules.Marketing;
using crm_backend.Modules.Contract;
using crm_backend.Modules.Collaboration;

namespace crm_backend.Data;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<crm_backend.Modules.User.UserCompany> UserCompanies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<CustomerNote> CustomerNotes { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<CustomerPreference> CustomerPreferences { get; set; }
    public DbSet<CustomerCategory> CustomerCategories { get; set; }
    public DbSet<CustomerCategoryMapping> CustomerCategoryMappings { get; set; }
    
    // Phase 1: Sales Pipeline models
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<PipelineStage> PipelineStages { get; set; }
    public DbSet<LeadSource> LeadSources { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OpportunityProduct> OpportunityProducts { get; set; }
    
    // Phase 4: Enterprise CRM Patterns (TODO: Implement these models)
    // public DbSet<Opportunity.Territory> Territories { get; set; }
    // public DbSet<Opportunity.ProductCategory> ProductCategories { get; set; }
    // public DbSet<Opportunity.ProductFamily> ProductFamilies { get; set; }
    // public DbSet<Opportunity.PriceBook> PriceBooks { get; set; }
    // public DbSet<Opportunity.PriceBookEntry> PriceBookEntries { get; set; }
    // public DbSet<Opportunity.SalesForecast> SalesForecasts { get; set; }
    // public DbSet<Opportunity.ForecastItem> ForecastItems { get; set; }
    // public DbSet<Customer.Account> Accounts { get; set; }
    // public DbSet<Collaboration.ApprovalProcess> ApprovalProcesses { get; set; }
    // public DbSet<Collaboration.ApprovalStep> ApprovalSteps { get; set; }
    // public DbSet<Collaboration.ApprovalRequest> ApprovalRequests { get; set; }
    // public DbSet<Collaboration.ApprovalResponse> ApprovalResponses { get; set; }
    // public DbSet<Communication.DocumentTemplate> DocumentTemplates { get; set; }
    // public DbSet<Communication.EmailTemplate> EmailTemplates { get; set; }
    
    // Phase 2: Financial models
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteLineItem> QuoteLineItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    // Financial Integration models
    public DbSet<FinancialIntegration> FinancialIntegrations { get; set; }
    public DbSet<SyncHistory> SyncHistories { get; set; }
    
    // Phase 3: Communication models
    public DbSet<Email> Emails { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentAttendee> AppointmentAttendees { get; set; }
    public DbSet<crm_backend.Modules.Communication.Task> Tasks { get; set; }
    
    // Phase 4: Marketing models
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignMember> CampaignMembers { get; set; }
    
    // Phase 5: Contract models
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractLineItem> ContractLineItems { get; set; }
    
    // Collaboration: Team & Role Management
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    // Collaboration: AI Agent Management
    public DbSet<AIAgent> AIAgents { get; set; }
    public DbSet<AIAgentAssignment> AIAgentAssignments { get; set; }
    public DbSet<AIAgentInteraction> AIAgentInteractions { get; set; }
    
    // Collaboration: Customer Workspace
    public DbSet<CustomerWorkspace> CustomerWorkspaces { get; set; }
    public DbSet<CustomerStrategy> CustomerStrategies { get; set; }
    public DbSet<CustomerIdea> CustomerIdeas { get; set; }
    public DbSet<IdeaVote> IdeaVotes { get; set; }
    public DbSet<NoteComment> NoteComments { get; set; }
    
    // Collaboration: Real-Time Communication
    public DbSet<Channel> Channels { get; set; }
    public DbSet<ChannelMember> ChannelMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageMention> MessageMentions { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    
    // Collaboration: Activity Timeline
    public DbSet<ActivityTimeline> ActivityTimelines { get; set; }
    public DbSet<ActivityFeed> ActivityFeeds { get; set; }
    
    // Collaboration: AI Agent API Keys & Tools
    public DbSet<AIAgentApiKey> AIAgentApiKeys { get; set; }
    public DbSet<AIAgentApiKeyUsageLog> AIAgentApiKeyUsageLogs { get; set; }
    public DbSet<AIAgentTool> AIAgentTools { get; set; }
    public DbSet<AIAgentToolUsageLog> AIAgentToolUsageLogs { get; set; }
    
    // Collaboration: Notifications
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many relationship
        modelBuilder.Entity<crm_backend.Modules.User.UserCompany>()
            .HasKey(uc => new { uc.UserId, uc.CompanyId });

        modelBuilder.Entity<crm_backend.Modules.User.UserCompany>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCompanies)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<crm_backend.Modules.User.UserCompany>()
            .HasOne(uc => uc.Company)
            .WithMany(c => c.UserCompanies)
            .HasForeignKey(uc => uc.CompanyId);

        // Configure active company relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Customer-Company relationship
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.AssignedTeam)
            .WithMany()
            .HasForeignKey(c => c.AssignedToTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.ConvertedFromLead)
            .WithMany()
            .HasForeignKey(c => c.ConvertedFromLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Customer-Category many-to-many relationship
        modelBuilder.Entity<CustomerCategoryMapping>()
            .HasKey(ccm => new { ccm.CustomerId, ccm.CategoryId });

        modelBuilder.Entity<CustomerCategoryMapping>()
            .HasOne(ccm => ccm.Customer)
            .WithMany(c => c.CategoryMappings)
            .HasForeignKey(ccm => ccm.CustomerId);

        modelBuilder.Entity<CustomerCategoryMapping>()
            .HasOne(ccm => ccm.Category)
            .WithMany(c => c.CustomerMappings)
            .HasForeignKey(ccm => ccm.CategoryId);

        // Configure Opportunity relationships
        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Company)
            .WithMany()
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.PipelineStage)
            .WithMany(ps => ps.Opportunities)
            .HasForeignKey(o => o.PipelineStageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.AssignedUser)
            .WithMany()
            .HasForeignKey(o => o.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Source)
            .WithMany(ls => ls.Opportunities)
            .HasForeignKey(o => o.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.AssignedTeam)
            .WithMany()
            .HasForeignKey(o => o.AssignedToTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.ConvertedFromLead)
            .WithMany()
            .HasForeignKey(o => o.ConvertedFromLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Opportunity>()
            .HasMany(o => o.Quotes)
            .WithOne(q => q.Opportunity)
            .HasForeignKey(q => q.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Opportunity>()
            .HasMany(o => o.Contracts)
            .WithOne(c => c.Opportunity)
            .HasForeignKey(c => c.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure PipelineStage relationship
        modelBuilder.Entity<PipelineStage>()
            .HasOne(ps => ps.Company)
            .WithMany()
            .HasForeignKey(ps => ps.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure LeadSource relationship
        modelBuilder.Entity<LeadSource>()
            .HasOne(ls => ls.Company)
            .WithMany()
            .HasForeignKey(ls => ls.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Product relationship
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure OpportunityProduct junction
        modelBuilder.Entity<OpportunityProduct>()
            .HasOne(op => op.Opportunity)
            .WithMany(o => o.Products)
            .HasForeignKey(op => op.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OpportunityProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OpportunityProducts)
            .HasForeignKey(op => op.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Quote relationships
        modelBuilder.Entity<Quote>()
            .HasOne(q => q.Company)
            .WithMany()
            .HasForeignKey(q => q.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quote>()
            .HasOne(q => q.Customer)
            .WithMany()
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Opportunity relationship configured above in Opportunity section

        modelBuilder.Entity<Quote>()
            .HasOne(q => q.CreatedByUser)
            .WithMany()
            .HasForeignKey(q => q.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quote>()
            .HasOne(q => q.AssignedTeam)
            .WithMany()
            .HasForeignKey(q => q.AssignedToTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure QuoteLineItem relationships
        modelBuilder.Entity<QuoteLineItem>()
            .HasOne(qli => qli.Quote)
            .WithMany(q => q.LineItems)
            .HasForeignKey(qli => qli.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuoteLineItem>()
            .HasOne(qli => qli.Product)
            .WithMany()
            .HasForeignKey(qli => qli.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Invoice relationships
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Company)
            .WithMany()
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Quote)
            .WithMany()
            .HasForeignKey(i => i.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Opportunity)
            .WithMany()
            .HasForeignKey(i => i.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.AssignedTeam)
            .WithMany()
            .HasForeignKey(i => i.AssignedToTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Contracts)
            .WithOne(c => c.Invoice)
            .HasForeignKey(c => c.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.CreatedByUser)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure InvoiceLineItem relationships
        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(ili => ili.Invoice)
            .WithMany(i => i.LineItems)
            .HasForeignKey(ili => ili.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(ili => ili.Product)
            .WithMany()
            .HasForeignKey(ili => ili.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Payment relationships
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.ReceivedByUser)
            .WithMany()
            .HasForeignKey(p => p.ReceivedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.FinancialIntegration)
            .WithMany(fi => fi.Payments)
            .HasForeignKey(p => p.FinancialIntegrationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Invoice FinancialIntegration relationship
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.FinancialIntegration)
            .WithMany(fi => fi.Invoices)
            .HasForeignKey(i => i.FinancialIntegrationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure FinancialIntegration relationships
        modelBuilder.Entity<FinancialIntegration>()
            .HasOne(fi => fi.Company)
            .WithMany()
            .HasForeignKey(fi => fi.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure SyncHistory relationships
        modelBuilder.Entity<SyncHistory>()
            .HasOne(sh => sh.FinancialIntegration)
            .WithMany(fi => fi.SyncHistory)
            .HasForeignKey(sh => sh.FinancialIntegrationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SyncHistory>()
            .HasOne(sh => sh.Company)
            .WithMany()
            .HasForeignKey(sh => sh.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Email relationships
        modelBuilder.Entity<Email>()
            .HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.Contact)
            .WithMany()
            .HasForeignKey(e => e.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.Opportunity)
            .WithMany()
            .HasForeignKey(e => e.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.Quote)
            .WithMany()
            .HasForeignKey(e => e.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.Invoice)
            .WithMany()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.Ticket)
            .WithMany()
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.SentByUser)
            .WithMany()
            .HasForeignKey(e => e.SentByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Email>()
            .HasOne(e => e.ParentEmail)
            .WithMany(e => e.Replies)
            .HasForeignKey(e => e.ParentEmailId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Appointment relationships
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Contact)
            .WithMany()
            .HasForeignKey(a => a.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Opportunity)
            .WithMany()
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Quote)
            .WithMany()
            .HasForeignKey(a => a.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AppointmentAttendee relationships
        modelBuilder.Entity<AppointmentAttendee>()
            .HasOne(aa => aa.Appointment)
            .WithMany(a => a.Attendees)
            .HasForeignKey(aa => aa.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppointmentAttendee>()
            .HasOne(aa => aa.User)
            .WithMany()
            .HasForeignKey(aa => aa.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppointmentAttendee>()
            .HasOne(aa => aa.Contact)
            .WithMany()
            .HasForeignKey(aa => aa.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Task relationships
        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Company)
            .WithMany()
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Contact)
            .WithMany()
            .HasForeignKey(t => t.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Opportunity)
            .WithMany()
            .HasForeignKey(t => t.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Quote)
            .WithMany()
            .HasForeignKey(t => t.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Invoice)
            .WithMany()
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Contract)
            .WithMany()
            .HasForeignKey(t => t.ContractId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.Ticket)
            .WithMany()
            .HasForeignKey(t => t.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<crm_backend.Modules.Communication.Task>()
            .HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Lead relationships
        modelBuilder.Entity<Lead>()
            .HasOne(l => l.Company)
            .WithMany()
            .HasForeignKey(l => l.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lead>()
            .HasOne(l => l.Source)
            .WithMany()
            .HasForeignKey(l => l.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Lead>()
            .HasOne(l => l.AssignedUser)
            .WithMany()
            .HasForeignKey(l => l.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Lead>()
            .HasOne(l => l.ConvertedToCustomer)
            .WithMany()
            .HasForeignKey(l => l.ConvertedToCustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Lead>()
            .HasOne(l => l.ConvertedToOpportunity)
            .WithMany()
            .HasForeignKey(l => l.ConvertedToOpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Lead>()
            .HasOne(l => l.ConvertedByUser)
            .WithMany()
            .HasForeignKey(l => l.ConvertedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Campaign relationships
        modelBuilder.Entity<Campaign>()
            .HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Campaign>()
            .HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure CampaignMember relationships
        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.Campaign)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.Lead)
            .WithMany(l => l.CampaignMembers)
            .HasForeignKey(cm => cm.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.Customer)
            .WithMany()
            .HasForeignKey(cm => cm.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.Contact)
            .WithMany()
            .HasForeignKey(cm => cm.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Contract relationships
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Opportunity relationship configured above in Opportunity section
        
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Invoice)
            .WithMany(i => i.Contracts)
            .HasForeignKey(c => c.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Account)
            .WithMany(a => a.Contracts)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ContractLineItem relationships
        modelBuilder.Entity<ContractLineItem>()
            .HasOne(li => li.Contract)
            .WithMany(c => c.LineItems)
            .HasForeignKey(li => li.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ContractLineItem>()
            .HasOne(li => li.Product)
            .WithMany()
            .HasForeignKey(li => li.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Team relationships
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Company)
            .WithMany()
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Manager)
            .WithMany()
            .HasForeignKey(t => t.ManagerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure TeamMember relationships
        modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.User)
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Role relationships
        modelBuilder.Entity<Role>()
            .HasOne(r => r.Company)
            .WithMany()
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure UserRole relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Company)
            .WithMany()
            .HasForeignKey(ur => ur.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.AssignedByUser)
            .WithMany()
            .HasForeignKey(ur => ur.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure RolePermission relationships
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AIAgent relationships
        modelBuilder.Entity<AIAgent>()
            .HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIAgent>()
            .HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AIAgentAssignment relationships
        modelBuilder.Entity<AIAgentAssignment>()
            .HasOne(aa => aa.Agent)
            .WithMany(a => a.Assignments)
            .HasForeignKey(aa => aa.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIAgentAssignment>()
            .HasOne(aa => aa.Company)
            .WithMany()
            .HasForeignKey(aa => aa.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIAgentAssignment>()
            .HasOne(aa => aa.AssignedByUser)
            .WithMany()
            .HasForeignKey(aa => aa.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AIAgentInteraction relationships
        modelBuilder.Entity<AIAgentInteraction>()
            .HasOne(ai => ai.Agent)
            .WithMany(a => a.Interactions)
            .HasForeignKey(ai => ai.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIAgentInteraction>()
            .HasOne(ai => ai.Company)
            .WithMany()
            .HasForeignKey(ai => ai.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIAgentInteraction>()
            .HasOne(ai => ai.User)
            .WithMany()
            .HasForeignKey(ai => ai.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure CustomerWorkspace relationships
        modelBuilder.Entity<CustomerWorkspace>()
            .HasOne(cw => cw.Customer)
            .WithMany()
            .HasForeignKey(cw => cw.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerWorkspace>()
            .HasOne(cw => cw.Company)
            .WithMany()
            .HasForeignKey(cw => cw.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerWorkspace>()
            .HasOne(cw => cw.Team)
            .WithMany()
            .HasForeignKey(cw => cw.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CustomerWorkspace>()
            .HasOne(cw => cw.LastUpdatedByUser)
            .WithMany()
            .HasForeignKey(cw => cw.LastUpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure CustomerStrategy relationships
        modelBuilder.Entity<CustomerStrategy>()
            .HasOne(cs => cs.Customer)
            .WithMany()
            .HasForeignKey(cs => cs.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerStrategy>()
            .HasOne(cs => cs.Company)
            .WithMany()
            .HasForeignKey(cs => cs.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerStrategy>()
            .HasOne(cs => cs.Workspace)
            .WithMany(w => w.Strategies)
            .HasForeignKey(cs => cs.WorkspaceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CustomerStrategy>()
            .HasOne(cs => cs.CreatedByUser)
            .WithMany()
            .HasForeignKey(cs => cs.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerStrategy>()
            .HasOne(cs => cs.AssignedTeam)
            .WithMany()
            .HasForeignKey(cs => cs.AssignedToTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure CustomerIdea relationships
        modelBuilder.Entity<CustomerIdea>()
            .HasOne(ci => ci.Customer)
            .WithMany()
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerIdea>()
            .HasOne(ci => ci.Company)
            .WithMany()
            .HasForeignKey(ci => ci.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerIdea>()
            .HasOne(ci => ci.Workspace)
            .WithMany(w => w.Ideas)
            .HasForeignKey(ci => ci.WorkspaceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CustomerIdea>()
            .HasOne(ci => ci.CreatedByUser)
            .WithMany()
            .HasForeignKey(ci => ci.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure IdeaVote relationships
        modelBuilder.Entity<IdeaVote>()
            .HasOne(iv => iv.Idea)
            .WithMany(i => i.Votes)
            .HasForeignKey(iv => iv.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IdeaVote>()
            .HasOne(iv => iv.User)
            .WithMany()
            .HasForeignKey(iv => iv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one vote per user per idea
        modelBuilder.Entity<IdeaVote>()
            .HasIndex(iv => new { iv.IdeaId, iv.UserId })
            .IsUnique();

        // Configure NoteComment relationships
        modelBuilder.Entity<NoteComment>()
            .HasOne(nc => nc.Company)
            .WithMany()
            .HasForeignKey(nc => nc.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NoteComment>()
            .HasOne(nc => nc.CreatedByUser)
            .WithMany()
            .HasForeignKey(nc => nc.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NoteComment>()
            .HasOne(nc => nc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(nc => nc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Channel relationships
        modelBuilder.Entity<Channel>()
            .HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Channel>()
            .HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Channel>()
            .HasOne(c => c.Team)
            .WithMany()
            .HasForeignKey(c => c.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Channel>()
            .HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure ChannelMember relationships
        modelBuilder.Entity<ChannelMember>()
            .HasOne(cm => cm.Channel)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChannelMember>()
            .HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one membership per user per channel
        modelBuilder.Entity<ChannelMember>()
            .HasIndex(cm => new { cm.ChannelId, cm.UserId })
            .IsUnique();

        // Configure Message relationships
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Channel)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Company)
            .WithMany()
            .HasForeignKey(m => m.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.CreatedByUser)
            .WithMany()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.AIAgent)
            .WithMany()
            .HasForeignKey(m => m.AIAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ParentMessage)
            .WithMany(pm => pm.ThreadReplies)
            .HasForeignKey(m => m.ParentMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure MessageMention relationships
        modelBuilder.Entity<MessageMention>()
            .HasOne(mm => mm.Message)
            .WithMany(m => m.Mentions)
            .HasForeignKey(mm => mm.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessageMention>()
            .HasOne(mm => mm.Company)
            .WithMany()
            .HasForeignKey(mm => mm.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MessageMention>()
            .HasOne(mm => mm.MentionedUser)
            .WithMany()
            .HasForeignKey(mm => mm.MentionedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MessageMention>()
            .HasOne(mm => mm.MentionedTeam)
            .WithMany()
            .HasForeignKey(mm => mm.MentionedTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MessageMention>()
            .HasOne(mm => mm.MentionedAIAgent)
            .WithMany()
            .HasForeignKey(mm => mm.MentionedAIAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure MessageAttachment relationships
        modelBuilder.Entity<MessageAttachment>()
            .HasOne(ma => ma.Message)
            .WithMany(m => m.Attachments)
            .HasForeignKey(ma => ma.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessageAttachment>()
            .HasOne(ma => ma.Company)
            .WithMany()
            .HasForeignKey(ma => ma.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MessageAttachment>()
            .HasOne(ma => ma.UploadedByUser)
            .WithMany()
            .HasForeignKey(ma => ma.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ActivityTimeline relationships
        modelBuilder.Entity<ActivityTimeline>()
            .HasOne(at => at.Company)
            .WithMany()
            .HasForeignKey(at => at.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivityTimeline>()
            .HasOne(at => at.PerformedByUser)
            .WithMany()
            .HasForeignKey(at => at.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ActivityTimeline>()
            .HasOne(at => at.AIAgent)
            .WithMany()
            .HasForeignKey(at => at.AIAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure ActivityFeed relationships
        modelBuilder.Entity<ActivityFeed>()
            .HasOne(af => af.User)
            .WithMany()
            .HasForeignKey(af => af.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivityFeed>()
            .HasOne(af => af.Activity)
            .WithMany(a => a.ActivityFeeds)
            .HasForeignKey(af => af.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one feed entry per user per activity
        modelBuilder.Entity<ActivityFeed>()
            .HasIndex(af => new { af.UserId, af.ActivityId })
            .IsUnique();

        // Configure AIAgentApiKey relationships
        modelBuilder.Entity<AIAgentApiKey>()
            .HasOne(ak => ak.Agent)
            .WithMany()
            .HasForeignKey(ak => ak.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIAgentApiKey>()
            .HasOne(ak => ak.Company)
            .WithMany()
            .HasForeignKey(ak => ak.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIAgentApiKey>()
            .HasOne(ak => ak.CreatedByUser)
            .WithMany()
            .HasForeignKey(ak => ak.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AIAgentApiKeyUsageLog relationships
        modelBuilder.Entity<AIAgentApiKeyUsageLog>()
            .HasOne(log => log.ApiKey)
            .WithMany(ak => ak.UsageLogs)
            .HasForeignKey(log => log.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure AIAgentTool relationships
        modelBuilder.Entity<AIAgentTool>()
            .HasOne(t => t.Agent)
            .WithMany()
            .HasForeignKey(t => t.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIAgentTool>()
            .HasOne(t => t.Company)
            .WithMany()
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIAgentTool>()
            .HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: tool name must be unique per agent
        modelBuilder.Entity<AIAgentTool>()
            .HasIndex(t => new { t.AgentId, t.ToolName })
            .IsUnique();

        // Configure AIAgentToolUsageLog relationships
        modelBuilder.Entity<AIAgentToolUsageLog>()
            .HasOne(log => log.Tool)
            .WithMany(t => t.UsageLogs)
            .HasForeignKey(log => log.ToolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIAgentToolUsageLog>()
            .HasOne(log => log.ApiKey)
            .WithMany()
            .HasForeignKey(log => log.ApiKeyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Notification relationships
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Company)
            .WithMany()
            .HasForeignKey(n => n.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for efficient querying
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        // Configure NotificationPreference relationships
        modelBuilder.Entity<NotificationPreference>()
            .HasOne(np => np.User)
            .WithMany()
            .HasForeignKey(np => np.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NotificationPreference>()
            .HasOne(np => np.Company)
            .WithMany()
            .HasForeignKey(np => np.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one preference per user per type per channel
        modelBuilder.Entity<NotificationPreference>()
            .HasIndex(np => new { np.UserId, np.NotificationType, np.Channel })
            .IsUnique();

        // Phase 4: Enterprise CRM Patterns - TODO: Implement Account, Territory, ProductCategory, etc. models
        // Then uncomment the corresponding model builder configurations
    }
}
