using crm_backend.Modules.Financial.DTOs;

namespace crm_backend.Modules.Financial.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync(int? companyId = null);
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto dto);
    Task<InvoiceDto?> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto);
    Task<bool> DeleteInvoiceAsync(int id);
    Task UpdateInvoiceStatusAsync(int invoiceId);
}

