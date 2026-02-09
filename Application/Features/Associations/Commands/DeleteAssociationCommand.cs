using Application.Wrappers;
using MediatR;

namespace Application.Features.Associations.Commands;

public class DeleteAssociationCommand : IRequest<IResponseWrapper>
{
    public required string AssociationId { get; set; }
}

public class DeleteAssociationCommandHandler(IAssociationService associationService) : IRequestHandler<DeleteAssociationCommand, IResponseWrapper>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<IResponseWrapper> Handle(DeleteAssociationCommand request, CancellationToken cancellationToken)
    {
        var associationInDb = await _associationService.GetByIdAsync(request.AssociationId);

        if (associationInDb is not null)
        {
            var deletedAssociationId = await _associationService.DeleteAsync(associationInDb);

            return await ResponseWrapper<string>.SuccessAsync(data: deletedAssociationId, message: "Association deleted successfully.");
        }

        return await ResponseWrapper<string>.FailAsync(message: "Association not found.");
    }
}
