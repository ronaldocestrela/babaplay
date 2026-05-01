using BabaPlay.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BabaPlay.Api.Middlewares;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, code, message) = exception switch
        {
            NotFoundException ex   => (StatusCodes.Status404NotFound,  ex.Code, ex.Message),
            ValidationException ex => (StatusCodes.Status422UnprocessableEntity, ex.Code, ex.Message),
            DomainException ex     => (StatusCodes.Status400BadRequest, ex.Code, ex.Message),
            _                      => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        _logger.LogError(exception, "Handled exception [{Code}]: {Message}", code, message);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = code,
            Detail = message
        };

        var json = JsonSerializer.Serialize(problem);
        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }
}
