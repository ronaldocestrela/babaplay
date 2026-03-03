using BabaPlayShared.Library.Models.Responses.Cors;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Queries;

public class GetAllCorsOriginsQuery : IRequest<IResponseWrapper>
{
    public class Handler(ICorsOriginService service) : IRequestHandler<GetAllCorsOriginsQuery, IResponseWrapper>
    {
        private readonly ICorsOriginService _service = service;

        public async Task<IResponseWrapper> Handle(GetAllCorsOriginsQuery request, CancellationToken cancellationToken)
        {
            var list = await _service.GetAllAsync();
            return await ResponseWrapper<List<CorsOriginResponse>>.SuccessAsync(list);
        }
    }
}