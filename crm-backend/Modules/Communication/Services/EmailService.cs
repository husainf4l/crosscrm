using crm_backend.Data;
using crm_backend.Modules.Communication.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Communication.Services;

public class EmailService : IEmailService
{
    private readonly CrmDbContext _context;

    public EmailService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmailDto>> GetAllEmailsAsync(int? companyId = null)
    {
        var query = _context.Emails
            .Include(e => e.Company)
            .Include(e => e.Customer)
            .Include(e => e.Contact)
            .Include(e => e.Opportunity)
            .Include(e => e.Ticket)
            .Include(e => e.SentByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(e => e.CompanyId == companyId.Value);
        }

        var emails = await query
            .Select(e => new EmailDto
            {
                Id = e.Id,
                Subject = e.Subject,
                Body = e.Body,
                BodyHtml = e.BodyHtml,
                FromEmail = e.FromEmail,
                ToEmail = e.ToEmail,
                CcEmail = e.CcEmail,
                BccEmail = e.BccEmail,
                Direction = e.Direction,
                Status = e.Status,
                SentAt = e.SentAt,
                ReceivedAt = e.ReceivedAt,
                IsRead = e.IsRead,
                ThreadId = e.ThreadId,
                ParentEmailId = e.ParentEmailId,
                CustomerId = e.CustomerId,
                CustomerName = e.Customer != null ? e.Customer.Name : null,
                ContactId = e.ContactId,
                ContactName = e.Contact != null ? e.Contact.Name : null,
                OpportunityId = e.OpportunityId,
                OpportunityName = e.Opportunity != null ? e.Opportunity.Name : null,
                TicketId = e.TicketId,
                TicketTitle = e.Ticket != null ? e.Ticket.Title : null,
                SentByUserId = e.SentByUserId,
                SentByUserName = e.SentByUser != null ? e.SentByUser.Name : null,
                CompanyId = e.CompanyId,
                CompanyName = e.Company.Name,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return emails;
    }

    public async Task<EmailDto?> GetEmailByIdAsync(int id)
    {
        var email = await _context.Emails
            .Include(e => e.Company)
            .Include(e => e.Customer)
            .Include(e => e.Contact)
            .Include(e => e.Opportunity)
            .Include(e => e.Ticket)
            .Include(e => e.SentByUser)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (email == null) return null;

        return new EmailDto
        {
            Id = email.Id,
            Subject = email.Subject,
            Body = email.Body,
            BodyHtml = email.BodyHtml,
            FromEmail = email.FromEmail,
            ToEmail = email.ToEmail,
            CcEmail = email.CcEmail,
            BccEmail = email.BccEmail,
            Direction = email.Direction,
            Status = email.Status,
            SentAt = email.SentAt,
            ReceivedAt = email.ReceivedAt,
            IsRead = email.IsRead,
            ThreadId = email.ThreadId,
            ParentEmailId = email.ParentEmailId,
            CustomerId = email.CustomerId,
            CustomerName = email.Customer != null ? email.Customer.Name : null,
            ContactId = email.ContactId,
            ContactName = email.Contact != null ? email.Contact.Name : null,
            OpportunityId = email.OpportunityId,
            OpportunityName = email.Opportunity != null ? email.Opportunity.Name : null,
            TicketId = email.TicketId,
            TicketTitle = email.Ticket != null ? email.Ticket.Title : null,
            SentByUserId = email.SentByUserId,
            SentByUserName = email.SentByUser != null ? email.SentByUser.Name : null,
            CompanyId = email.CompanyId,
            CompanyName = email.Company.Name,
            CreatedAt = email.CreatedAt
        };
    }

    public async Task<EmailDto> CreateEmailAsync(CreateEmailDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        
        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
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

        // Verify contact exists and belongs to customer (if provided)
        if (dto.ContactId.HasValue)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == dto.ContactId.Value);
            if (contact == null)
            {
                throw new InvalidOperationException($"Contact with ID {dto.ContactId.Value} does not exist.");
            }
            if (dto.CustomerId.HasValue && contact.CustomerId != dto.CustomerId.Value)
            {
                throw new InvalidOperationException("Contact does not belong to the specified customer.");
            }
        }

        var email = new Email
        {
            Subject = dto.Subject,
            Body = dto.Body,
            BodyHtml = dto.BodyHtml,
            FromEmail = dto.FromEmail,
            ToEmail = dto.ToEmail,
            CcEmail = dto.CcEmail,
            BccEmail = dto.BccEmail,
            Direction = dto.Direction,
            Status = dto.Direction == EmailDirection.Outbound ? EmailStatus.Draft : EmailStatus.Received,
            CustomerId = dto.CustomerId,
            ContactId = dto.ContactId,
            OpportunityId = dto.OpportunityId,
            TicketId = dto.TicketId,
            SentByUserId = dto.SentByUserId,
            ThreadId = dto.ThreadId,
            ParentEmailId = dto.ParentEmailId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Direction == EmailDirection.Outbound && email.Status == EmailStatus.Sent)
        {
            email.SentAt = DateTime.UtcNow;
        }
        else if (dto.Direction == EmailDirection.Inbound)
        {
            email.ReceivedAt = DateTime.UtcNow;
        }

        _context.Emails.Add(email);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(email).Reference(e => e.Company).LoadAsync();
        if (email.CustomerId.HasValue)
        {
            await _context.Entry(email).Reference(e => e.Customer).LoadAsync();
        }
        if (email.ContactId.HasValue)
        {
            await _context.Entry(email).Reference(e => e.Contact).LoadAsync();
        }
        if (email.OpportunityId.HasValue)
        {
            await _context.Entry(email).Reference(e => e.Opportunity).LoadAsync();
        }
        if (email.TicketId.HasValue)
        {
            await _context.Entry(email).Reference(e => e.Ticket).LoadAsync();
        }
        if (email.SentByUserId.HasValue)
        {
            await _context.Entry(email).Reference(e => e.SentByUser).LoadAsync();
        }

        return new EmailDto
        {
            Id = email.Id,
            Subject = email.Subject,
            Body = email.Body,
            BodyHtml = email.BodyHtml,
            FromEmail = email.FromEmail,
            ToEmail = email.ToEmail,
            CcEmail = email.CcEmail,
            BccEmail = email.BccEmail,
            Direction = email.Direction,
            Status = email.Status,
            SentAt = email.SentAt,
            ReceivedAt = email.ReceivedAt,
            IsRead = email.IsRead,
            ThreadId = email.ThreadId,
            ParentEmailId = email.ParentEmailId,
            CustomerId = email.CustomerId,
            CustomerName = email.Customer != null ? email.Customer.Name : null,
            ContactId = email.ContactId,
            ContactName = email.Contact != null ? email.Contact.Name : null,
            OpportunityId = email.OpportunityId,
            OpportunityName = email.Opportunity != null ? email.Opportunity.Name : null,
            TicketId = email.TicketId,
            TicketTitle = email.Ticket != null ? email.Ticket.Title : null,
            SentByUserId = email.SentByUserId,
            SentByUserName = email.SentByUser != null ? email.SentByUser.Name : null,
            CompanyId = email.CompanyId,
            CompanyName = email.Company.Name,
            CreatedAt = email.CreatedAt
        };
    }

