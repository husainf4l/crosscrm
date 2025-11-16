using crm_backend.Modules.Communication.DTOs;

namespace crm_backend.Modules.Communication.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync(int? companyId = null);
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto);
    Task<bool> DeleteAppointmentAsync(int id);
    Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync(int companyId, int daysAhead = 30);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByEntityAsync(string entityType, int entityId, int companyId);
}

