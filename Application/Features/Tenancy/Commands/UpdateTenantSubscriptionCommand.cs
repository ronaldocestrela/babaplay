using BabaPlayShared.Library.Models.Requests.Tenancy;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class UpdateTenantSubscriptionCommand : IRequest<IResponseWrapper>
{
    public required UpdateTenantSubscriptionRequest UpdateTenantSubscription { get; set; }
}

public class UpdateTenantSubscriptionCommandHandler(ITenantService tenantService) : IRequestHandler<UpdateTenantSubscriptionCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(UpdateTenantSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var result = await _tenantService.UpdateSubscriptionAsync(request.UpdateTenantSubscription);

        return await ResponseWrapper<string>.SuccessAsync(data: result, message: "Tenant subscription updated successfully.");
    }
}
