using HotChocolate;
using FluentValidation.Results;

namespace crm_backend.Services;

/// <summary>
/// Service for standardized error handling across the application
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// Handles exceptions and converts them to GraphQL exceptions with appropriate messages
    /// </summary>
    GraphQLException HandleException(Exception ex, string? context = null);
    
    /// <summary>
    /// Creates a validation error from FluentValidation results
    /// </summary>
    string CreateValidationError(ValidationResult validationResult);
    
    /// <summary>
    /// Creates a standardized error message
    /// </summary>
    string CreateError(string errorCode, string message);
}

public class ErrorHandlingService : IErrorHandlingService
{
    public GraphQLException HandleException(Exception ex, string? context = null)
    {
        // Handle specific exception types
        return ex switch
        {
            UnauthorizedAccessException => new GraphQLException(ex.Message),
            InvalidOperationException => new GraphQLException(ex.Message),
            ArgumentException => new GraphQLException(ex.Message),
            GraphQLException => ex as GraphQLException ?? new GraphQLException(ex.Message),
            _ => new GraphQLException(context != null 
                ? $"{context}: {ex.Message}" 
                : $"An error occurred: {ex.Message}")
        };
    }
    
    public string CreateValidationError(ValidationResult validationResult)
    {
        var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
        return $"Validation failed: {errors}";
    }
    
    public string CreateError(string errorCode, string message)
    {
        return $"[{errorCode}] {message}";
    }
}

