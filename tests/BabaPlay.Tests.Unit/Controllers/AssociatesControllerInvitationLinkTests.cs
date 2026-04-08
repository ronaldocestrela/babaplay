using System.Security.Claims;
using BabaPlay.Modules.Associates.Controllers;
using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace BabaPlay.Tests.Unit.Controllers;

public sealed class AssociatesControllerInvitationLinkTests
{
    [Fact]
    public void BuildInvitationLink_IncludesTenantQueryParam()
    {
        var url = AssociatesController.BuildInvitationLink("abc123", "refactest", "https://app.example.com");

        url.Should().Be("https://app.example.com/convite/abc123?tenant=refactest");
    }

    [Fact]
    public void BuildInvitationLink_TrimsTrailingSlash()
    {
        var url = AssociatesController.BuildInvitationLink("t", "sub", "https://app.example.com/");

        url.Should().Be("https://app.example.com/convite/t?tenant=sub");
    }

    [Fact]
    public void BuildInvitationLink_EncodesTokenAndSubdomain()
    {
        var url = AssociatesController.BuildInvitationLink("a/b", "s p", "https://x.com");

        url.Should().Be("https://x.com/convite/a%2Fb?tenant=s%20p");
    }

    [Fact]
    public async Task CreateInvitation_EmptyFrontendBaseUrl_DoesNotCreateInvitation()
    {
        var invitations = new Mock<IAssociateInvitationService>();
        var options = Options.Create(new InvitationLinkOptions { FrontendBaseUrl = "" });
        var sut = new AssociatesController(
            service: null!,
            invitations.Object,
            options);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "admin-1")],
            authenticationType: "Test"));
        httpContext.Request.Headers["X-Tenant-Subdomain"] = "club";

        sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await sut.CreateInvitation(new AssociatesController.CreateInvitationBody(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        invitations.Verify(
            s => s.CreateAsync(
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
