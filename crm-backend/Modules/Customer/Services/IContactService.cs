using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface IContactService
{
    Task<IEnumerable<ContactDto>> GetAllContactsAsync(int customerId);
    Task<ContactDto?> GetContactByIdAsync(int id);
    Task<ContactDto> CreateContactAsync(CreateContactDto contactDto);
    Task<ContactDto?> UpdateContactAsync(int id, UpdateContactDto contactDto);
    Task<bool> DeleteContactAsync(int id);
}