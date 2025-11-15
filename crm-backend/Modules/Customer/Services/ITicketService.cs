using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface ITicketService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync(int companyId);
    Task<IEnumerable<TicketDto>> GetTicketsByCustomerAsync(int customerId);
    Task<TicketDto?> GetTicketByIdAsync(int id);
    Task<TicketDto> CreateTicketAsync(CreateTicketDto ticketDto);
    Task<TicketDto?> UpdateTicketAsync(int id, UpdateTicketDto ticketDto);
    Task<bool> DeleteTicketAsync(int id);
    Task<IEnumerable<TicketDto>> GetTicketsByAssignedUserAsync(int userId);
    Task<IEnumerable<TicketDto>> GetTicketsByStatusAsync(int companyId, TicketStatus status);
}