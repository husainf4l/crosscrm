using crm_backend.Data;
using crm_backend.Modules.Contract.DTOs;
using crm_backend.Modules.Opportunity;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Contract.Services;

public class ContractService : IContractService
{
    private readonly CrmDbContext _context;

    public ContractService(CrmDbContext context)
    {
        _context = context;
    }

    private async Task<string> GenerateContractNumberAsync(int companyId)
    {
        var year = DateTime.UtcNow.Year;
        var lastContract = await _context.Contracts
            .Where(c => c.CompanyId == companyId && c.ContractNumber.StartsWith($"CNT-{year}-"))
            .OrderByDescending(c => c.ContractNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastContract != null)
        {
            var parts = lastContract.ContractNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"CNT-{year}-{nextNumber:D4}";
    }

    public async Task<IEnumerable<ContractDto>> GetAllContractsAsync(int? companyId = null)
    {
        var query = _context.Contracts
            .Include(c => c.Company)
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .Include(c => c.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == companyId.Value);
        }

        var contracts = await query
            .Select(c => new ContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                Name = c.Name,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                RenewalDate = c.RenewalDate,
                AutoRenew = c.AutoRenew,
                TotalValue = c.TotalValue,
                Currency = c.Currency,
                Status = c.Status,
                SignedAt = c.SignedAt,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.Name,
                OpportunityId = c.OpportunityId,
                OpportunityName = c.Opportunity != null ? c.Opportunity.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return contracts;
    }

    public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Company)
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null) return null;

        return new ContractDto
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            Name = contract.Name,
            Description = contract.Description,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            RenewalDate = contract.RenewalDate,
            AutoRenew = contract.AutoRenew,
            TotalValue = contract.TotalValue,
            Currency = contract.Currency,
            Status = contract.Status,
            SignedAt = contract.SignedAt,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer.Name,
            OpportunityId = contract.OpportunityId,
            OpportunityName = contract.Opportunity != null ? contract.Opportunity.Name : null,
            CreatedByUserId = contract.CreatedByUserId,
            CreatedByUserName = contract.CreatedByUser.Name,
            CompanyId = contract.CompanyId,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }

    public async Task<ContractDto> CreateContractAsync(CreateContractDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        
        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify customer exists and belongs to company
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {dto.CustomerId} does not exist.");
        }
        if (customer.CompanyId != companyId)
        {
            throw new InvalidOperationException("Customer does not belong to the specified company.");
        }

        // Verify opportunity exists and belongs to company (if provided)
        if (dto.OpportunityId.HasValue)
        {
            var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == dto.OpportunityId.Value);
            if (opportunity == null)
            {
                throw new InvalidOperationException($"Opportunity with ID {dto.OpportunityId.Value} does not exist.");
            }
            if (opportunity.CompanyId != companyId)
            {
                throw new InvalidOperationException("Opportunity does not belong to the specified company.");
            }
            // Business rule: Contract can only be created from Won Opportunity
            if (opportunity.Status != OpportunityStatus.Won)
            {
                throw new InvalidOperationException($"Cannot create contract for opportunity with status {opportunity.Status}. Opportunity must be Won.");
            }
        }

        // Calculate total value from line items if provided, otherwise use provided value
        decimal totalValue = dto.TotalValue ?? 0;
        if (dto.LineItems != null && dto.LineItems.Count > 0)
        {
            foreach (var itemDto in dto.LineItems)
            {
                // Verify product exists and belongs to company
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} does not exist.");
                }
                if (product.CompanyId != companyId)
                {
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} does not belong to the specified company.");
                }

                totalValue += itemDto.Quantity * itemDto.UnitPrice;
            }
        }

        var contract = new Contract
        {
            ContractNumber = await GenerateContractNumberAsync(companyId),
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            RenewalDate = dto.RenewalDate,
            AutoRenew = dto.AutoRenew,
            TotalValue = totalValue,
            Currency = dto.Currency ?? "USD",
            Status = ContractStatus.Draft,
            CustomerId = dto.CustomerId,
            OpportunityId = dto.OpportunityId,
            CreatedByUserId = dto.CreatedByUserId ?? throw new InvalidOperationException("CreatedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        // Add line items
        if (dto.LineItems != null && dto.LineItems.Count > 0)
        {
            int order = 1;
            foreach (var itemDto in dto.LineItems)
            {
                var lineItem = new ContractLineItem
                {
                    ContractId = contract.Id,
                    ProductId = itemDto.ProductId,
                    Description = itemDto.Description,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.Quantity * itemDto.UnitPrice,
                    Order = itemDto.Order > 0 ? itemDto.Order : order++
                };
                _context.ContractLineItems.Add(lineItem);
            }
            await _context.SaveChangesAsync();
        }

        // Load related entities for response
        await _context.Entry(contract).Reference(c => c.Company).LoadAsync();
        await _context.Entry(contract).Reference(c => c.Customer).LoadAsync();
        await _context.Entry(contract).Reference(c => c.CreatedByUser).LoadAsync();
        if (contract.OpportunityId.HasValue)
        {
            await _context.Entry(contract).Reference(c => c.Opportunity).LoadAsync();
        }

        return new ContractDto
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            Name = contract.Name,
            Description = contract.Description,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            RenewalDate = contract.RenewalDate,
            AutoRenew = contract.AutoRenew,
            TotalValue = contract.TotalValue,
            Currency = contract.Currency,
            Status = contract.Status,
            SignedAt = contract.SignedAt,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer.Name,
            OpportunityId = contract.OpportunityId,
            OpportunityName = contract.Opportunity != null ? contract.Opportunity.Name : null,
            CreatedByUserId = contract.CreatedByUserId,
            CreatedByUserName = contract.CreatedByUser.Name,
            CompanyId = contract.CompanyId,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }

    public async Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto dto)
    {
        var contract = await _context.Contracts
            .Include(c => c.Company)
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            contract.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            contract.Description = dto.Description;
        }

        if (dto.StartDate.HasValue)
        {
            contract.StartDate = dto.StartDate.Value;
        }

        if (dto.EndDate.HasValue)
        {
            contract.EndDate = dto.EndDate.Value;
        }

        if (dto.RenewalDate.HasValue)
        {
            contract.RenewalDate = dto.RenewalDate;
        }

        if (dto.AutoRenew.HasValue)
        {
            contract.AutoRenew = dto.AutoRenew.Value;
        }

        if (dto.TotalValue.HasValue)
        {
            contract.TotalValue = dto.TotalValue.Value;
        }

        if (dto.Currency != null)
        {
            contract.Currency = dto.Currency;
        }

        if (dto.Status.HasValue)
        {
            contract.Status = dto.Status.Value;
            
            if (dto.Status.Value == ContractStatus.Signed && contract.SignedAt == null)
            {
                contract.SignedAt = DateTime.UtcNow;
            }
            else if (dto.Status.Value == ContractStatus.Active && contract.Status != ContractStatus.Active)
            {
                // Auto-update status based on dates
                if (contract.StartDate <= DateTime.UtcNow && contract.EndDate >= DateTime.UtcNow)
                {
                    contract.Status = ContractStatus.Active;
                }
            }
        }

        // Auto-update status based on dates
        if (contract.Status == ContractStatus.Active && contract.EndDate < DateTime.UtcNow.Date)
        {
            contract.Status = ContractStatus.Expired;
        }

        contract.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ContractDto
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            Name = contract.Name,
            Description = contract.Description,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            RenewalDate = contract.RenewalDate,
            AutoRenew = contract.AutoRenew,
            TotalValue = contract.TotalValue,
            Currency = contract.Currency,
            Status = contract.Status,
            SignedAt = contract.SignedAt,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer.Name,
            OpportunityId = contract.OpportunityId,
            OpportunityName = contract.Opportunity != null ? contract.Opportunity.Name : null,
            CreatedByUserId = contract.CreatedByUserId,
            CreatedByUserName = contract.CreatedByUser.Name,
            CompanyId = contract.CompanyId,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }

    public async Task<bool> DeleteContractAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return false;

        // Delete line items first
        var lineItems = await _context.ContractLineItems.Where(li => li.ContractId == id).ToListAsync();
        _context.ContractLineItems.RemoveRange(lineItems);

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ContractDto> RenewContractAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Company)
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LineItems)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            throw new InvalidOperationException($"Contract with ID {id} does not exist.");
        }

        if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Expired)
        {
            throw new InvalidOperationException("Only active or expired contracts can be renewed.");
        }

        // Mark old contract as renewed
        contract.Status = ContractStatus.Renewed;
        contract.UpdatedAt = DateTime.UtcNow;

        // Calculate new dates
        var duration = contract.EndDate - contract.StartDate;
        var newStartDate = contract.EndDate.AddDays(1);
        var newEndDate = newStartDate.Add(duration);
        var newRenewalDate = contract.RenewalDate.HasValue 
            ? contract.RenewalDate.Value.Add(duration) 
            : (DateTime?)null;

        // Create new contract
        var newContract = new Contract
        {
            ContractNumber = await GenerateContractNumberAsync(contract.CompanyId),
            Name = contract.Name,
            Description = contract.Description,
            StartDate = newStartDate,
            EndDate = newEndDate,
            RenewalDate = newRenewalDate,
            AutoRenew = contract.AutoRenew,
            TotalValue = contract.TotalValue,
            Currency = contract.Currency,
            Status = ContractStatus.Draft,
            CustomerId = contract.CustomerId,
            OpportunityId = contract.OpportunityId,
            CreatedByUserId = contract.CreatedByUserId,
            CompanyId = contract.CompanyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(newContract);
        await _context.SaveChangesAsync();

        // Copy line items
        foreach (var oldItem in contract.LineItems)
        {
            var newItem = new ContractLineItem
            {
                ContractId = newContract.Id,
                ProductId = oldItem.ProductId,
                Description = oldItem.Description,
                Quantity = oldItem.Quantity,
                UnitPrice = oldItem.UnitPrice,
                TotalPrice = oldItem.TotalPrice,
                Order = oldItem.Order
            };
            _context.ContractLineItems.Add(newItem);
        }

        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(newContract).Reference(c => c.Company).LoadAsync();
        await _context.Entry(newContract).Reference(c => c.Customer).LoadAsync();
        await _context.Entry(newContract).Reference(c => c.CreatedByUser).LoadAsync();
        if (newContract.OpportunityId.HasValue)
        {
            await _context.Entry(newContract).Reference(c => c.Opportunity).LoadAsync();
        }

        return new ContractDto
        {
            Id = newContract.Id,
            ContractNumber = newContract.ContractNumber,
            Name = newContract.Name,
            Description = newContract.Description,
            StartDate = newContract.StartDate,
            EndDate = newContract.EndDate,
            RenewalDate = newContract.RenewalDate,
            AutoRenew = newContract.AutoRenew,
            TotalValue = newContract.TotalValue,
            Currency = newContract.Currency,
            Status = newContract.Status,
            SignedAt = newContract.SignedAt,
            CustomerId = newContract.CustomerId,
            CustomerName = newContract.Customer.Name,
            OpportunityId = newContract.OpportunityId,
            OpportunityName = newContract.Opportunity != null ? newContract.Opportunity.Name : null,
            CreatedByUserId = newContract.CreatedByUserId,
            CreatedByUserName = newContract.CreatedByUser.Name,
            CompanyId = newContract.CompanyId,
            CreatedAt = newContract.CreatedAt,
            UpdatedAt = newContract.UpdatedAt
        };
    }

    public async Task<IEnumerable<ContractDto>> GetExpiringContractsAsync(int companyId, int daysAhead = 30)
    {
        var endDate = DateTime.UtcNow.AddDays(daysAhead);

        var contracts = await _context.Contracts
            .Include(c => c.Company)
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .Include(c => c.CreatedByUser)
            .Where(c => c.CompanyId == companyId 
                && c.EndDate >= DateTime.UtcNow.Date 
                && c.EndDate <= endDate
                && (c.Status == ContractStatus.Active || c.Status == ContractStatus.Signed))
            .OrderBy(c => c.EndDate)
            .Select(c => new ContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                Name = c.Name,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                RenewalDate = c.RenewalDate,
                AutoRenew = c.AutoRenew,
                TotalValue = c.TotalValue,
                Currency = c.Currency,
                Status = c.Status,
                SignedAt = c.SignedAt,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.Name,
                OpportunityId = c.OpportunityId,
                OpportunityName = c.Opportunity != null ? c.Opportunity.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return contracts;
    }

    public async Task<IEnumerable<ContractDto>> GetExpiredContractsAsync(int companyId)
    {
        var contracts = await _context.Contracts
            .Include(c => c.Company)
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .Include(c => c.CreatedByUser)
            .Where(c => c.CompanyId == companyId 
                && c.EndDate < DateTime.UtcNow.Date
                && c.Status != ContractStatus.Expired
                && c.Status != ContractStatus.Cancelled
                && c.Status != ContractStatus.Renewed)
            .OrderByDescending(c => c.EndDate)
            .Select(c => new ContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                Name = c.Name,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                RenewalDate = c.RenewalDate,
                AutoRenew = c.AutoRenew,
                TotalValue = c.TotalValue,
                Currency = c.Currency,
                Status = c.Status,
                SignedAt = c.SignedAt,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.Name,
                OpportunityId = c.OpportunityId,
                OpportunityName = c.Opportunity != null ? c.Opportunity.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return contracts;
    }
}

