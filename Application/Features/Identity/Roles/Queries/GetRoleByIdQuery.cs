using BabaPlayShared.Library.Models.Responses.Identity;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Queries;

public class GetRoleByIdQuery : IRequest<IResponseWrapper>
{
    public required string RoleId { get; set; }
}

public class GetRoleByIdQueryHandler(IRoleService roleService) : IRequestHandler<GetRoleByIdQuery, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdAsync(request.RoleId, cancellationToken);
        return await ResponseWrapper<RoleResponse>.SuccessAsync(role);
    }
}
