using Application.Pipelines;
using Application.Wrappers;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Associations.Commands;

public class CreateAssociationCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required CreateAssociationRequest CreateAssociation { get; set; }
}

public class CreateAssociationCommandHandler(IAssociationService associationService) : IRequestHandler<CreateAssociationCommand, IResponseWrapper>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<IResponseWrapper> Handle(CreateAssociationCommand request, CancellationToken cancellationToken)
    {
        var newAssociation = request.CreateAssociation.Adapt<Association>();
        var associationId = await _associationService.CreateAsync(newAssociation);

        return await ResponseWrapper<string>.SuccessAsync(data: associationId, message: "Association created successfully.");
    }
}
