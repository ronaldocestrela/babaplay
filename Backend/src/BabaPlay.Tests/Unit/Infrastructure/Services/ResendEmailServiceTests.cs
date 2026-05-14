using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Services;
using BabaPlay.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Resend;
using ApplicationEmailMessage = BabaPlay.Application.Interfaces.EmailMessage;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public class ResendEmailServiceTests
{
    [Fact]
    public async Task SendAsync_MissingApiKey_ShouldReturnFailure()
    {
        var resendMock = new Mock<IResend>(MockBehavior.Strict);

        var sut = new ResendEmailService(
            resendMock.Object,
            Options.Create(new ResendEmailSettings
            {
                ApiKey = string.Empty,
                FromEmail = "noreply@babaplay.com",
                FromName = "BabaPlay",
                BaseUrl = "https://api.resend.com"
            }));

        var result = await sut.SendAsync(new ApplicationEmailMessage("user@club.com", "Subject", "<p>Body</p>"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_PROVIDER_NOT_CONFIGURED");
        resendMock.Verify(x => x.EmailSendAsync(It.IsAny<Resend.EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_ValidPayload_ShouldCallResendSdk()
    {
        var resendMock = new Mock<IResend>(MockBehavior.Strict);
        resendMock
            .Setup(x => x.EmailSendAsync(It.IsAny<Resend.EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));

        var sut = new ResendEmailService(
            resendMock.Object,
            Options.Create(new ResendEmailSettings
            {
                ApiKey = "re_test_key",
                FromEmail = "noreply@babaplay.com",
                FromName = "BabaPlay",
                BaseUrl = "https://api.resend.com"
            }));

        var result = await sut.SendAsync(new ApplicationEmailMessage("user@club.com", "Subject", "<p>Body</p>", "Body"));

        result.IsSuccess.Should().BeTrue();
        resendMock.Verify(x => x.EmailSendAsync(
            It.Is<Resend.EmailMessage>(m =>
                m.Subject == "Subject" &&
                m.To.Any(t => t.ToString() == "user@club.com")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ProviderFailure_ShouldReturnFailure()
    {
        var resendMock = new Mock<IResend>(MockBehavior.Strict);
        resendMock
            .Setup(x => x.EmailSendAsync(It.IsAny<Resend.EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("network down"));

        var sut = new ResendEmailService(
            resendMock.Object,
            Options.Create(new ResendEmailSettings
            {
                ApiKey = "re_test_key",
                FromEmail = "noreply@babaplay.com",
                FromName = "BabaPlay",
                BaseUrl = "https://api.resend.com"
            }));

        var result = await sut.SendAsync(new ApplicationEmailMessage("user@club.com", "Subject", "<p>Body</p>"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_SEND_FAILED");
        resendMock.Verify(x => x.EmailSendAsync(It.IsAny<Resend.EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
