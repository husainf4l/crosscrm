using crm_backend.Modules.Opportunity.DTOs;

namespace crm_backend.Modules.Opportunity.Services;

public interface IPipelineStageService
{
    Task<IEnumerable<PipelineStageDto>> GetAllPipelineStagesAsync(int? companyId = null);
    Task<PipelineStageDto?> GetPipelineStageByIdAsync(int id);
    Task<PipelineStageDto> CreatePipelineStageAsync(CreatePipelineStageDto dto);
    Task<PipelineStageDto?> UpdatePipelineStageAsync(int id, UpdatePipelineStageDto dto);
    Task<bool> DeletePipelineStageAsync(int id);
}

