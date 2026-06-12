using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PetHaven.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleProblemDetailsAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleProblemDetailsAsync(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await HandleProblemDetailsAsync(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.");
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ValidationProblemDetails(
            exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(e => e.ErrorMessage).ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation error",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}