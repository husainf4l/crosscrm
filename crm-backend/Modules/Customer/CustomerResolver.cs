using System.Security.Claims;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Customer.DTOs;
using crm_backend.Modules.Customer.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Customer;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class CustomerResolver : BaseResolver
{
    [Authorize]
    public async Task<CustomerConnectionDto> GetCustomers(
        int? first,
        string? after,
        string? search,
        CustomerFiltersDto? filters,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new CustomerConnectionDto
            {
                Edges = new List<CustomerEdgeDto>(),
                PageInfo = new PageInfoDto
                {
                    HasNextPage = false,
                    HasPreviousPage = false,
                    StartCursor = null,
                    EndCursor = null
                },
                TotalCount = 0
            };
        }

        return await customerService.GetCustomersConnectionAsync(
            companyId.Value,
            first,
            after,
            search,
            filters);
    }

    [Authorize]
    public async Task<CustomerDto?> GetCustomer(
        int id,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var customer = await customerService.GetCustomerByIdAsync(id);

        if (customer == null || customer.CompanyId != companyId)
        {
            return null;
        }

        return customer;
    }

    [Authorize]
    [UseProjection]
    public async Task<CustomerDto?> GetCustomerWithContacts(
        int id,
        [Service] ICustomerService customerService,
        [Service] IContactService contactService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var customer = await customerService.GetCustomerByIdAsync(id);

        if (customer == null || customer.CompanyId != companyId)
        {
            return null;
        }

        // Load contacts for this customer
        var contacts = await contactService.GetAllContactsAsync(id);
        // Note: In a real implementation, you'd modify the CustomerDto to include contacts
        // For now, this is just a placeholder showing the pattern

        return customer;
    }

    [Authorize]
    [UseProjection]
    public async Task<CustomerDto?> GetCustomerWithDetails(
        int id,
        [Service] ICustomerService customerService,
        [Service] ICustomerCategoryService categoryService,
        [Service] IActivityLogService activityLogService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        var customer = await customerService.GetCustomerByIdAsync(id);

        if (customer == null || customer.CompanyId != companyId)
        {
            return null;
        }

        // Load categories
        customer.Categories = (await categoryService.GetCustomerCategoriesAsync(id, userId)).ToList();

        // Load activity summary
        var activities = await activityLogService.GetCustomerActivitiesAsync(id, userId);
        customer.TotalActivities = activities.Count();
        customer.LastActivity = activities.Any() ? activities.Max(a => a.CreatedAt) : null;

        return customer;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class CustomerMutation : BaseResolver
{
    [Authorize]
    public async Task<CustomerDto> CreateCustomer(
        CreateCustomerDto input,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateCustomerDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            // Validate input
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // Override the companyId in the input to ensure it's the user's company
            var modifiedInput = new CreateCustomerDto
            {
                Name = input.Name,
                Email = input.Email,
                Phone = input.Phone,
                Address = input.Address,
                City = input.City,
                Country = input.Country,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                CompanyId = companyId
            };

            return await customerService.CreateCustomerAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create customer");
        }
    }

    [Authorize]
    public async Task<CustomerDto?> UpdateCustomer(
        int id,
        UpdateCustomerDto input,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateCustomerDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            // Validate input
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // First check if the customer exists and belongs to the user's company
            var existingCustomer = await customerService.GetCustomerByIdAsync(id);
            if (existingCustomer == null || existingCustomer.CompanyId != companyId)
            {
                throw new GraphQLException("Customer not found or access denied");
            }

            return await customerService.UpdateCustomerAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update customer");
        }
    }

    [Authorize]
    public async Task<bool> DeleteCustomer(
        int id,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // First check if the customer exists and belongs to the user's company
            var existingCustomer = await customerService.GetCustomerByIdAsync(id);
            if (existingCustomer == null || existingCustomer.CompanyId != companyId)
            {
                throw new GraphQLException("Customer not found or access denied");
            }

            return await customerService.DeleteCustomerAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete customer");
        }
    }

    [Authorize]
    public async Task<CustomerDto?> UpdateCustomerStatus(
        int id,
        string status,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateCustomerDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // First check if the customer exists and belongs to user's company
            var existingCustomer = await customerService.GetCustomerByIdAsync(id);
            if (existingCustomer == null || existingCustomer.CompanyId != companyId)
            {
                throw new GraphQLException("Customer not found or access denied");
            }

            var updateDto = new UpdateCustomerDto
            {
                Status = status
            };

            // Validate the update DTO
            var validationResult = await validator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            return await customerService.UpdateCustomerAsync(id, updateDto);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update customer status");
        }
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class ContactResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ContactDto>> GetContactsByCustomer(
        int customerId,
        [Service] IContactService contactService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            // If no active company, return empty list
            return new List<ContactDto>();
        }

        // Verify customer belongs to user's company
        var customer = await customerService.GetCustomerByIdAsync(customerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return new List<ContactDto>();
        }

        return await contactService.GetAllContactsAsync(customerId);
    }

    [Authorize]
    public async Task<ContactDto?> GetContact(
        int id,
        [Service] IContactService contactService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            throw new GraphQLException("User has no active company");
        }

        var contact = await contactService.GetContactByIdAsync(id);
        if (contact == null)
        {
            return null;
        }

        // Verify the contact's customer belongs to user's company
        var customer = await customerService.GetCustomerByIdAsync(contact.CustomerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return null; // Return null instead of throwing error for security
        }

        return contact;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class ContactMutation
{
    [Authorize]
    public async Task<ContactDto> CreateContact(
        CreateContactDto input,
        [Service] IContactService contactService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // Verify customer belongs to user's company
            var customer = await customerService.GetCustomerByIdAsync(input.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Customer not found or access denied");
            }

            return await contactService.CreateContactAsync(input);
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to create contact: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<ContactDto?> UpdateContact(
        int id,
        UpdateContactDto input,
        [Service] IContactService contactService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // First check if the contact exists and belongs to user's company
            var existingContact = await contactService.GetContactByIdAsync(id);
            if (existingContact == null)
            {
                throw new GraphQLException("Contact not found");
            }

            var customer = await customerService.GetCustomerByIdAsync(existingContact.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Contact not found or access denied");
            }

            return await contactService.UpdateContactAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to update contact: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<bool> DeleteContact(
        int id,
        [Service] IContactService contactService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // First check if the contact exists and belongs to user's company
            var existingContact = await contactService.GetContactByIdAsync(id);
            if (existingContact == null)
            {
                throw new GraphQLException("Contact not found");
            }

            var customer = await customerService.GetCustomerByIdAsync(existingContact.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Contact not found or access denied");
            }

            return await contactService.DeleteContactAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to delete contact: {ex.Message}");
        }
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class TicketResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<TicketDto>> GetTickets(
        [Service] ITicketService ticketService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            // If no active company, return empty list
            return new List<TicketDto>();
        }

        return await ticketService.GetAllTicketsAsync(companyId.Value);
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<TicketDto>> GetTicketsByCustomer(
        int customerId,
        [Service] ITicketService ticketService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            // If no active company, return empty list
            return new List<TicketDto>();
        }

        // Verify customer belongs to user's company
        var customer = await customerService.GetCustomerByIdAsync(customerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return new List<TicketDto>();
        }

        return await ticketService.GetTicketsByCustomerAsync(customerId);
    }

    [Authorize]
    [UseProjection]
    public async Task<TicketDto?> GetTicket(
        int id,
        [Service] ITicketService ticketService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            throw new GraphQLException("User has no active company");
        }

        var ticket = await ticketService.GetTicketByIdAsync(id);

        // Check if ticket's customer belongs to user's company
        if (ticket == null)
        {
            return null;
        }

        // Verify the ticket's customer belongs to the user's company
        var customer = await context.Customers.FindAsync(ticket.CustomerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return null; // Return null instead of throwing error for security
        }

        return ticket;
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<TicketDto>> GetMyAssignedTickets(
        [Service] ITicketService ticketService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        return await ticketService.GetTicketsByAssignedUserAsync(userId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class TicketMutation
{
    [Authorize]
    public async Task<TicketDto> CreateTicket(
        CreateTicketDto input,
        [Service] ITicketService ticketService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // Verify customer belongs to user's company
            var customer = await customerService.GetCustomerByIdAsync(input.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Customer not found or access denied");
            }

            return await ticketService.CreateTicketAsync(input);
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to create ticket: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<TicketDto?> UpdateTicket(
        int id,
        UpdateTicketDto input,
        [Service] ITicketService ticketService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // First check if the ticket exists and belongs to user's company
            var existingTicket = await ticketService.GetTicketByIdAsync(id);
            if (existingTicket == null)
            {
                throw new GraphQLException("Ticket not found");
            }

            var customer = await context.Customers.FindAsync(existingTicket.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Ticket not found or access denied");
            }

            return await ticketService.UpdateTicketAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to update ticket: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<bool> DeleteTicket(
        int id,
        [Service] ITicketService ticketService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // First check if the ticket exists and belongs to user's company
            var existingTicket = await ticketService.GetTicketByIdAsync(id);
            if (existingTicket == null)
            {
                throw new GraphQLException("Ticket not found");
            }

            var customer = await context.Customers.FindAsync(existingTicket.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Ticket not found or access denied");
            }

            return await ticketService.DeleteTicketAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to delete ticket: {ex.Message}");
        }
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class CustomerNoteResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<CustomerNoteDto>> GetCustomerNotes(
        int customerId,
        [Service] ICustomerNoteService noteService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            // If no active company, return empty list
            return new List<CustomerNoteDto>();
        }

        // Verify customer belongs to user's company
        var customer = await customerService.GetCustomerByIdAsync(customerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return new List<CustomerNoteDto>();
        }

        return await noteService.GetAllNotesAsync(customerId, userId);
    }

    [Authorize]
    [UseProjection]
    public async Task<CustomerNoteDto?> GetCustomerNote(
        int id,
        [Service] ICustomerNoteService noteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        return await noteService.GetNoteByIdAsync(id, userId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class CustomerNoteMutation
{
    [Authorize]
    public async Task<CustomerNoteDto> CreateCustomerNote(
        CreateCustomerNoteDto input,
        [Service] ICustomerNoteService noteService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new GraphQLException("User not found");
            }

            // Get the active company ID for the user
            int? companyId = user.CompanyId;
            if (!companyId.HasValue)
            {
                throw new GraphQLException("User has no active company");
            }

            // Verify customer belongs to user's company
            var customer = await customerService.GetCustomerByIdAsync(input.CustomerId);
            if (customer == null || customer.CompanyId != companyId.Value)
            {
                throw new GraphQLException("Customer not found or access denied");
            }

            return await noteService.CreateNoteAsync(input, userId);
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to create customer note: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<CustomerNoteDto?> UpdateCustomerNote(
        int id,
        UpdateCustomerNoteDto input,
        [Service] ICustomerNoteService noteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            return await noteService.UpdateNoteAsync(id, input, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to update customer note: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<bool> DeleteCustomerNote(
        int id,
        [Service] ICustomerNoteService noteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
            {
                throw new GraphQLException("User not authenticated");
            }

            var claimsPrincipal = httpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new GraphQLException("Invalid user token");
            }

            return await noteService.DeleteNoteAsync(id, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to delete customer note: {ex.Message}");
        }
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class FileAttachmentResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<FileAttachmentDto>> GetFileAttachments(
        int customerId,
        [Service] IFileUploadService fileUploadService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            // If no active company, return empty list
            return new List<FileAttachmentDto>();
        }

        // Verify customer belongs to user's company
        var customer = await customerService.GetCustomerByIdAsync(customerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return new List<FileAttachmentDto>();
        }

        return await fileUploadService.GetFilesByCustomerAsync(customerId, userId);
    }

    [Authorize]
    [UseProjection]
    public async Task<FileAttachmentDto?> GetFileAttachment(
        int id,
        [Service] IFileUploadService fileUploadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        return await fileUploadService.GetFileAsync(id, userId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class FileAttachmentMutation
{
    [Authorize]
    public async Task<UploadResultDto> UploadFile(
        UploadFileDto input,
        [Service] IFileUploadService fileUploadService,
        [Service] ICustomerService customerService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get the active company ID for the user
        int? companyId = user.CompanyId;
        if (!companyId.HasValue)
        {
            return new UploadResultDto
            {
                Success = false,
                Message = "User has no active company"
            };
        }

        // Verify customer belongs to user's company
        var customer = await customerService.GetCustomerByIdAsync(input.CustomerId);
        if (customer == null || customer.CompanyId != companyId.Value)
        {
            return new UploadResultDto
            {
                Success = false,
                Message = "Customer not found or access denied"
            };
        }

        return await fileUploadService.UploadFileAsync(input, userId);
    }

    [Authorize]
    public async Task<bool> DeleteFileAttachment(
        int id,
        [Service] IFileUploadService fileUploadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        return await fileUploadService.DeleteFileAsync(id, userId);
    }
}
