// CryptoFinData.API/Middleware/ExceptionHandlingMiddleware.cs

using System.Diagnostics;
using CryptoFinData.API.Models;
using CryptoFinData.Core.Exeptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized access";
                break;

            case ApiException apiException:
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                response.Message = "External API service error";
                response.Details = apiException.Message;
                break;

            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Validation error";
                response.Details = validationException.Message;
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Resource not found";
                response.Details = notFoundException.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An unexpected error occurred";
                if (_env.IsDevelopment())
                {
                    response.Details = exception.ToString();
                }
                break;
        }

        if (_env.IsDevelopment())
        {
            response.DevelopmentDetails = new DevelopmentErrorDetails
            {
                ExceptionType = exception.GetType().Name,
                StackTrace = exception.StackTrace,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                Timestamp = DateTime.UtcNow
            };
        }

        await context.Response.WriteAsJsonAsync(response);
    }
}
