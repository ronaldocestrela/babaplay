namespace Application.Features.Cors.Models;

public record UpdateCorsOriginRequest(string Origin, bool IsActive);