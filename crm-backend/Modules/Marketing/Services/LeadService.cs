using crm_backend.Data;
using crm_backend.Modules.Marketing.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Marketing.Services;

public class LeadService : ILeadService
{
    private readonly CrmDbContext _context;

    public LeadService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeadDto>> GetAllLeadsAsync(int? companyId = null)
    {
        var query = _context.Leads
            .Include(l => l.Company)
            .Include(l => l.Source)
            .Include(l => l.AssignedUser)
            .Include(l => l.ConvertedToCustomer)
            .Include(l => l.ConvertedToOpportunity)
            .Include(l => l.ConvertedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == companyId.Value);
        }

        var leads = await query
            .Select(l => new LeadDto
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                CompanyName = l.CompanyName,
                Title = l.Title,
                Email = l.Email,
                Phone = l.Phone,
                Mobile = l.Mobile,
                Website = l.Website,
                Address = l.Address,
                City = l.City,
                State = l.State,
                Country = l.Country,
                PostalCode = l.PostalCode,
                Industry = l.Industry,
                EstimatedValue = l.EstimatedValue,
                Currency = l.Currency,
                Status = l.Status,
                Rating = l.Rating,
                SourceId = l.SourceId,
                SourceName = l.Source != null ? l.Source.Name : null,
                AssignedUserId = l.AssignedUserId,
                AssignedUserName = l.AssignedUser != null ? l.AssignedUser.Name : null,
                ConvertedToCustomerId = l.ConvertedToCustomerId,
                ConvertedToCustomerName = l.ConvertedToCustomer != null ? l.ConvertedToCustomer.Name : null,
                ConvertedToOpportunityId = l.ConvertedToOpportunityId,
                ConvertedToOpportunityName = l.ConvertedToOpportunity != null ? l.ConvertedToOpportunity.Name : null,
                ConvertedAt = l.ConvertedAt,
                ConvertedByUserId = l.ConvertedByUserId,
                ConvertedByUserName = l.ConvertedByUser != null ? l.ConvertedByUser.Name : null,
                CompanyId = l.CompanyId,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        return leads;
    }

    public async Task<LeadDto?> GetLeadByIdAsync(int id)
    {
        var lead = await _context.Leads
            .Include(l => l.Company)
            .Include(l => l.Source)
            .Include(l => l.AssignedUser)
            .Include(l => l.ConvertedToCustomer)
            .Include(l => l.ConvertedToOpportunity)
            .Include(l => l.ConvertedByUser)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lead == null) return null;

        return new LeadDto
        {
            Id = lead.Id,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            CompanyName = lead.CompanyName,
            Title = lead.Title,
            Email = lead.Email,
            Phone = lead.Phone,
            Mobile = lead.Mobile,
            Website = lead.Website,
            Address = lead.Address,
            City = lead.City,
            State = lead.State,
            Country = lead.Country,
            PostalCode = lead.PostalCode,
            Industry = lead.Industry,
            EstimatedValue = lead.EstimatedValue,
            Currency = lead.Currency,
            Status = lead.Status,
            Rating = lead.Rating,
            SourceId = lead.SourceId,
            SourceName = lead.Source != null ? lead.Source.Name : null,
            AssignedUserId = lead.AssignedUserId,
            AssignedUserName = lead.AssignedUser != null ? lead.AssignedUser.Name : null,
            ConvertedToCustomerId = lead.ConvertedToCustomerId,
            ConvertedToCustomerName = lead.ConvertedToCustomer != null ? lead.ConvertedToCustomer.Name : null,
            ConvertedToOpportunityId = lead.ConvertedToOpportunityId,
            ConvertedToOpportunityName = lead.ConvertedToOpportunity != null ? lead.ConvertedToOpportunity.Name : null,
            ConvertedAt = lead.ConvertedAt,
            ConvertedByUserId = lead.ConvertedByUserId,
            ConvertedByUserName = lead.ConvertedByUser != null ? lead.ConvertedByUser.Name : null,
            CompanyId = lead.CompanyId,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }

    public async Task<LeadDto> CreateLeadAsync(CreateLeadDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        
        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify source exists and belongs to company (if provided)
        if (dto.SourceId.HasValue)
        {
            var source = await _context.LeadSources.FirstOrDefaultAsync(s => s.Id == dto.SourceId.Value);
            if (source == null)
            {
                throw new InvalidOperationException($"Lead source with ID {dto.SourceId.Value} does not exist.");
            }
            if (source.CompanyId != companyId)
            {
                throw new InvalidOperationException("Lead source does not belong to the specified company.");
            }
        }

        // Verify assigned user belongs to company (if provided)
        if (dto.AssignedUserId.HasValue)
        {
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.AssignedUserId.Value && uc.CompanyId == companyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("Assigned user does not belong to the specified company.");
            }
        }

