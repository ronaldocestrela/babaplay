using MediatR;

namespace Application.Features.Associations.Queries;

public class HasAssociationQuery : IRequest<bool>
{

}

public class HasAssociationQueryHandler(IAssociationService associationService) 
    : IRequestHandler<HasAssociationQuery, bool>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<bool> Handle(HasAssociationQuery request, CancellationToken cancellationToken)
    {
        return await _associationService.HasAnyAsync();
    }
}
