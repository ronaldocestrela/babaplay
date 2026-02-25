using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Commands;

public class CreateRoleCommand : IRequest<IResponseWrapper>
{
    public required CreateRoleRequest CreateRole { get; set; }
}

public class CreateRoleCommandHandler(IRoleService roleService) : IRequestHandler<CreateRoleCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = await _roleService.CreateAsync(request.CreateRole);

        return await ResponseWrapper<string>.SuccessAsync(message: $"Role '{roleName}' created successfully.");
    }
}