        var lead = new Lead
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CompanyName = dto.CompanyName,
            Title = dto.Title,
            Email = dto.Email,
            Phone = dto.Phone,
            Mobile = dto.Mobile,
            Website = dto.Website,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            Industry = dto.Industry,
            EstimatedValue = dto.EstimatedValue,
            Currency = dto.Currency ?? "USD",
            Status = LeadStatus.New,
            Rating = dto.Rating,
            SourceId = dto.SourceId,
            AssignedUserId = dto.AssignedUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(lead).Reference(l => l.Company).LoadAsync();
        if (lead.SourceId.HasValue)
        {
            await _context.Entry(lead).Reference(l => l.Source).LoadAsync();
        }
        if (lead.AssignedUserId.HasValue)
        {
            await _context.Entry(lead).Reference(l => l.AssignedUser).LoadAsync();
        }

        return new LeadDto
        {
            Id = lead.Id,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            CompanyName = lead.CompanyName,
            Title = lead.Title,
            Email = lead.Email,
            Phone = lead.Phone,
            Mobile = lead.Mobile,
            Website = lead.Website,
            Address = lead.Address,
            City = lead.City,
            State = lead.State,
            Country = lead.Country,
            PostalCode = lead.PostalCode,
            Industry = lead.Industry,
            EstimatedValue = lead.EstimatedValue,
            Currency = lead.Currency,
            Status = lead.Status,
            Rating = lead.Rating,
            SourceId = lead.SourceId,
            SourceName = lead.Source != null ? lead.Source.Name : null,
            AssignedUserId = lead.AssignedUserId,
            AssignedUserName = lead.AssignedUser != null ? lead.AssignedUser.Name : null,
            ConvertedToCustomerId = lead.ConvertedToCustomerId,
            ConvertedToCustomerName = null,
            ConvertedToOpportunityId = lead.ConvertedToOpportunityId,
            ConvertedToOpportunityName = null,
            ConvertedAt = lead.ConvertedAt,
            ConvertedByUserId = lead.ConvertedByUserId,
            ConvertedByUserName = null,
            CompanyId = lead.CompanyId,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }

    public async Task<LeadDto?> UpdateLeadAsync(int id, UpdateLeadDto dto)
    {
        var lead = await _context.Leads
            .Include(l => l.Company)
            .Include(l => l.Source)
            .Include(l => l.AssignedUser)
            .Include(l => l.ConvertedToCustomer)
            .Include(l => l.ConvertedToOpportunity)
            .Include(l => l.ConvertedByUser)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lead == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
        {
            lead.FirstName = dto.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(dto.LastName))
        {
            lead.LastName = dto.LastName;
        }

        if (dto.CompanyName != null)
        {
            lead.CompanyName = dto.CompanyName;
        }

        if (dto.Title != null)
        {
            lead.Title = dto.Title;
        }

        if (dto.Email != null)
        {
            lead.Email = dto.Email;
        }

        if (dto.Phone != null)
        {
            lead.Phone = dto.Phone;
        }

        if (dto.Mobile != null)
        {
            lead.Mobile = dto.Mobile;
        }

        if (dto.Website != null)
        {
            lead.Website = dto.Website;
        }

        if (dto.Address != null)
        {
            lead.Address = dto.Address;
        }

        if (dto.City != null)
        {
            lead.City = dto.City;
        }

        if (dto.State != null)
        {
            lead.State = dto.State;
        }

        if (dto.Country != null)
        {
            lead.Country = dto.Country;
        }

        if (dto.PostalCode != null)
        {
            lead.PostalCode = dto.PostalCode;
        }

        if (dto.Industry != null)
        {
            lead.Industry = dto.Industry;
        }

        if (dto.EstimatedValue.HasValue)
        {
            lead.EstimatedValue = dto.EstimatedValue;
        }

        if (dto.Currency != null)
        {
            lead.Currency = dto.Currency;
        }

        if (dto.Status.HasValue)
        {
            lead.Status = dto.Status.Value;
        }

        if (dto.Rating.HasValue)
        {
            lead.Rating = dto.Rating.Value;
        }

        if (dto.SourceId.HasValue)
        {
            // Verify source belongs to company
            var source = await _context.LeadSources.FirstOrDefaultAsync(s => s.Id == dto.SourceId.Value);
            if (source == null || source.CompanyId != lead.CompanyId)
            {
                throw new InvalidOperationException("Lead source does not belong to the lead's company.");
            }
            lead.SourceId = dto.SourceId;
        }

        if (dto.AssignedUserId.HasValue)
        {
            // Verify user belongs to company
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.AssignedUserId.Value && uc.CompanyId == lead.CompanyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("Assigned user does not belong to the lead's company.");
            }
            lead.AssignedUserId = dto.AssignedUserId;
        }

        lead.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload related entities if changed
        if (dto.SourceId.HasValue)
        {
            await _context.Entry(lead).Reference(l => l.Source).LoadAsync();
        }
        if (dto.AssignedUserId.HasValue)
        {
            await _context.Entry(lead).Reference(l => l.AssignedUser).LoadAsync();
        }

