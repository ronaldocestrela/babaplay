using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class DeactivateTenantCommand : IRequest<IResponseWrapper>
{
    public required string TenantId { get; set; }
}

public class DeactivateTenantCommandHandler(ITenantService tenantService) : IRequestHandler<DeactivateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var result = await _tenantService.DeactivateAsync(request.TenantId);

        return await ResponseWrapper<string>.SuccessAsync(data: result, message: "Tenant deactivated successfully.");
    }
}
