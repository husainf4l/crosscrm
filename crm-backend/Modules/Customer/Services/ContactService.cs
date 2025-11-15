using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class ContactService : IContactService
{
    private readonly CrmDbContext _context;

    public ContactService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContactDto>> GetAllContactsAsync(int customerId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return contacts.Select(MapToDto);
    }

    public async Task<ContactDto?> GetContactByIdAsync(int id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        return contact != null ? MapToDto(contact) : null;
    }

    public async Task<ContactDto> CreateContactAsync(CreateContactDto contactDto)
    {
        // If this is set as primary, unset other primary contacts for this customer
        if (contactDto.IsPrimary)
        {
            var existingPrimaryContacts = await _context.Contacts
                .Where(c => c.CustomerId == contactDto.CustomerId && c.IsPrimary)
                .ToListAsync();

            foreach (var existingContact in existingPrimaryContacts)
            {
                existingContact.IsPrimary = false;
                existingContact.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Split Name into FirstName and LastName
        var nameParts = contactDto.Name.Trim().Split(new[] { ' ' }, 2);
        var firstName = nameParts.Length > 0 ? nameParts[0] : contactDto.Name;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var contact = new Contact
        {
            FirstName = firstName,
            LastName = lastName,
            Title = contactDto.Title,
            Email = contactDto.Email,
            Phone = contactDto.Phone,
            Mobile = contactDto.Mobile,
            IsPrimary = contactDto.IsPrimary,
            CustomerId = contactDto.CustomerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return MapToDto(contact);
    }

    public async Task<ContactDto?> UpdateContactAsync(int id, UpdateContactDto contactDto)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null)
        {
            return null;
        }

        // If setting as primary, unset other primary contacts for this customer
        if (contactDto.IsPrimary.HasValue && contactDto.IsPrimary.Value && !contact.IsPrimary)
        {
            var existingPrimaryContacts = await _context.Contacts
                .Where(c => c.CustomerId == contact.CustomerId && c.IsPrimary && c.Id != id)
                .ToListAsync();

            foreach (var otherContact in existingPrimaryContacts)
            {
                otherContact.IsPrimary = false;
                otherContact.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (contactDto.Name != null)
        {
            var nameParts = contactDto.Name.Trim().Split(new[] { ' ' }, 2);
            contact.FirstName = nameParts.Length > 0 ? nameParts[0] : contactDto.Name;
            contact.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        }
        if (contactDto.Title != null) contact.Title = contactDto.Title;
        if (contactDto.Email != null) contact.Email = contactDto.Email;
        if (contactDto.Phone != null) contact.Phone = contactDto.Phone;
        if (contactDto.Mobile != null) contact.Mobile = contactDto.Mobile;
        if (contactDto.IsPrimary.HasValue) contact.IsPrimary = contactDto.IsPrimary.Value;

        contact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(contact);
    }

    public async Task<bool> DeleteContactAsync(int id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null)
        {
            return false;
        }

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ContactDto MapToDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            Name = contact.Name,
            Title = contact.Title,
            Email = contact.Email,
            Phone = contact.Phone,
            Mobile = contact.Mobile,
            IsPrimary = contact.IsPrimary,
            CustomerId = contact.CustomerId ?? 0, // Default to 0 if null (Account-based contact)
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }
}