        return new LeadDto
        {
            Id = lead.Id,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            CompanyName = lead.CompanyName,
            Title = lead.Title,
            Email = lead.Email,
            Phone = lead.Phone,
            Mobile = lead.Mobile,
            Website = lead.Website,
            Address = lead.Address,
            City = lead.City,
            State = lead.State,
            Country = lead.Country,
            PostalCode = lead.PostalCode,
            Industry = lead.Industry,
            EstimatedValue = lead.EstimatedValue,
            Currency = lead.Currency,
            Status = lead.Status,
            Rating = lead.Rating,
            SourceId = lead.SourceId,
            SourceName = lead.Source != null ? lead.Source.Name : null,
            AssignedUserId = lead.AssignedUserId,
            AssignedUserName = lead.AssignedUser != null ? lead.AssignedUser.Name : null,
            ConvertedToCustomerId = lead.ConvertedToCustomerId,
            ConvertedToCustomerName = lead.ConvertedToCustomer != null ? lead.ConvertedToCustomer.Name : null,
            ConvertedToOpportunityId = lead.ConvertedToOpportunityId,
            ConvertedToOpportunityName = lead.ConvertedToOpportunity != null ? lead.ConvertedToOpportunity.Name : null,
            ConvertedAt = lead.ConvertedAt,
            ConvertedByUserId = lead.ConvertedByUserId,
            ConvertedByUserName = lead.ConvertedByUser != null ? lead.ConvertedByUser.Name : null,
            CompanyId = lead.CompanyId,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }

    public async Task<bool> DeleteLeadAsync(int id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return false;

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LeadDto> ConvertLeadToCustomerAsync(int leadId, ConvertLeadDto? dto = null)
    {
        var lead = await _context.Leads
            .Include(l => l.Company)
            .Include(l => l.Source)
            .Include(l => l.AssignedUser)
            .FirstOrDefaultAsync(l => l.Id == leadId);

        if (lead == null)
        {
            throw new InvalidOperationException($"Lead with ID {leadId} does not exist.");
        }

        if (lead.Status == LeadStatus.Converted)
        {
            throw new InvalidOperationException("Lead has already been converted.");
        }

        // Create customer from lead
        var customer = new Customer.Customer
        {
            Name = dto?.CustomerName ?? lead.CompanyName ?? $"{lead.FirstName} {lead.LastName}",
            Email = lead.Email,
            Phone = lead.Phone,
            Address = lead.Address,
            City = lead.City,
            Country = lead.Country,
            CompanyId = lead.CompanyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Update lead
        lead.Status = LeadStatus.Converted;
        lead.ConvertedToCustomerId = customer.Id;
        lead.ConvertedAt = DateTime.UtcNow;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(lead).Reference(l => l.ConvertedToCustomer).LoadAsync();

        return await GetLeadByIdAsync(leadId) ?? throw new InvalidOperationException("Failed to retrieve converted lead");
    }

    public async Task<LeadDto> ConvertLeadToOpportunityAsync(int leadId, ConvertLeadDto? dto = null)
    {
        var lead = await _context.Leads
            .Include(l => l.Company)
            .Include(l => l.Source)
            .Include(l => l.AssignedUser)
            .FirstOrDefaultAsync(l => l.Id == leadId);

        if (lead == null)
        {
            throw new InvalidOperationException($"Lead with ID {leadId} does not exist.");
        }

        if (lead.Status == LeadStatus.Converted)
        {
            throw new InvalidOperationException("Lead has already been converted.");
        }

        // Ensure customer exists first
        if (!lead.ConvertedToCustomerId.HasValue)
        {
            // Convert to customer first
            await ConvertLeadToCustomerAsync(leadId, dto);
            await _context.Entry(lead).ReloadAsync();
        }

        // Verify customer was created
        if (!lead.ConvertedToCustomerId.HasValue)
        {
            throw new InvalidOperationException("Failed to convert lead to customer before creating opportunity.");
        }

        // Create opportunity from lead
        var opportunity = new Opportunity.Opportunity
        {
            Name = $"{lead.FirstName} {lead.LastName} - {lead.CompanyName ?? "Opportunity"}",
            Description = $"Converted from lead: {lead.FirstName} {lead.LastName}",
            Amount = lead.EstimatedValue ?? 0,
            Currency = lead.Currency ?? "USD",
            Probability = 10, // Default probability for new opportunity
            CustomerId = lead.ConvertedToCustomerId.Value,
            AssignedUserId = lead.AssignedUserId,
            SourceId = lead.SourceId,
            CompanyId = lead.CompanyId,
            Status = Opportunity.OpportunityStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Update lead
        lead.Status = LeadStatus.Converted;
        lead.ConvertedToOpportunityId = opportunity.Id;
        if (!lead.ConvertedToCustomerId.HasValue)
        {
            // This shouldn't happen, but just in case
            lead.ConvertedToCustomerId = opportunity.CustomerId;
        }
        lead.ConvertedAt = DateTime.UtcNow;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(lead).Reference(l => l.ConvertedToOpportunity).LoadAsync();

        return await GetLeadByIdAsync(leadId) ?? throw new InvalidOperationException("Failed to retrieve converted lead");
    }
}

