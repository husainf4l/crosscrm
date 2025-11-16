using crm_backend.Data;
using crm_backend.Modules.Communication.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Communication.Services;

public class TaskService : ITaskService
{
    private readonly CrmDbContext _context;

    public TaskService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(int? companyId = null)
    {
        var query = _context.Tasks
            .Include(t => t.Company)
            .Include(t => t.Customer)
            .Include(t => t.Contact)
            .Include(t => t.Opportunity)
            .Include(t => t.Ticket)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(t => t.CompanyId == companyId.Value);
        }

        var tasks = await query
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt,
                ReminderDate = t.ReminderDate,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.Name : null,
                ContactId = t.ContactId,
                ContactName = t.Contact != null ? t.Contact.Name : null,
                OpportunityId = t.OpportunityId,
                OpportunityName = t.Opportunity != null ? t.Opportunity.Name : null,
                TicketId = t.TicketId,
                TicketTitle = t.Ticket != null ? t.Ticket.Title : null,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser.Name,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser.Name,
                CompanyId = t.CompanyId,
                CompanyName = t.Company.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return tasks;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByAssignedUserAsync(int userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Company)
            .Include(t => t.Customer)
            .Include(t => t.Contact)
            .Include(t => t.Opportunity)
            .Include(t => t.Ticket)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.AssignedToUserId == userId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt,
                ReminderDate = t.ReminderDate,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.Name : null,
                ContactId = t.ContactId,
                ContactName = t.Contact != null ? t.Contact.Name : null,
                OpportunityId = t.OpportunityId,
                OpportunityName = t.Opportunity != null ? t.Opportunity.Name : null,
                TicketId = t.TicketId,
                TicketTitle = t.Ticket != null ? t.Ticket.Title : null,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser.Name,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser.Name,
                CompanyId = t.CompanyId,
                CompanyName = t.Company.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return tasks;
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Company)
            .Include(t => t.Customer)
            .Include(t => t.Contact)
            .Include(t => t.Opportunity)
            .Include(t => t.Ticket)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return null;

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            ReminderDate = task.ReminderDate,
            CustomerId = task.CustomerId,
            CustomerName = task.Customer != null ? task.Customer.Name : null,
            ContactId = task.ContactId,
            ContactName = task.Contact != null ? task.Contact.Name : null,
            OpportunityId = task.OpportunityId,
            OpportunityName = task.Opportunity != null ? task.Opportunity.Name : null,
            TicketId = task.TicketId,
            TicketTitle = task.Ticket != null ? task.Ticket.Title : null,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUserName = task.AssignedToUser.Name,
            CreatedByUserId = task.CreatedByUserId,
            CreatedByUserName = task.CreatedByUser.Name,
            CompanyId = task.CompanyId,
            CompanyName = task.Company.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");

        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify assigned user exists and belongs to company
        var userCompany = await _context.UserCompanies
            .AnyAsync(uc => uc.UserId == dto.AssignedToUserId && uc.CompanyId == companyId);
        if (!userCompany)
        {
            throw new InvalidOperationException("Assigned user does not belong to the specified company.");
        }

        // Verify customer exists and belongs to company (if provided)
        if (dto.CustomerId.HasValue)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId.Value);
            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with ID {dto.CustomerId.Value} does not exist.");
            }
            if (customer.CompanyId != companyId)
            {
                throw new InvalidOperationException("Customer does not belong to the specified company.");
            }
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
        }

        var task = new Task
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = TaskStatus.NotStarted,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ReminderDate = dto.ReminderDate,
            CustomerId = dto.CustomerId,
            ContactId = dto.ContactId,
            OpportunityId = dto.OpportunityId,
            TicketId = dto.TicketId,
            AssignedToUserId = dto.AssignedToUserId,
            CreatedByUserId = dto.CreatedByUserId ?? throw new InvalidOperationException("CreatedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(task).Reference(t => t.Company).LoadAsync();
        await _context.Entry(task).Reference(t => t.AssignedToUser).LoadAsync();
        await _context.Entry(task).Reference(t => t.CreatedByUser).LoadAsync();
        if (task.CustomerId.HasValue)
        {
            await _context.Entry(task).Reference(t => t.Customer).LoadAsync();
        }
        if (task.ContactId.HasValue)
        {
            await _context.Entry(task).Reference(t => t.Contact).LoadAsync();
        }
        if (task.OpportunityId.HasValue)
        {
            await _context.Entry(task).Reference(t => t.Opportunity).LoadAsync();
        }
        if (task.TicketId.HasValue)
        {
            await _context.Entry(task).Reference(t => t.Ticket).LoadAsync();
        }

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            ReminderDate = task.ReminderDate,
            CustomerId = task.CustomerId,
            CustomerName = task.Customer != null ? task.Customer.Name : null,
            ContactId = task.ContactId,
            ContactName = task.Contact != null ? task.Contact.Name : null,
            OpportunityId = task.OpportunityId,
            OpportunityName = task.Opportunity != null ? task.Opportunity.Name : null,
            TicketId = task.TicketId,
            TicketTitle = task.Ticket != null ? task.Ticket.Title : null,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUserName = task.AssignedToUser.Name,
            CreatedByUserId = task.CreatedByUserId,
            CreatedByUserName = task.CreatedByUser.Name,
            CompanyId = task.CompanyId,
            CompanyName = task.Company.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Company)
            .Include(t => t.Customer)
            .Include(t => t.Contact)
            .Include(t => t.Opportunity)
            .Include(t => t.Ticket)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            task.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            task.Description = dto.Description;
        }

        if (dto.Status.HasValue)
        {
            task.Status = dto.Status.Value;

            if (dto.Status.Value == TaskStatus.Completed && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
        }

        if (dto.Priority.HasValue)
        {
            task.Priority = dto.Priority.Value;
        }

        if (dto.DueDate.HasValue)
        {
            task.DueDate = dto.DueDate;
        }

        if (dto.ReminderDate.HasValue)
        {
            task.ReminderDate = dto.ReminderDate;
        }

        if (dto.AssignedToUserId.HasValue)
        {
            // Verify user belongs to company
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.AssignedToUserId.Value && uc.CompanyId == task.CompanyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("Assigned user does not belong to the task's company.");
            }
            task.AssignedToUserId = dto.AssignedToUserId.Value;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload assigned user if changed
        if (dto.AssignedToUserId.HasValue)
        {
            await _context.Entry(task).Reference(t => t.AssignedToUser).LoadAsync();
        }

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            ReminderDate = task.ReminderDate,
            CustomerId = task.CustomerId,
            CustomerName = task.Customer != null ? task.Customer.Name : null,
            ContactId = task.ContactId,
            ContactName = task.Contact != null ? task.Contact.Name : null,
            OpportunityId = task.OpportunityId,
            OpportunityName = task.Opportunity != null ? task.Opportunity.Name : null,
            TicketId = task.TicketId,
            TicketTitle = task.Ticket != null ? task.Ticket.Title : null,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUserName = task.AssignedToUser.Name,
            CreatedByUserId = task.CreatedByUserId,
            CreatedByUserName = task.CreatedByUser.Name,
            CompanyId = task.CompanyId,
            CompanyName = task.Company.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(int companyId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Company)
            .Include(t => t.Customer)
            .Include(t => t.Contact)
            .Include(t => t.Opportunity)
            .Include(t => t.Ticket)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.CompanyId == companyId
                && t.DueDate.HasValue
                && t.DueDate.Value < DateTime.UtcNow
                && t.Status != TaskStatus.Completed
                && t.Status != TaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt,
                ReminderDate = t.ReminderDate,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.Name : null,
                ContactId = t.ContactId,
                ContactName = t.Contact != null ? t.Contact.Name : null,
                OpportunityId = t.OpportunityId,
                OpportunityName = t.Opportunity != null ? t.Opportunity.Name : null,
                TicketId = t.TicketId,
                TicketTitle = t.Ticket != null ? t.Ticket.Title : null,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser.Name,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser.Name,
                CompanyId = t.CompanyId,
                CompanyName = t.Company.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return tasks;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByEntityAsync(string entityType, int entityId, int companyId)
    {
        var query = _context.Tasks
            .Include(t => t.Company)
            .Include(t => t.Customer)
            .Include(t => t.Contact)
            .Include(t => t.Opportunity)
            .Include(t => t.Quote)
            .Include(t => t.Invoice)
            .Include(t => t.Contract)
            .Include(t => t.Ticket)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.CompanyId == companyId)
            .AsQueryable();

        // Filter by entity type
        query = entityType.ToLower() switch
        {
            "customer" => query.Where(t => t.CustomerId == entityId),
            "opportunity" => query.Where(t => t.OpportunityId == entityId),
            "quote" => query.Where(t => t.QuoteId == entityId),
            "invoice" => query.Where(t => t.InvoiceId == entityId),
            "contract" => query.Where(t => t.ContractId == entityId),
            "ticket" => query.Where(t => t.TicketId == entityId),
            _ => throw new ArgumentException($"Unsupported entity type: {entityType}")
        };

        var tasks = await query
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt,
                ReminderDate = t.ReminderDate,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.Name : null,
                ContactId = t.ContactId,
                ContactName = t.Contact != null ? t.Contact.Name : null,
                OpportunityId = t.OpportunityId,
                OpportunityName = t.Opportunity != null ? t.Opportunity.Name : null,
                TicketId = t.TicketId,
                TicketTitle = t.Ticket != null ? t.Ticket.Title : null,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser.Name,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser.Name,
                CompanyId = t.CompanyId,
                CompanyName = t.Company.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return tasks;
    }
}

