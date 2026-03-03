using Application.Common.Exceptions;
using System.Text.Json;

namespace API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning("Not found: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status404NotFound,
                new { error = ex.Message });
        }
        catch (AlreadyExistsException ex)
        {
            logger.LogWarning("Conflict: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status409Conflict,
                new { error = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning("Business rule violation: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status400BadRequest,
                new { error = ex.Message });
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning("Concurrency conflict: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status409Conflict,
                new
                {
                    error          = ex.Message,
                    clientValues   = ex.ClientValues,
                    databaseValues = ex.DatabaseValues
                });
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation errors: {@Errors}", ex.Errors);
            await WriteResponseAsync(context, StatusCodes.Status422UnprocessableEntity,
                new
                {
                    error  = ex.Message,
                    errors = ex.Errors
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        object body)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}