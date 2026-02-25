using Application.Exceptions;
using BabaPlayShared.Library.Wrappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace WebApi;

public class ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly IWebHostEnvironment _env = env;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // log full exception server-side
            _logger.LogError(ex, "Unhandled exception caught by middleware");

            var response = context.Response;
            response.ContentType = "application/json";

            var responseWrapper = ResponseWrapper.Fail();

            switch (ex)
            {
                case ConflictException ce:
                    response.StatusCode = (int)ce.StatusCode;
                    responseWrapper.Messages = ce.ErrorMessages;
                    break;
                case NotFoundException nfe:
                    response.StatusCode = (int)nfe.StatusCode;
                    responseWrapper.Messages = nfe.ErrorMessages;
                    break;
                case ForbiddenException fe:
                    response.StatusCode = (int)fe.StatusCode;
                    responseWrapper.Messages = fe.ErrorMessages;
                    break;
                case IdentityException ie:
                    response.StatusCode = (int)ie.StatusCode;
                    responseWrapper.Messages = ie.ErrorMessages;
                    break;
                case UnauthorizedException ue:
                    response.StatusCode = (int)ue.StatusCode;
                    responseWrapper.Messages = ue.ErrorMessages;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseWrapper.Messages = new List<string> { ex.Message };
                    // include full exception details in Development to help debugging
                    if (_env.IsDevelopment())
                    {
                        responseWrapper.Messages.Add(ex.ToString());
                    }
                    break;
            }

            var result = JsonSerializer.Serialize(responseWrapper);

            await response.WriteAsync(result);
        }
    }
}
