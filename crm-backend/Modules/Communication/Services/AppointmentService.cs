using crm_backend.Data;
using crm_backend.Modules.Communication.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Communication.Services;

public class AppointmentService : IAppointmentService
{
    private readonly CrmDbContext _context;

    public AppointmentService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync(int? companyId = null)
    {
        var query = _context.Appointments
            .Include(a => a.Company)
            .Include(a => a.Customer)
            .Include(a => a.Contact)
            .Include(a => a.Opportunity)
            .Include(a => a.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        var appointments = await query
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Location = a.Location,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                TimeZone = a.TimeZone,
                IsAllDay = a.IsAllDay,
                Type = a.Type,
                Status = a.Status,
                CompletedAt = a.CompletedAt,
                CancelledAt = a.CancelledAt,
                CancellationReason = a.CancellationReason,
                ReminderMinutesBefore = a.ReminderMinutesBefore,
                ReminderSent = a.ReminderSent,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer != null ? a.Customer.Name : null,
                ContactId = a.ContactId,
                ContactName = a.Contact != null ? a.Contact.Name : null,
                OpportunityId = a.OpportunityId,
                OpportunityName = a.Opportunity != null ? a.Opportunity.Name : null,
                CreatedByUserId = a.CreatedByUserId,
                CreatedByUserName = a.CreatedByUser.Name,
                CompanyId = a.CompanyId,
                CompanyName = a.Company.Name,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return appointments;
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Company)
            .Include(a => a.Customer)
            .Include(a => a.Contact)
            .Include(a => a.Opportunity)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        return new AppointmentDto
        {
            Id = appointment.Id,
            Title = appointment.Title,
            Description = appointment.Description,
            Location = appointment.Location,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            TimeZone = appointment.TimeZone,
            IsAllDay = appointment.IsAllDay,
            Type = appointment.Type,
            Status = appointment.Status,
            CompletedAt = appointment.CompletedAt,
            CancelledAt = appointment.CancelledAt,
            CancellationReason = appointment.CancellationReason,
            ReminderMinutesBefore = appointment.ReminderMinutesBefore,
            ReminderSent = appointment.ReminderSent,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer != null ? appointment.Customer.Name : null,
            ContactId = appointment.ContactId,
            ContactName = appointment.Contact != null ? appointment.Contact.Name : null,
            OpportunityId = appointment.OpportunityId,
            OpportunityName = appointment.Opportunity != null ? appointment.Opportunity.Name : null,
            CreatedByUserId = appointment.CreatedByUserId,
            CreatedByUserName = appointment.CreatedByUser.Name,
            CompanyId = appointment.CompanyId,
            CompanyName = appointment.Company.Name,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
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

        var appointment = new Appointment
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TimeZone = dto.TimeZone,
            IsAllDay = dto.IsAllDay,
            Type = dto.Type,
            Status = AppointmentStatus.Scheduled,
            ReminderMinutesBefore = dto.ReminderMinutesBefore,
            CustomerId = dto.CustomerId,
            ContactId = dto.ContactId,
            OpportunityId = dto.OpportunityId,
            CreatedByUserId = dto.CreatedByUserId ?? throw new InvalidOperationException("CreatedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Add attendees
        foreach (var attendeeDto in dto.Attendees)
        {
            var attendee = new AppointmentAttendee
            {
                AppointmentId = appointment.Id,
                UserId = attendeeDto.UserId,
                ContactId = attendeeDto.ContactId,
                Email = attendeeDto.Email,
                Name = attendeeDto.Name,
                Status = AttendeeStatus.Invited
            };
            _context.AppointmentAttendees.Add(attendee);
        }

        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(appointment).Reference(a => a.Company).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.CreatedByUser).LoadAsync();
        if (appointment.CustomerId.HasValue)
        {
            await _context.Entry(appointment).Reference(a => a.Customer).LoadAsync();
        }
        if (appointment.ContactId.HasValue)
        {
            await _context.Entry(appointment).Reference(a => a.Contact).LoadAsync();
        }
        if (appointment.OpportunityId.HasValue)
        {
            await _context.Entry(appointment).Reference(a => a.Opportunity).LoadAsync();
        }

        return new AppointmentDto
        {
            Id = appointment.Id,
            Title = appointment.Title,
            Description = appointment.Description,
            Location = appointment.Location,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            TimeZone = appointment.TimeZone,
            IsAllDay = appointment.IsAllDay,
            Type = appointment.Type,
            Status = appointment.Status,
            CompletedAt = appointment.CompletedAt,
            CancelledAt = appointment.CancelledAt,
            CancellationReason = appointment.CancellationReason,
            ReminderMinutesBefore = appointment.ReminderMinutesBefore,
            ReminderSent = appointment.ReminderSent,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer != null ? appointment.Customer.Name : null,
            ContactId = appointment.ContactId,
            ContactName = appointment.Contact != null ? appointment.Contact.Name : null,
            OpportunityId = appointment.OpportunityId,
            OpportunityName = appointment.Opportunity != null ? appointment.Opportunity.Name : null,
            CreatedByUserId = appointment.CreatedByUserId,
            CreatedByUserName = appointment.CreatedByUser.Name,
            CompanyId = appointment.CompanyId,
            CompanyName = appointment.Company.Name,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }

    public async Task<AppointmentDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Company)
            .Include(a => a.Customer)
            .Include(a => a.Contact)
            .Include(a => a.Opportunity)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            appointment.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            appointment.Description = dto.Description;
        }

        if (dto.Location != null)
        {
            appointment.Location = dto.Location;
        }

