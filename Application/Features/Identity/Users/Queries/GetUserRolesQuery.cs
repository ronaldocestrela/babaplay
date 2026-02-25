using BabaPlayShared.Library.Models.Responses.Identity;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Users.Queries;

public class GetUserRolesQuery : IRequest<IResponseWrapper>
{
    public required string UserId { get; set; }
}

public class GetUserRolesQueryHandler(IUserService userService) : IRequestHandler<GetUserRolesQuery, IResponseWrapper>
{
    private readonly IUserService _userService = userService;

    public async Task<IResponseWrapper> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var userRoles = await _userService.GetUserRolesAsync(request.UserId, cancellationToken);
        return await ResponseWrapper<List<UserRoleResponse>>.SuccessAsync(data: userRoles);
    }
}