    public async Task<EmailDto?> UpdateEmailAsync(int id, UpdateEmailDto dto)
    {
        var email = await _context.Emails
            .Include(e => e.Company)
            .Include(e => e.Customer)
            .Include(e => e.Contact)
            .Include(e => e.Opportunity)
            .Include(e => e.Ticket)
            .Include(e => e.SentByUser)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (email == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Subject))
        {
            email.Subject = dto.Subject;
        }

        if (dto.Body != null)
        {
            email.Body = dto.Body;
        }

        if (dto.BodyHtml != null)
        {
            email.BodyHtml = dto.BodyHtml;
        }

        if (dto.Status.HasValue)
        {
            email.Status = dto.Status.Value;
            
            if (dto.Status.Value == EmailStatus.Sent && email.SentAt == null)
            {
                email.SentAt = DateTime.UtcNow;
            }
        }

        if (dto.IsRead.HasValue)
        {
            email.IsRead = dto.IsRead.Value;
        }

        await _context.SaveChangesAsync();

        return new EmailDto
        {
            Id = email.Id,
            Subject = email.Subject,
            Body = email.Body,
            BodyHtml = email.BodyHtml,
            FromEmail = email.FromEmail,
            ToEmail = email.ToEmail,
            CcEmail = email.CcEmail,
            BccEmail = email.BccEmail,
            Direction = email.Direction,
            Status = email.Status,
            SentAt = email.SentAt,
            ReceivedAt = email.ReceivedAt,
            IsRead = email.IsRead,
            ThreadId = email.ThreadId,
            ParentEmailId = email.ParentEmailId,
            CustomerId = email.CustomerId,
            CustomerName = email.Customer != null ? email.Customer.Name : null,
            ContactId = email.ContactId,
            ContactName = email.Contact != null ? email.Contact.Name : null,
            OpportunityId = email.OpportunityId,
            OpportunityName = email.Opportunity != null ? email.Opportunity.Name : null,
            TicketId = email.TicketId,
            TicketTitle = email.Ticket != null ? email.Ticket.Title : null,
            SentByUserId = email.SentByUserId,
            SentByUserName = email.SentByUser != null ? email.SentByUser.Name : null,
            CompanyId = email.CompanyId,
            CompanyName = email.Company.Name,
            CreatedAt = email.CreatedAt
        };
    }

    public async Task<bool> DeleteEmailAsync(int id)
    {
        var email = await _context.Emails.FindAsync(id);
        if (email == null) return false;

        _context.Emails.Remove(email);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EmailDto?> MarkEmailAsReadAsync(int id, bool isRead)
    {
        var email = await _context.Emails
            .Include(e => e.Company)
            .Include(e => e.Customer)
            .Include(e => e.Contact)
            .Include(e => e.Opportunity)
            .Include(e => e.Ticket)
            .Include(e => e.SentByUser)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (email == null) return null;

        email.IsRead = isRead;
        await _context.SaveChangesAsync();

        return new EmailDto
        {
            Id = email.Id,
            Subject = email.Subject,
            Body = email.Body,
            BodyHtml = email.BodyHtml,
            FromEmail = email.FromEmail,
            ToEmail = email.ToEmail,
            CcEmail = email.CcEmail,
            BccEmail = email.BccEmail,
            Direction = email.Direction,
            Status = email.Status,
            SentAt = email.SentAt,
            ReceivedAt = email.ReceivedAt,
            IsRead = email.IsRead,
            ThreadId = email.ThreadId,
            ParentEmailId = email.ParentEmailId,
            CustomerId = email.CustomerId,
            CustomerName = email.Customer != null ? email.Customer.Name : null,
            ContactId = email.ContactId,
            ContactName = email.Contact != null ? email.Contact.Name : null,
            OpportunityId = email.OpportunityId,
            OpportunityName = email.Opportunity != null ? email.Opportunity.Name : null,
            TicketId = email.TicketId,
            TicketTitle = email.Ticket != null ? email.Ticket.Title : null,
            SentByUserId = email.SentByUserId,
            SentByUserName = email.SentByUser != null ? email.SentByUser.Name : null,
            CompanyId = email.CompanyId,
            CompanyName = email.Company.Name,
            CreatedAt = email.CreatedAt
        };
    }
}

