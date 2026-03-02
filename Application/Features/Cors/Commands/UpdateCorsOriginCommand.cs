using Application.Features.Cors.Models;
using Application.Pipelines;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Commands;

public class UpdateCorsOriginCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public string Id { get; init; } = null!;
    public UpdateCorsOriginRequest UpdateCors { get; init; } = null!;

    public class Handler : IRequestHandler<UpdateCorsOriginCommand, IResponseWrapper>
    {
        private readonly ICorsOriginService _service;

        public Handler(ICorsOriginService service)
        {
            _service = service;
        }

        public async Task<IResponseWrapper> Handle(UpdateCorsOriginCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(request.Id, request.UpdateCors);
            return await ResponseWrapper<CorsOriginResponse>.SuccessAsync(result);
        }
    }
}