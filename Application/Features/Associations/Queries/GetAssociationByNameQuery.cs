using Application.Wrappers;
using Mapster;
using MediatR;

namespace Application.Features.Associations.Queries;

public class GetAssociationByNameQuery : IRequest<IResponseWrapper>
{
    public required string Name { get; set; }
}

public class GetAssociationByNameQueryHandler(IAssociationService associationService) : IRequestHandler<GetAssociationByNameQuery, IResponseWrapper>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<IResponseWrapper> Handle(GetAssociationByNameQuery request, CancellationToken cancellationToken)
    {
        var association = await _associationService.GetByNameAsync(request.Name);

        if (association is null)
        {
            return await ResponseWrapper<string>.FailAsync(message: "Association not found.");
        }

        return await ResponseWrapper<AssociationResponse>.SuccessAsync(data: association.Adapt<AssociationResponse>());
    }
}
