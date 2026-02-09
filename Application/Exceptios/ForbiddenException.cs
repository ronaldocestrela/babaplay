using System.Net;

namespace Application.Exceptios;

public class ForbiddenException(List<string> errorMessages = default, HttpStatusCode statusCode = HttpStatusCode.Forbidden) : Exception
{
    public List<string> ErrorMessages { get; set; } = errorMessages;
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
