using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Communication.DTOs;
using crm_backend.Modules.Communication.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Communication;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class AppointmentResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<AppointmentDto>> GetAppointments(
        [Service] IAppointmentService appointmentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new List<AppointmentDto>();
        }

        return await appointmentService.GetAllAppointmentsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<AppointmentDto?> GetAppointment(
        int id,
        [Service] IAppointmentService appointmentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var appointment = await appointmentService.GetAppointmentByIdAsync(id);

        if (appointment == null || appointment.CompanyId != companyId)
        {
            return null;
        }

        return appointment;
    }

    [Authorize]
    public async Task<IEnumerable<AppointmentDto>> GetUpcomingAppointments(
        int? daysAhead,
        [Service] IAppointmentService appointmentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await appointmentService.GetUpcomingAppointmentsAsync(companyId, daysAhead ?? 30);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class AppointmentMutation : BaseResolver
{
    [Authorize]
    public async Task<AppointmentDto> CreateAppointment(
        CreateAppointmentDto input,
        [Service] IAppointmentService appointmentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateAppointmentDto> validator,
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

            var modifiedInput = new CreateAppointmentDto
            {
                Title = input.Title,
                Description = input.Description,
                Location = input.Location,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                TimeZone = input.TimeZone,
                IsAllDay = input.IsAllDay,
                Type = input.Type,
                ReminderMinutesBefore = input.ReminderMinutesBefore,
                CustomerId = input.CustomerId,
                ContactId = input.ContactId,
                OpportunityId = input.OpportunityId,
                CompanyId = companyId,
                CreatedByUserId = userId,
                Attendees = input.Attendees
            };

            return await appointmentService.CreateAppointmentAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create appointment");
        }
    }

    [Authorize]
    public async Task<AppointmentDto?> UpdateAppointment(
        int id,
        UpdateAppointmentDto input,
        [Service] IAppointmentService appointmentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateAppointmentDto> validator,
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

            var existingAppointment = await appointmentService.GetAppointmentByIdAsync(id);
            if (existingAppointment == null || existingAppointment.CompanyId != companyId)
            {
                throw new GraphQLException("Appointment not found or access denied");
            }

            return await appointmentService.UpdateAppointmentAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update appointment");
        }
    }

    [Authorize]
    public async Task<bool> DeleteAppointment(
        int id,
        [Service] IAppointmentService appointmentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingAppointment = await appointmentService.GetAppointmentByIdAsync(id);
            if (existingAppointment == null || existingAppointment.CompanyId != companyId)
            {
                throw new GraphQLException("Appointment not found or access denied");
            }

            return await appointmentService.DeleteAppointmentAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete appointment");
        }
    }
}

