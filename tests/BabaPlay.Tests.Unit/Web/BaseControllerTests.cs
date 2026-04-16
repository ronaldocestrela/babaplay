using System.Security.Claims;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Tests.Unit.Web;

public sealed class BaseControllerTests
{
    [Fact]
    public void FromResult_Success_ReturnsOkEnvelope()
    {
        var sut = new TestController();

        var response = sut.Invoke(Result.Success());

        var ok = response.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<object?>>().Subject;
        body.Success.Should().BeTrue();
        body.Data.Should().BeNull();
    }

    [Theory]
    [InlineData(ResultStatus.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ResultStatus.Invalid, StatusCodes.Status400BadRequest)]
    [InlineData(ResultStatus.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ResultStatus.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ResultStatus.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ResultStatus.Error, StatusCodes.Status400BadRequest)]
    public void FromResult_FailureStatus_MapsToExpectedHttpCode(ResultStatus status, int expectedStatusCode)
    {
        var sut = new TestController();

        var response = sut.Invoke(Result.Failure("boom", status));

        var obj = response.Should().BeAssignableTo<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void GetUserId_FallsBackToSubClaim_WhenNameIdentifierIsMissing()
    {
        var sut = new TestController();
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim("sub", "user-123")],
                    authenticationType: "Test"))
        };

        sut.ControllerContext = new ControllerContext { HttpContext = http };

        var userId = sut.InvokeGetUserId();

        userId.Should().Be("user-123");
    }

    private sealed class TestController : BaseController
    {
        public IActionResult Invoke(Result result) => FromResult(result);
        public string? InvokeGetUserId() => GetUserId();
    }
}
