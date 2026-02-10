using System.Net;

namespace Application.Exceptions;

public class UnauthorizedException(List<string> errorMessages = default, HttpStatusCode statusCode = HttpStatusCode.Unauthorized) : Exception
{
    public List<string> ErrorMessages { get; set; } = errorMessages;
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
