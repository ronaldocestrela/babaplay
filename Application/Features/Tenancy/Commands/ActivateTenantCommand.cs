using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class ActivateTenantCommand : IRequest<IResponseWrapper>
{
    public required string TenantId { get; set; }
}

public class ActiveTenantCommandHandler(ITenantService tenantService) : IRequestHandler<ActivateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var result = await _tenantService.ActivateAsync(request.TenantId);

        return await ResponseWrapper<string>.SuccessAsync(data: result, message: "Tenant activated successfully.");
    }
}
