using Application.Features.Associations;
using Application.Features.Associations.Commands;
using Application.Features.Associations.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class AssociationsController : BaseApiController
{
    [HttpPost("add")]
    [ShouldHavePermission(AssociationAction.Create, AssociationFeature.Associations)]
    public async Task<IActionResult> CreateAssociationAsync([FromBody] CreateAssociationRequest createAssociation)
    {
        var response = await Sender.Send(new CreateAssociationCommand { CreateAssociation = createAssociation });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("update")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Associations)]
    public async Task<IActionResult> UpdateAssociationAsync([FromBody] UpdateAssociationRequest updateAssociation)
    {
        var response = await Sender.Send(new UpdateAssociationCommand { UpdateAssociation = updateAssociation });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpDelete("{AssociationId}")]
    [ShouldHavePermission(AssociationAction.Delete, AssociationFeature.Associations)]
    public async Task<IActionResult> DeleteAssociationAsync(string AssociationId)
    {
        var response = await Sender.Send(new DeleteAssociationCommand { AssociationId = AssociationId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("by-id/{AssociationId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Associations)]
    public async Task<IActionResult> GetAssociationByIdAsync(string AssociationId)
    {
        var response = await Sender.Send(new GetAssociationByIdQuery { AssociationId = AssociationId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("by-name/{name}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Associations)]
    public async Task<IActionResult> GetAssociationByNameAsync(string name)
    {
        var response = await Sender.Send(new GetAssociationByNameQuery { Name = name });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("all")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Associations)]
    public async Task<IActionResult> GetAllAssociationsAsync()
    {
        var response = await Sender.Send(new GetAssociationsQuery());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }
}
