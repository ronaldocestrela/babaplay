using Application.Pipelines;
using Application.Wrappers;
using MediatR;

namespace Application.Features.Associados.Commands;

public class UpdateAssociadoCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required string AssociadoId { get; set; }
    public required UpdateAssociadoRequest UpdateAssociado { get; set; }
}

public class UpdateAssociadoCommandHandler(IAssociadoService associadoService) : IRequestHandler<UpdateAssociadoCommand, IResponseWrapper>
{
    private readonly IAssociadoService _associadoService = associadoService;

    public async Task<IResponseWrapper> Handle(UpdateAssociadoCommand request, CancellationToken cancellationToken)
    {
        var associadoId = await _associadoService.UpdateAsync(request.UpdateAssociado, request.AssociadoId);

        return await ResponseWrapper<string>.SuccessAsync(data: associadoId, message: "Associado atualizado com sucesso.");
    }
}
