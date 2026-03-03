using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Cors.Commands;

public class DeleteCorsOriginCommand : IRequest<IResponseWrapper>
{
    public string Id { get; init; } = null!;

    public class Handler(ICorsOriginService service) : IRequestHandler<DeleteCorsOriginCommand, IResponseWrapper>
    {
        private readonly ICorsOriginService _service = service;

        public async Task<IResponseWrapper> Handle(DeleteCorsOriginCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _service.DeleteAsync(request.Id);
            if (!deleted)
                return await ResponseWrapper.FailAsync("Not found");

            return await ResponseWrapper.SuccessAsync("Deleted");
        }
    }
}