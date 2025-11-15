using crm_backend.Modules.Financial.DTOs;

namespace crm_backend.Modules.Financial.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(int? companyId = null);
    Task<IEnumerable<PaymentDto>> GetPaymentsByInvoiceAsync(int invoiceId);
    Task<PaymentDto?> GetPaymentByIdAsync(int id);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
    Task<bool> DeletePaymentAsync(int id);
}

