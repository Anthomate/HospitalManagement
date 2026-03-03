using Application.Common;
using System.Text.Json;

namespace API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning("Concurrency conflict: {Message}", ex.Message);

            context.Response.StatusCode  = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error          = ex.Message,
                clientValues   = ex.ClientValues,
                databaseValues = ex.DatabaseValues
            }));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Business rule violation: {Message}", ex.Message);

            context.Response.StatusCode  = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = ex.Message
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred."
            }));
        }
    }
}