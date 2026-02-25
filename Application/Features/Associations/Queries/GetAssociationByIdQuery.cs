using BabaPlayShared.Library.Models.Responses.Associations;
using BabaPlayShared.Library.Wrappers;
using Mapster;
using MediatR;

namespace Application.Features.Associations.Queries;

public class GetAssociationByIdQuery : IRequest<IResponseWrapper>
{
    public required string AssociationId { get; set; }
}

public class GetAssociationByIdQueryHandler(IAssociationService associationService) 
    : IRequestHandler<GetAssociationByIdQuery, IResponseWrapper>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<IResponseWrapper> Handle(GetAssociationByIdQuery request, CancellationToken cancellationToken)
    {
        var associationInDb = await _associationService.GetByIdAsync(request.AssociationId);

        if (associationInDb is null)
        {
            return await ResponseWrapper<string>.FailAsync(message: "Association not found.");
        }

        return await ResponseWrapper<AssociationResponse>.SuccessAsync(data: associationInDb.Adapt<AssociationResponse>());
    }
}
