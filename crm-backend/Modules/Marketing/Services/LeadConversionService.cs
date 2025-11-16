using crm_backend.Data;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Modules.Customer.DTOs;
using crm_backend.Modules.Customer.Services;
using crm_backend.Modules.Marketing.DTOs;
using crm_backend.Modules.Opportunity.DTOs;
using crm_backend.Modules.Opportunity.Services;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Marketing.Services;

public class LeadConversionService : ILeadConversionService
{
    private readonly CrmDbContext _context;
    private readonly ICustomerService _customerService;
    private readonly IOpportunityService _opportunityService;
    private readonly IActivityTimelineService _activityTimelineService;

    public LeadConversionService(
        CrmDbContext context,
        ICustomerService customerService,
        IOpportunityService opportunityService,
        IActivityTimelineService activityTimelineService)
    {
        _context = context;
        _customerService = customerService;
        _opportunityService = opportunityService;
        _activityTimelineService = activityTimelineService;
    }

    public async Task<LeadConversionResultDto> ConvertLeadToCustomerAndOpportunityAsync(int leadId, ConvertLeadDto dto, int userId, int companyId)
    {
        var lead = await _context.Leads
            .Include(l => l.Source)
            .FirstOrDefaultAsync(l => l.Id == leadId && l.CompanyId == companyId);

        if (lead == null)
            throw new InvalidOperationException("Lead not found");

        if (lead.Status == LeadStatus.Converted)
            throw new InvalidOperationException("Lead has already been converted");

        CustomerDto? customer = null;
        OpportunityDto? opportunity = null;

        // Create Customer if needed
        if (dto.CreateCustomer)
        {
            var createCustomerDto = new CreateCustomerDto
            {
                Name = $"{lead.FirstName} {lead.LastName}".Trim(),
                Email = lead.Email,
                Phone = lead.Phone ?? lead.Mobile,
                Address = lead.Address,
                City = lead.City,
                Country = lead.Country,
                CompanyId = companyId
            };

            customer = await _customerService.CreateCustomerAsync(createCustomerDto);

            // Link customer to lead
            var customerEntity = await _context.Customers.FindAsync(customer.Id);
            if (customerEntity != null)
            {
                customerEntity.ConvertedFromLeadId = leadId;
                await _context.SaveChangesAsync();
            }
        }
        else if (dto.CustomerId.HasValue)
        {
            // Link to existing customer
            customer = await _customerService.GetCustomerByIdAsync(dto.CustomerId.Value);
            if (customer == null || customer.CompanyId != companyId)
                throw new InvalidOperationException("Customer not found or does not belong to company");
        }

        // Create Opportunity if needed
        if (dto.CreateOpportunity && customer != null)
        {
            // Get the first pipeline stage for the company (default stage)
            var defaultStage = await _context.PipelineStages
                .Where(ps => ps.CompanyId == companyId)
                .OrderBy(ps => ps.Order)
                .FirstOrDefaultAsync();

            if (defaultStage == null)
                throw new InvalidOperationException("No pipeline stages found for company");

            var createOpportunityDto = new CreateOpportunityDto
            {
                CustomerId = customer.Id,
                Name = $"Opportunity for {lead.FirstName} {lead.LastName}",
                Description = $"Converted from lead: {lead.CompanyName}",
                Amount = lead.EstimatedValue ?? 0,
                Currency = lead.Currency ?? "USD",
                Probability = 50, // Default probability
                PipelineStageId = defaultStage.Id,
                SourceId = lead.SourceId,
                AssignedUserId = lead.AssignedUserId ?? userId,
                CompanyId = companyId
            };

            opportunity = await _opportunityService.CreateOpportunityAsync(createOpportunityDto);

            // Link opportunity to lead
            var opportunityEntity = await _context.Opportunities.FindAsync(opportunity.Id);
            if (opportunityEntity != null)
            {
                opportunityEntity.ConvertedFromLeadId = leadId;
                await _context.SaveChangesAsync();
            }
        }
        else if (dto.OpportunityId.HasValue)
        {
            // Link to existing opportunity
            opportunity = await _opportunityService.GetOpportunityByIdAsync(dto.OpportunityId.Value);
            if (opportunity == null || opportunity.CompanyId != companyId)
                throw new InvalidOperationException("Opportunity not found or does not belong to company");
        }

        // Update lead status
        lead.Status = LeadStatus.Converted;
        lead.ConvertedAt = DateTime.UtcNow;
        lead.ConvertedByUserId = userId;
        if (customer != null)
            lead.ConvertedToCustomerId = customer.Id;
        if (opportunity != null)
            lead.ConvertedToOpportunityId = opportunity.Id;

        await _context.SaveChangesAsync();

        // Log activity
        if (customer != null)
        {
            await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
            {
                EntityType = "Customer",
                EntityId = customer.Id,
                Type = "Created",
                Description = $"Customer created from Lead: {lead.FirstName} {lead.LastName}"
            }, companyId, userId);
        }

        if (opportunity != null)
        {
            await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
            {
                EntityType = "Opportunity",
                EntityId = opportunity.Id,
                Type = "Created",
                Description = $"Opportunity created from Lead: {lead.FirstName} {lead.LastName}"
            }, companyId, userId);
        }

        // Map lead to DTO
        var leadDto = new LeadDto
        {
            Id = lead.Id,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Email = lead.Email,
            Phone = lead.Phone,
            Status = lead.Status,
            ConvertedToCustomerId = lead.ConvertedToCustomerId,
            ConvertedToOpportunityId = lead.ConvertedToOpportunityId,
            ConvertedAt = lead.ConvertedAt,
            CompanyId = lead.CompanyId
        };

        return new LeadConversionResultDto
        {
            Lead = leadDto,
            Customer = customer,
            Opportunity = opportunity
        };
    }

    public async Task<int> CalculateLeadScoreAsync(int leadId, int companyId)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.CompanyId == companyId);

        if (lead == null)
            throw new InvalidOperationException("Lead not found");

        int score = 0;

        // Email present: +10
        if (!string.IsNullOrEmpty(lead.Email))
            score += 10;

        // Phone present: +10
        if (!string.IsNullOrEmpty(lead.Phone) || !string.IsNullOrEmpty(lead.Mobile))
            score += 10;

        // Company name present: +10
        if (!string.IsNullOrEmpty(lead.CompanyName))
            score += 10;

        // Industry present: +5
        if (!string.IsNullOrEmpty(lead.Industry))
            score += 5;

        // Estimated value present: +15
        if (lead.EstimatedValue.HasValue && lead.EstimatedValue.Value > 0)
            score += 15;

        // Rating: Hot=30, Warm=20, Cold=10
        score += lead.Rating switch
        {
            LeadRating.Hot => 30,
            LeadRating.Warm => 20,
            LeadRating.Cold => 10,
            _ => 0
        };

        // Status: Qualified=20, Contacted=10
        score += lead.Status switch
        {
            LeadStatus.Qualified => 20,
            LeadStatus.Contacted => 10,
            _ => 0
        };

        // Cap at 100
        score = Math.Min(score, 100);

        // Update lead score
        lead.LeadScore = score;
        await _context.SaveChangesAsync();

        return score;
    }
}

