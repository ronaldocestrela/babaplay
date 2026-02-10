using Application.Features.Associados;
using Application.Features.Associados.Commands;
using Application.Features.Associados.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class AssociadosController : BaseApiController
{
    [HttpPost("add")]
    [ShouldHavePermission(AssociationAction.Create, AssociationFeature.Associados)]
    public async Task<IActionResult> CreateAssociadoAsync([FromBody] CreateAssociadoRequest createAssociado)
    {
        var response = await Sender.Send(new CreateAssociadoCommand { CreateAssociado = createAssociado });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("update/{associadoId}")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Associados)]
    public async Task<IActionResult> UpdateAssociadoAsync([FromBody] UpdateAssociadoRequest updateAssociado, string associadoId)
    {
        var response = await Sender.Send(new UpdateAssociadoCommand { AssociadoId = associadoId, UpdateAssociado = updateAssociado });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpDelete("{associadoId}")]
    [ShouldHavePermission(AssociationAction.Delete, AssociationFeature.Associados)]
    public async Task<IActionResult> DeleteAssociadoAsync(string associadoId)
    {
        var response = await Sender.Send(new DeleteAssociadoCommand { AssociadoId = associadoId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("{associadoId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Associados)]
    public async Task<IActionResult> GetAssociadoByIdAsync(string associadoId)
    {
        var response = await Sender.Send(new GetAssociadoByIdQuery { AssociadoId = associadoId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("all")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Associados)]
    public async Task<IActionResult> GetAllAssociadosAsync()
    {
        var response = await Sender.Send(new GetAllAssociadosQuery());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }
}
