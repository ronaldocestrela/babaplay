using Application.Features.Cors.Models;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Queries;

public class GetAllCorsOriginsQuery : IRequest<IResponseWrapper>
{
    public class Handler : IRequestHandler<GetAllCorsOriginsQuery, IResponseWrapper>
    {
        private readonly ICorsOriginService _service;

        public Handler(ICorsOriginService service)
        {
            _service = service;
        }

        public async Task<IResponseWrapper> Handle(GetAllCorsOriginsQuery request, CancellationToken cancellationToken)
        {
            var list = await _service.GetAllAsync();
            return await ResponseWrapper<List<CorsOriginResponse>>.SuccessAsync(list);
        }
    }
}