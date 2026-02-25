using Application.Pipelines;
using BabaPlayShared.Library.Models.Requests.Associations;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Associations.Commands;

public class UpdateAssociationCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required UpdateAssociationRequest UpdateAssociation { get; set; }
}

public class UpdateAssociationCommandHandler(IAssociationService associationService) : IRequestHandler<UpdateAssociationCommand, IResponseWrapper>
{
    private readonly IAssociationService _associationService = associationService;

    public async Task<IResponseWrapper> Handle(UpdateAssociationCommand request, CancellationToken cancellationToken)
    {
        var associationInDb = await _associationService.GetByIdAsync(request.UpdateAssociation.Id);

        if (associationInDb is null)
        {
            return await ResponseWrapper<string>.FailAsync(message: "Association not found.");
        }

        associationInDb.Name = request.UpdateAssociation.Name;
        associationInDb.EstablishedDate = request.UpdateAssociation.EstablishedDate;

        var updatedAssociation = await _associationService.UpdateAsync(associationInDb);

        return await ResponseWrapper<string>.SuccessAsync(data: updatedAssociation, message: "Association updated successfully.");
    }
}