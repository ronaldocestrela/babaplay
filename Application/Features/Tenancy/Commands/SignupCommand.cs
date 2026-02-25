using BabaPlayShared.Library.Wrappers;
using MediatR;
using Application.Pipelines;
using BabaPlayShared.Library.Models.Requests.Tenancy;

namespace Application.Features.Tenancy.Commands;

public class SignupCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required SignupRequest SignupRequest { get; set; }
}

public class SignupCommandHandler(ITenantService tenantService) : IRequestHandler<SignupCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var tenantIdentifier = await _tenantService.SignupAsync(request.SignupRequest, cancellationToken);
        return await ResponseWrapper<string>.SuccessAsync(data: tenantIdentifier, message: "Signup successful. Tenant created.");
    }
}
