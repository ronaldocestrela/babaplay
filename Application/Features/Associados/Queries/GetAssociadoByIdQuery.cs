using BabaPlayShared.Library.Models.Responses.Associados;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Associados.Queries;

public class GetAssociadoByIdQuery : IRequest<IResponseWrapper>
{
    public required string AssociadoId { get; set; }
}

public class GetAssociadoByIdQueryHandler(IAssociadoService associadoService) : IRequestHandler<GetAssociadoByIdQuery, IResponseWrapper>
{
    private readonly IAssociadoService _associadoService = associadoService;

    public async Task<IResponseWrapper> Handle(GetAssociadoByIdQuery request, CancellationToken cancellationToken)
    {
        var associado = await _associadoService.GetByIdAsync(request.AssociadoId);

        if (associado is null)
        {
            return await ResponseWrapper<string>.FailAsync(message: "Associado não encontrado.");
        }

        return await ResponseWrapper<AssociadoResponse>.SuccessAsync(data: associado);
    }
}
