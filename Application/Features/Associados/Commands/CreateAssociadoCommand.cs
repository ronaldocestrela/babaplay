using Application.Pipelines;
using BabaPlayShared.Library.Models.Requests.Associados;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Associados.Commands;

public class CreateAssociadoCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required CreateAssociadoRequest CreateAssociado { get; set; }
}

public class CreateAssociadoCommandHandler(IAssociadoService associadoService) : IRequestHandler<CreateAssociadoCommand, IResponseWrapper>
{
    private readonly IAssociadoService _associadoService = associadoService;

    public async Task<IResponseWrapper> Handle(CreateAssociadoCommand request, CancellationToken cancellationToken)
    {
        var associadoId = await _associadoService.CreateAsync(request.CreateAssociado);

        return await ResponseWrapper<string>.SuccessAsync(data: associadoId, message: "Associado criado com sucesso.");
    }
}
