using crm_backend.Modules.Communication.DTOs;

namespace crm_backend.Modules.Communication.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(int? companyId = null);
    Task<IEnumerable<TaskDto>> GetTasksByAssignedUserAsync(int userId);
    Task<TaskDto?> GetTaskByIdAsync(int id);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
    Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto dto);
    Task<bool> DeleteTaskAsync(int id);
    Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(int companyId);
    Task<IEnumerable<TaskDto>> GetTasksByEntityAsync(string entityType, int entityId, int companyId);
}

