using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using BabaPlay.Web.Authorization;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web.Authorization;

public class ClientPermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenTenantOwnerAndCommunicationWrite_ShouldSucceed()
    {
        var tenantState = new TenantState();
        tenantState.SetTenant(Guid.NewGuid(), "Baba FC", "baba-fc", isOwner: true);

        var handler = new ClientPermissionAuthorizationHandler(tenantState);
        var requirement = new ClientPermissionRequirement(ClientAuthorizationPolicies.CommunicationWrite);
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity("test")),
            null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenNotOwnerAndCommunicationWrite_ShouldFail()
    {
        var tenantState = new TenantState();
        tenantState.SetTenant(Guid.NewGuid(), "Baba FC", "baba-fc", isOwner: false);

        var handler = new ClientPermissionAuthorizationHandler(tenantState);
        var requirement = new ClientPermissionRequirement(ClientAuthorizationPolicies.CommunicationWrite);
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity("test")),
            null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
