using Application.Features.Cors.Models;
using Application.Pipelines;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Commands;

public class CreateCorsOriginCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public CreateCorsOriginRequest CreateCors { get; init; } = null!;

    public class Handler : IRequestHandler<CreateCorsOriginCommand, IResponseWrapper>
    {
        private readonly ICorsOriginService _service;

        public Handler(ICorsOriginService service)
        {
            _service = service;
        }

        public async Task<IResponseWrapper> Handle(CreateCorsOriginCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request.CreateCors);
            return await ResponseWrapper<CorsOriginResponse>.SuccessAsync(result);
        }
    }
}