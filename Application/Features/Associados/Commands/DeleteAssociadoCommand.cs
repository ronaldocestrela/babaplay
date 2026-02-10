using Application.Wrappers;
using MediatR;

namespace Application.Features.Associados.Commands;

public class DeleteAssociadoCommand : IRequest<IResponseWrapper>
{
    public required string AssociadoId { get; set; }
}

public class DeleteAssociadoCommandHandler(IAssociadoService associadoService) : IRequestHandler<DeleteAssociadoCommand, IResponseWrapper>
{
    private readonly IAssociadoService _associadoService = associadoService;

    public async Task<IResponseWrapper> Handle(DeleteAssociadoCommand request, CancellationToken cancellationToken)
    {
        var deletedId = await _associadoService.DeleteAsync(request.AssociadoId);

        return await ResponseWrapper<string>.SuccessAsync(data: deletedId, message: "Associado removido com sucesso.");
    }
}
