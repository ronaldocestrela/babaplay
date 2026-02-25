using BabaPlayShared.Library.Models.Responses.Tenency;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Queries;

public class GetTenantByIdQuery : IRequest<IResponseWrapper>
{
    public required string TenantId { get; set; }
}

public class GetTenantByIdQueryHandler(ITenantService tenantService) : IRequestHandler<GetTenantByIdQuery, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(request.TenantId);

        if (tenant is null)
        {
            return await ResponseWrapper<TenantResponse>.FailAsync(message: "Tenant not found.");
        }

        return await ResponseWrapper<TenantResponse>.SuccessAsync(data: tenant, message: "Tenant retrieved successfully.");
    }
}