using crm_backend.Data;
using crm_backend.Modules.Marketing.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Marketing.Services;

public class CampaignService : ICampaignService
{
    private readonly CrmDbContext _context;

    public CampaignService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync(int? companyId = null)
    {
        var query = _context.Campaigns
            .Include(c => c.Company)
            .Include(c => c.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == companyId.Value);
        }

        var campaigns = await query
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Budget = c.Budget,
                ActualCost = c.ActualCost,
                Currency = c.Currency,
                ExpectedLeads = c.ExpectedLeads,
                ActualLeads = c.ActualLeads,
                ExpectedRevenue = c.ExpectedRevenue,
                ActualRevenue = c.ActualRevenue,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CompanyName = c.Company.Name,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return campaigns;
    }

    public async Task<CampaignDto?> GetCampaignByIdAsync(int id)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Company)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return null;

        return new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            Type = campaign.Type,
            Status = campaign.Status,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Budget = campaign.Budget,
            ActualCost = campaign.ActualCost,
            Currency = campaign.Currency,
            ExpectedLeads = campaign.ExpectedLeads,
            ActualLeads = campaign.ActualLeads,
            ExpectedRevenue = campaign.ExpectedRevenue,
            ActualRevenue = campaign.ActualRevenue,
            CreatedByUserId = campaign.CreatedByUserId,
            CreatedByUserName = campaign.CreatedByUser.Name,
            CompanyId = campaign.CompanyId,
            CompanyName = campaign.Company.Name,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }

    public async Task<CampaignDto> CreateCampaignAsync(CreateCampaignDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");

        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        var campaign = new Campaign
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Status = CampaignStatus.Planned,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            Currency = dto.Currency ?? "USD",
            ExpectedLeads = dto.ExpectedLeads,
            ExpectedRevenue = dto.ExpectedRevenue,
            CreatedByUserId = dto.CreatedByUserId ?? throw new InvalidOperationException("CreatedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(campaign).Reference(c => c.Company).LoadAsync();
        await _context.Entry(campaign).Reference(c => c.CreatedByUser).LoadAsync();

        return new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            Type = campaign.Type,
            Status = campaign.Status,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Budget = campaign.Budget,
            ActualCost = campaign.ActualCost,
            Currency = campaign.Currency,
            ExpectedLeads = campaign.ExpectedLeads,
            ActualLeads = campaign.ActualLeads,
            ExpectedRevenue = campaign.ExpectedRevenue,
            ActualRevenue = campaign.ActualRevenue,
            CreatedByUserId = campaign.CreatedByUserId,
            CreatedByUserName = campaign.CreatedByUser.Name,
            CompanyId = campaign.CompanyId,
            CompanyName = campaign.Company.Name,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }

    public async Task<CampaignDto?> UpdateCampaignAsync(int id, UpdateCampaignDto dto)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Company)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            campaign.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            campaign.Description = dto.Description;
        }

        if (dto.Type.HasValue)
        {
            campaign.Type = dto.Type.Value;
        }

        if (dto.Status.HasValue)
        {
            campaign.Status = dto.Status.Value;
        }

        if (dto.StartDate.HasValue)
        {
            campaign.StartDate = dto.StartDate;
        }

        if (dto.EndDate.HasValue)
        {
            campaign.EndDate = dto.EndDate;
        }

        if (dto.Budget.HasValue)
        {
            campaign.Budget = dto.Budget;
        }

        if (dto.ActualCost.HasValue)
        {
            campaign.ActualCost = dto.ActualCost;
        }

        if (dto.Currency != null)
        {
            campaign.Currency = dto.Currency;
        }

        if (dto.ExpectedLeads.HasValue)
        {
            campaign.ExpectedLeads = dto.ExpectedLeads;
        }

        if (dto.ExpectedRevenue.HasValue)
        {
            campaign.ExpectedRevenue = dto.ExpectedRevenue;
        }

        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            Type = campaign.Type,
            Status = campaign.Status,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Budget = campaign.Budget,
            ActualCost = campaign.ActualCost,
            Currency = campaign.Currency,
            ExpectedLeads = campaign.ExpectedLeads,
            ActualLeads = campaign.ActualLeads,
            ExpectedRevenue = campaign.ExpectedRevenue,
            ActualRevenue = campaign.ActualRevenue,
            CreatedByUserId = campaign.CreatedByUserId,
            CreatedByUserName = campaign.CreatedByUser.Name,
            CompanyId = campaign.CompanyId,
            CompanyName = campaign.Company.Name,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }

    public async Task<bool> DeleteCampaignAsync(int id)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null) return false;

        // Delete members first
        var members = await _context.CampaignMembers.Where(cm => cm.CampaignId == id).ToListAsync();
        _context.CampaignMembers.RemoveRange(members);

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CampaignMemberDto> AddCampaignMemberAsync(CreateCampaignMemberDto dto)
    {
        // Verify campaign exists
        var campaign = await _context.Campaigns.FindAsync(dto.CampaignId);
        if (campaign == null)
        {
            throw new InvalidOperationException($"Campaign with ID {dto.CampaignId} does not exist.");
        }

        // Verify at least one member type is provided
        if (!dto.LeadId.HasValue && !dto.CustomerId.HasValue && !dto.ContactId.HasValue)
        {
            throw new InvalidOperationException("At least one member (Lead, Customer, or Contact) must be provided.");
        }

        // Verify member exists and belongs to same company
        if (dto.LeadId.HasValue)
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == dto.LeadId.Value);
            if (lead == null || lead.CompanyId != campaign.CompanyId)
            {
                throw new InvalidOperationException("Lead does not exist or does not belong to the campaign's company.");
            }
        }

        if (dto.CustomerId.HasValue)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId.Value);
            if (customer == null || customer.CompanyId != campaign.CompanyId)
            {
                throw new InvalidOperationException("Customer does not exist or does not belong to the campaign's company.");
            }
        }

        if (dto.ContactId.HasValue)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == dto.ContactId.Value);
            if (contact == null)
            {
                throw new InvalidOperationException("Contact does not exist.");
            }
        }

        // Check if member already exists
        var existingMember = await _context.CampaignMembers
            .FirstOrDefaultAsync(cm => cm.CampaignId == dto.CampaignId
                && (cm.LeadId == dto.LeadId || cm.CustomerId == dto.CustomerId || cm.ContactId == dto.ContactId)
                && (dto.LeadId.HasValue && cm.LeadId == dto.LeadId
                    || dto.CustomerId.HasValue && cm.CustomerId == dto.CustomerId
                    || dto.ContactId.HasValue && cm.ContactId == dto.ContactId));

        if (existingMember != null)
        {
            throw new InvalidOperationException("Member is already part of this campaign.");
        }

        var member = new CampaignMember
        {
            CampaignId = dto.CampaignId,
            LeadId = dto.LeadId,
            CustomerId = dto.CustomerId,
            ContactId = dto.ContactId,
            Status = CampaignMemberStatus.Sent
        };

        _context.CampaignMembers.Add(member);
        await _context.SaveChangesAsync();

        // Update campaign metrics
        await UpdateCampaignMetricsAsync(dto.CampaignId);

        // Load related entities for response
        await _context.Entry(member).Reference(m => m.Campaign).LoadAsync();
        if (member.LeadId.HasValue)
        {
            await _context.Entry(member).Reference(m => m.Lead).LoadAsync();
        }
        if (member.CustomerId.HasValue)
        {
            await _context.Entry(member).Reference(m => m.Customer).LoadAsync();
        }
        if (member.ContactId.HasValue)
        {
            await _context.Entry(member).Reference(m => m.Contact).LoadAsync();
        }

        return new CampaignMemberDto
        {
            Id = member.Id,
            CampaignId = member.CampaignId,
            CampaignName = member.Campaign.Name,
            LeadId = member.LeadId,
            LeadName = member.Lead != null ? $"{member.Lead.FirstName} {member.Lead.LastName}" : null,
            CustomerId = member.CustomerId,
            CustomerName = member.Customer != null ? member.Customer.Name : null,
            ContactId = member.ContactId,
            ContactName = member.Contact != null ? member.Contact.Name : null,
            Status = member.Status,
            RespondedAt = member.RespondedAt,
            ConvertedAt = member.ConvertedAt
        };
    }

    public async Task<bool> RemoveCampaignMemberAsync(int memberId)
    {
        var member = await _context.CampaignMembers.FindAsync(memberId);
        if (member == null) return false;

        var campaignId = member.CampaignId;

        _context.CampaignMembers.Remove(member);
        await _context.SaveChangesAsync();

        // Update campaign metrics
        await UpdateCampaignMetricsAsync(campaignId);

        return true;
    }

    public async Task<IEnumerable<CampaignMemberDto>> GetCampaignMembersAsync(int campaignId)
    {
        var members = await _context.CampaignMembers
            .Include(cm => cm.Campaign)
            .Include(cm => cm.Lead)
            .Include(cm => cm.Customer)
            .Include(cm => cm.Contact)
            .Where(cm => cm.CampaignId == campaignId)
            .Select(cm => new CampaignMemberDto
            {
                Id = cm.Id,
                CampaignId = cm.CampaignId,
                CampaignName = cm.Campaign.Name,
                LeadId = cm.LeadId,
                LeadName = cm.Lead != null ? $"{cm.Lead.FirstName} {cm.Lead.LastName}" : null,
                CustomerId = cm.CustomerId,
                CustomerName = cm.Customer != null ? cm.Customer.Name : null,
                ContactId = cm.ContactId,
                ContactName = cm.Contact != null ? cm.Contact.Name : null,
                Status = cm.Status,
                RespondedAt = cm.RespondedAt,
                ConvertedAt = cm.ConvertedAt
            })
            .ToListAsync();

        return members;
    }

    public async Task UpdateCampaignMetricsAsync(int campaignId)
    {
        var campaign = await _context.Campaigns.FindAsync(campaignId);
        if (campaign == null) return;

        // Count actual leads (members that are leads)
        campaign.ActualLeads = await _context.CampaignMembers
            .CountAsync(cm => cm.CampaignId == campaignId && cm.LeadId.HasValue);

        // Calculate actual revenue from converted leads
        var convertedMembers = await _context.CampaignMembers
            .Include(cm => cm.Lead)
            .Where(cm => cm.CampaignId == campaignId
                && cm.Status == CampaignMemberStatus.Converted
                && cm.LeadId.HasValue
                && cm.Lead != null
                && cm.Lead.EstimatedValue.HasValue)
            .ToListAsync();

        campaign.ActualRevenue = convertedMembers
            .Where(cm => cm.Lead != null && cm.Lead.EstimatedValue.HasValue)
            .Sum(cm => cm.Lead!.EstimatedValue!.Value);

        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