        if (dto.StartTime.HasValue)
        {
            appointment.StartTime = dto.StartTime.Value;
        }

        if (dto.EndTime.HasValue)
        {
            appointment.EndTime = dto.EndTime.Value;
        }

        if (dto.TimeZone != null)
        {
            appointment.TimeZone = dto.TimeZone;
        }

        if (dto.IsAllDay.HasValue)
        {
            appointment.IsAllDay = dto.IsAllDay.Value;
        }

        if (dto.Type.HasValue)
        {
            appointment.Type = dto.Type.Value;
        }

        if (dto.Status.HasValue)
        {
            appointment.Status = dto.Status.Value;
            
            if (dto.Status.Value == AppointmentStatus.Completed && appointment.CompletedAt == null)
            {
                appointment.CompletedAt = DateTime.UtcNow;
            }
            else if (dto.Status.Value == AppointmentStatus.Cancelled && appointment.CancelledAt == null)
            {
                appointment.CancelledAt = DateTime.UtcNow;
            }
        }

        if (dto.CancellationReason != null)
        {
            appointment.CancellationReason = dto.CancellationReason;
        }

        if (dto.ReminderMinutesBefore.HasValue)
        {
            appointment.ReminderMinutesBefore = dto.ReminderMinutesBefore;
        }

        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new AppointmentDto
        {
            Id = appointment.Id,
            Title = appointment.Title,
            Description = appointment.Description,
            Location = appointment.Location,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            TimeZone = appointment.TimeZone,
            IsAllDay = appointment.IsAllDay,
            Type = appointment.Type,
            Status = appointment.Status,
            CompletedAt = appointment.CompletedAt,
            CancelledAt = appointment.CancelledAt,
            CancellationReason = appointment.CancellationReason,
            ReminderMinutesBefore = appointment.ReminderMinutesBefore,
            ReminderSent = appointment.ReminderSent,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer != null ? appointment.Customer.Name : null,
            ContactId = appointment.ContactId,
            ContactName = appointment.Contact != null ? appointment.Contact.Name : null,
            OpportunityId = appointment.OpportunityId,
            OpportunityName = appointment.Opportunity != null ? appointment.Opportunity.Name : null,
            CreatedByUserId = appointment.CreatedByUserId,
            CreatedByUserName = appointment.CreatedByUser.Name,
            CompanyId = appointment.CompanyId,
            CompanyName = appointment.Company.Name,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return false;

        // Delete attendees first
        var attendees = await _context.AppointmentAttendees.Where(aa => aa.AppointmentId == id).ToListAsync();
        _context.AppointmentAttendees.RemoveRange(attendees);

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync(int companyId, int daysAhead = 30)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(daysAhead);

        var appointments = await _context.Appointments
            .Include(a => a.Company)
            .Include(a => a.Customer)
            .Include(a => a.Contact)
            .Include(a => a.Opportunity)
            .Include(a => a.CreatedByUser)
            .Where(a => a.CompanyId == companyId 
                && a.StartTime >= startDate 
                && a.StartTime <= endDate
                && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Location = a.Location,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                TimeZone = a.TimeZone,
                IsAllDay = a.IsAllDay,
                Type = a.Type,
                Status = a.Status,
                CompletedAt = a.CompletedAt,
                CancelledAt = a.CancelledAt,
                CancellationReason = a.CancellationReason,
                ReminderMinutesBefore = a.ReminderMinutesBefore,
                ReminderSent = a.ReminderSent,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer != null ? a.Customer.Name : null,
                ContactId = a.ContactId,
                ContactName = a.Contact != null ? a.Contact.Name : null,
                OpportunityId = a.OpportunityId,
                OpportunityName = a.Opportunity != null ? a.Opportunity.Name : null,
                CreatedByUserId = a.CreatedByUserId,
                CreatedByUserName = a.CreatedByUser.Name,
                CompanyId = a.CompanyId,
                CompanyName = a.Company.Name,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return appointments;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByEntityAsync(string entityType, int entityId, int companyId)
    {
        var query = _context.Appointments
            .Include(a => a.Company)
            .Include(a => a.Customer)
            .Include(a => a.Contact)
            .Include(a => a.Opportunity)
            .Include(a => a.Quote)
            .Include(a => a.CreatedByUser)
            .Where(a => a.CompanyId == companyId)
            .AsQueryable();

        // Filter by entity type
        query = entityType.ToLower() switch
        {
            "customer" => query.Where(a => a.CustomerId == entityId),
            "opportunity" => query.Where(a => a.OpportunityId == entityId),
            "quote" => query.Where(a => a.QuoteId == entityId),
            _ => throw new ArgumentException($"Unsupported entity type: {entityType}")
        };

        var appointments = await query
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Location = a.Location,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                TimeZone = a.TimeZone,
                IsAllDay = a.IsAllDay,
                Type = a.Type,
                Status = a.Status,
                CompletedAt = a.CompletedAt,
                CancelledAt = a.CancelledAt,
                CancellationReason = a.CancellationReason,
                ReminderMinutesBefore = a.ReminderMinutesBefore,
                ReminderSent = a.ReminderSent,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer != null ? a.Customer.Name : null,
                ContactId = a.ContactId,
                ContactName = a.Contact != null ? a.Contact.Name : null,
                OpportunityId = a.OpportunityId,
                OpportunityName = a.Opportunity != null ? a.Opportunity.Name : null,
                CreatedByUserId = a.CreatedByUserId,
                CreatedByUserName = a.CreatedByUser.Name,
                CompanyId = a.CompanyId,
                CompanyName = a.Company.Name,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return appointments;
    }
}

