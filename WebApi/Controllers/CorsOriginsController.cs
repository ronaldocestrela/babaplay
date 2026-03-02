using Application.Features.Cors;
using Application.Features.Cors.Commands;
using Application.Features.Cors.Queries;
using Application.Features.Cors.Models;
using Application.Features.Cors.Constants;
using BabaPlayShared.Library.Constants;
using BabaPlayShared.Library.Wrappers;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

// feature-based permission attributes applied per endpoint
public class CorsOriginsController : BaseApiController
{
    [HttpPost]
    [ShouldHavePermission(AssociationAction.Create, CorsFeature.CorsOrigins)]
    public async Task<IActionResult> Create([FromBody] CreateCorsOriginRequest request)
    {
        var response = await Sender.Send(new CreateCorsOriginCommand { CreateCors = request });
        if (response.IsSuccessful)
            return Ok(response);
        return BadRequest(response);
    }

    [HttpGet]
    [ShouldHavePermission(AssociationAction.Read, CorsFeature.CorsOrigins)]
    public async Task<IActionResult> List()
    {
        var response = await Sender.Send(new GetAllCorsOriginsQuery());
        if (response.IsSuccessful)
            return Ok(response);
        return BadRequest(response);
    }

    [HttpGet("{id}")]
    [ShouldHavePermission(AssociationAction.Read, CorsFeature.CorsOrigins)]
    public async Task<IActionResult> GetById(string id)
    {
        var response = await Sender.Send(new GetCorsOriginByIdQuery { Id = id });
        if (response.IsSuccessful)
            return Ok(response);
        return NotFound(response);
    }

    [HttpPut("{id}")]
    [ShouldHavePermission(AssociationAction.Update, CorsFeature.CorsOrigins)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCorsOriginRequest request)
    {
        var response = await Sender.Send(new UpdateCorsOriginCommand { Id = id, UpdateCors = request });
        if (response.IsSuccessful)
            return Ok(response);
        return BadRequest(response);
    }

    [HttpDelete("{id}")]
    [ShouldHavePermission(AssociationAction.Delete, CorsFeature.CorsOrigins)]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await Sender.Send(new DeleteCorsOriginCommand { Id = id });
        if (response.IsSuccessful)
            return Ok(response);
        return BadRequest(response);
    }
}
