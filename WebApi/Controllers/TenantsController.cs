using Application.Features.Tenancy;
using Application.Features.Tenancy.Commands;
using Application.Features.Tenancy.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class TenantsController : BaseApiController
{
    [HttpPost("add")]
    [ShouldHavePermission(AssociationAction.Create, AssociationFeature.Tenants)]
    public async Task<IActionResult> CreateTenantAsync([FromBody] CreateTenantRequest createTenantRequest)
    {
        var response = await Sender.Send(new CreateTenantCommand { CreateTenant = createTenantRequest });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("{tenantId}/activate")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Tenants)]
    public async Task<IActionResult> ActivateTenantAsync(string tenantId)
    {
        var response = await Sender.Send(new ActivateTenantCommand { TenantId = tenantId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("{tenantId}/deactivate")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Tenants)]
    public async Task<IActionResult> DeactivateTenantAsync(string tenantId)
    {
        var response = await Sender.Send(new DeactivateTenantCommand { TenantId = tenantId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("upgrade")]
    [ShouldHavePermission(AssociationAction.UpgradeSubscription, AssociationFeature.Tenants)]
    public async Task<IActionResult> UpgradeTenantSubscriptionAsync([FromBody] UpdateTenantSubscriptionRequest updateTenant)
    {
        var response = await Sender.Send(new UpdateTenantSubscriptionCommand { UpdateTenantSubscription = updateTenant });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("{tenantId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Tenants)]
    public async Task<IActionResult> GetTenantByIdAsync(string tenantId)
    {
        var response = await Sender.Send(new GetTenantByIdQuery { TenantId = tenantId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("all")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Tenants)]
    public async Task<IActionResult> GetTenantsAsync()
    {
        var response = await Sender.Send(new GetTenantsQuery());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
