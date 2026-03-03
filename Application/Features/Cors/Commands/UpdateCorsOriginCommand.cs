using Application.Pipelines;
using BabaPlayShared.Library.Models.Requests.Cors;
using BabaPlayShared.Library.Models.Responses.Cors;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Commands;

public class UpdateCorsOriginCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public string Id { get; init; } = null!;
    public UpdateCorsOriginRequest UpdateCors { get; init; } = null!;

    public class Handler(ICorsOriginService service) : IRequestHandler<UpdateCorsOriginCommand, IResponseWrapper>
    {
        private readonly ICorsOriginService _service = service;

        public async Task<IResponseWrapper> Handle(UpdateCorsOriginCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(request.Id, request.UpdateCors);
            return await ResponseWrapper<CorsOriginResponse>.SuccessAsync(result);
        }
    }
}