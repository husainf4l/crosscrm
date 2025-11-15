using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Communication.DTOs;
using crm_backend.Modules.Communication.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Communication;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class TaskResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<TaskDto>> GetTasks(
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<TaskDto>();
        }

        return await taskService.GetAllTasksAsync(companyId.Value);
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<TaskDto>> GetMyTasks(
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await taskService.GetTasksByAssignedUserAsync(userId);
    }

    [Authorize]
    public async Task<IEnumerable<TaskDto>> GetOverdueTasks(
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await taskService.GetOverdueTasksAsync(companyId);
    }

    [Authorize]
    public async Task<TaskDto?> GetTask(
        int id,
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var task = await taskService.GetTaskByIdAsync(id);

        if (task == null || task.CompanyId != companyId)
        {
            return null;
        }

        return task;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class TaskMutation : BaseResolver
{
    [Authorize]
    public async Task<TaskDto> CreateTask(
        CreateTaskDto input,
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateTaskDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);

            var modifiedInput = new CreateTaskDto
            {
                Title = input.Title,
                Description = input.Description,
                Priority = input.Priority,
                DueDate = input.DueDate,
                ReminderDate = input.ReminderDate,
                CustomerId = input.CustomerId,
                ContactId = input.ContactId,
                OpportunityId = input.OpportunityId,
                TicketId = input.TicketId,
                AssignedToUserId = input.AssignedToUserId,
                CompanyId = companyId,
                CreatedByUserId = userId
            };

            return await taskService.CreateTaskAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create task");
        }
    }

    [Authorize]
    public async Task<TaskDto?> UpdateTask(
        int id,
        UpdateTaskDto input,
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateTaskDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingTask = await taskService.GetTaskByIdAsync(id);
            if (existingTask == null || existingTask.CompanyId != companyId)
            {
                throw new GraphQLException("Task not found or access denied");
            }

            return await taskService.UpdateTaskAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update task");
        }
    }

    [Authorize]
    public async Task<bool> DeleteTask(
        int id,
        [Service] ITaskService taskService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingTask = await taskService.GetTaskByIdAsync(id);
            if (existingTask == null || existingTask.CompanyId != companyId)
            {
                throw new GraphQLException("Task not found or access denied");
            }

            return await taskService.DeleteTaskAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete task");
        }
    }
}

