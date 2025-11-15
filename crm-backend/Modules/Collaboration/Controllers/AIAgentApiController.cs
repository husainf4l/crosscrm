using Microsoft.AspNetCore.Mvc;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using FluentValidation;
using System.Security.Claims;

namespace crm_backend.Modules.Collaboration.Controllers;

[ApiController]
[Route("api/ai-agent")]
public class AIAgentApiController : ControllerBase
{
    private readonly IAIAgentToolService _toolService;
    private readonly IAIAgentApiKeyService _apiKeyService;
    private readonly IValidator<ExecuteToolDto> _executeToolValidator;
    private readonly ILogger<AIAgentApiController> _logger;

    public AIAgentApiController(
        IAIAgentToolService toolService,
        IAIAgentApiKeyService apiKeyService,
        IValidator<ExecuteToolDto> executeToolValidator,
        ILogger<AIAgentApiController> logger)
    {
        _toolService = toolService;
        _apiKeyService = apiKeyService;
        _executeToolValidator = executeToolValidator;
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet("tools")]
    public async Task<IActionResult> GetTools()
    {
        try
        {
            var agentId = GetAgentIdFromContext();
            var companyId = GetCompanyIdFromContext();

            var tools = await _toolService.GetToolsByAgentAsync(agentId, companyId);
            return Ok(tools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tools");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("execute-tool")]
    public async Task<IActionResult> ExecuteTool([FromBody] ExecuteToolDto dto)
    {
        try
        {
            // Validate request
            var validationResult = await _executeToolValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var apiKeyId = GetApiKeyIdFromContext();
            var companyId = GetCompanyIdFromContext();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _toolService.ExecuteToolAsync(dto.ToolName, dto.Parameters, apiKeyId, companyId);
            stopwatch.Stop();

            // Log API key usage
            await _apiKeyService.LogApiKeyUsageAsync(
                apiKeyId,
                "/api/ai-agent/execute-tool",
                "POST",
                result.Success ? 200 : 400,
                (int)stopwatch.ElapsedMilliseconds,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString()
            );

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private int GetAgentIdFromContext()
    {
        if (HttpContext.Items.TryGetValue("AIAgentId", out var agentId) && agentId is int id)
            return id;
        
        var claim = HttpContext.User.FindFirst("AIAgentId");
        if (claim != null && int.TryParse(claim.Value, out var claimId))
            return claimId;

        throw new UnauthorizedAccessException("Agent ID not found in context");
    }

    private int GetApiKeyIdFromContext()
    {
        if (HttpContext.Items.TryGetValue("AIAgentApiKeyId", out var apiKeyId) && apiKeyId is int id)
            return id;
        
        var claim = HttpContext.User.FindFirst("AIAgentApiKeyId");
        if (claim != null && int.TryParse(claim.Value, out var claimId))
            return claimId;

        throw new UnauthorizedAccessException("API key ID not found in context");
    }

    private int GetCompanyIdFromContext()
    {
        if (HttpContext.Items.TryGetValue("AIAgentCompanyId", out var companyId) && companyId is int id)
            return id;
        
        var claim = HttpContext.User.FindFirst("CompanyId");
        if (claim != null && int.TryParse(claim.Value, out var claimId))
            return claimId;

        throw new UnauthorizedAccessException("Company ID not found in context");
    }
}

