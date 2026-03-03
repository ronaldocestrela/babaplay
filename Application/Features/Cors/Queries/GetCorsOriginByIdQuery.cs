using BabaPlayShared.Library.Models.Responses.Cors;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Queries;

public class GetCorsOriginByIdQuery : IRequest<IResponseWrapper>
{
    public string Id { get; init; } = null!;

    public class Handler : IRequestHandler<GetCorsOriginByIdQuery, IResponseWrapper>
    {
        private readonly ICorsOriginService _service;

        public Handler(ICorsOriginService service)
        {
            _service = service;
        }

        public async Task<IResponseWrapper> Handle(GetCorsOriginByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _service.GetByIdAsync(request.Id);
            if (item == null)
                return await ResponseWrapper.FailAsync("Not found");

            return await ResponseWrapper<CorsOriginResponse>.SuccessAsync(item);
        }
    }
}