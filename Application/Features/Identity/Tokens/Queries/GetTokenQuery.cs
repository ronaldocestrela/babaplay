using BabaPlayShared.Library.Models.Requests.Token;
using BabaPlayShared.Library.Models.Responses.Token;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Tokens.Queries;

public class GetTokenQuery : IRequest<IResponseWrapper>
{
    public required TokenRequest TokenRequest { get; set; }
}

public class GetTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetTokenQuery, IResponseWrapper>
{
    private readonly ITokenService _tokenService = tokenService;

    public async Task<IResponseWrapper> Handle(GetTokenQuery request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.LoginAsync(request.TokenRequest);

        return await ResponseWrapper<TokenResponse>.SuccessAsync(data: token);
    }
}
