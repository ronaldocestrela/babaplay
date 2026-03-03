using Application.Pipelines;
using BabaPlayShared.Library.Models.Requests.Cors;
using BabaPlayShared.Library.Models.Responses.Cors;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Commands;

public class CreateCorsOriginCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public CreateCorsOriginRequest CreateCors { get; init; } = null!;

    public class Handler(ICorsOriginService service) : IRequestHandler<CreateCorsOriginCommand, IResponseWrapper>
    {
        private readonly ICorsOriginService _service = service;

        public async Task<IResponseWrapper> Handle(CreateCorsOriginCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request.CreateCors);
            return await ResponseWrapper<CorsOriginResponse>.SuccessAsync(result);
        }
    }
}