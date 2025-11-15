using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class TicketService : ITicketService
{
    private readonly CrmDbContext _context;

    public TicketService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync(int companyId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedUser)
            .Where(t => t.Customer.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByCustomerAsync(int customerId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedUser)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto?> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        return ticket != null ? MapToDto(ticket) : null;
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto ticketDto)
    {
        var ticket = new Ticket
        {
            Title = ticketDto.Title,
            Description = ticketDto.Description,
            Priority = (TicketPriority)ticketDto.Priority,
            Status = TicketStatus.Open,
            CustomerId = ticketDto.CustomerId,
            AssignedUserId = ticketDto.AssignedUserId,
            Tags = ticketDto.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticket.Id) ?? throw new InvalidOperationException("Failed to create ticket");
    }

    public async Task<TicketDto?> UpdateTicketAsync(int id, UpdateTicketDto ticketDto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return null;
        }

        if (ticketDto.Title != null) ticket.Title = ticketDto.Title;
        if (ticketDto.Description != null) ticket.Description = ticketDto.Description;
        if (ticketDto.Status.HasValue)
        {
            ticket.Status = (TicketStatus)ticketDto.Status.Value;
            if (ticketDto.Status.Value == DTOs.TicketStatus.Resolved || ticketDto.Status.Value == DTOs.TicketStatus.Closed)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }
        }
        if (ticketDto.Priority.HasValue) ticket.Priority = (TicketPriority)ticketDto.Priority.Value;
        if (ticketDto.AssignedUserId.HasValue) ticket.AssignedUserId = ticketDto.AssignedUserId.Value;
        if (ticketDto.Resolution != null) ticket.Resolution = ticketDto.Resolution;
        if (ticketDto.Tags != null) ticket.Tags = ticketDto.Tags;

        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetTicketByIdAsync(id);
    }

    public async Task<bool> DeleteTicketAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return false;
        }

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByAssignedUserAsync(int userId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedUser)
            .Where(t => t.AssignedUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByStatusAsync(int companyId, TicketStatus status)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedUser)
            .Where(t => t.Customer.CompanyId == companyId && t.Status == (TicketStatus)status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    private static TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = (DTOs.TicketStatus)ticket.Status,
            Priority = (DTOs.TicketPriority)ticket.Priority,
            CustomerId = ticket.CustomerId,
            CustomerName = ticket.Customer?.Name,
            AssignedUserId = ticket.AssignedUserId,
            AssignedUserName = ticket.AssignedUser?.Name,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Resolution = ticket.Resolution,
            Tags = ticket.Tags
        };
    }
}