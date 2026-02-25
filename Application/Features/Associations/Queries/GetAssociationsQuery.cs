using BabaPlayShared.Library.Models.Responses.Associations;
using BabaPlayShared.Library.Wrappers;
using Mapster;
using MediatR;

namespace Application.Features.Associations.Queries;

public class GetAssociationsQuery : IRequest<IResponseWrapper>
{
}

public class GetAssociationsQueryHandler(IAssociationService associationService) : IRequestHandler<GetAssociationsQuery, IResponseWrapper>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<IResponseWrapper> Handle(GetAssociationsQuery request, CancellationToken cancellationToken)
    {
        var associations = await _associationService.GetAllAsync();
        if (associations?.Count > 0)
        {
            return await ResponseWrapper<List<AssociationResponse>>
                .SuccessAsync(data: associations.Adapt<List<AssociationResponse>>());
        }

        return await ResponseWrapper<string>.FailAsync(message: "No associations found.");
    }
}
