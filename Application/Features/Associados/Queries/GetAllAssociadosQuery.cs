using BabaPlayShared.Library.Models.Responses.Associados;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Associados.Queries;

public class GetAllAssociadosQuery : IRequest<IResponseWrapper>
{
}

public class GetAllAssociadosQueryHandler(IAssociadoService associadoService) : IRequestHandler<GetAllAssociadosQuery, IResponseWrapper>
{
    private readonly IAssociadoService _associadoService = associadoService;

    public async Task<IResponseWrapper> Handle(GetAllAssociadosQuery request, CancellationToken cancellationToken)
    {
        var associados = await _associadoService.GetAllAsync();

        if (associados?.Count > 0)
        {
            return await ResponseWrapper<List<AssociadoResponse>>.SuccessAsync(data: associados);
        }

        return await ResponseWrapper<string>.FailAsync(message: "Nenhum associado encontrado.");
    }
}
