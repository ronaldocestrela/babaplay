using System.Net;
using System.Net.Http.Headers;
using System.Text;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Services;
using BabaPlay.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public class ResendEmailServiceTests
{
    [Fact]
    public async Task SendAsync_MissingApiKey_ShouldReturnFailure()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Accepted));
        using var httpClient = new HttpClient(handler);

        var sut = new ResendEmailService(
            httpClient,
            Options.Create(new ResendEmailSettings
            {
                ApiKey = string.Empty,
                FromEmail = "noreply@babaplay.com",
                FromName = "BabaPlay",
                BaseUrl = "https://api.resend.com"
            }));

        var result = await sut.SendAsync(new EmailMessage("user@club.com", "Subject", "<p>Body</p>"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_PROVIDER_NOT_CONFIGURED");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_ValidPayload_ShouldSendAuthorizedRequest()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Accepted));
        using var httpClient = new HttpClient(handler);

        var sut = new ResendEmailService(
            httpClient,
            Options.Create(new ResendEmailSettings
            {
                ApiKey = "re_test_key",
                FromEmail = "noreply@babaplay.com",
                FromName = "BabaPlay",
                BaseUrl = "https://api.resend.com"
            }));

        var result = await sut.SendAsync(new EmailMessage("user@club.com", "Subject", "<p>Body</p>", "Body"));

        result.IsSuccess.Should().BeTrue();
        handler.Requests.Should().HaveCount(1);

        var request = handler.Requests.Single();
        request.RequestUri.Should().Be("https://api.resend.com/emails");
        request.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", "re_test_key"));

        var json = request.Content;
        json.Should().Contain("\"to\"");
        json.Should().Contain("user@club.com");
        json.Should().Contain("\"from\"");
        json.Should().Contain("noreply@babaplay.com");
        json.Should().Contain("\"subject\"");
    }

    [Fact]
    public async Task SendAsync_ProviderFailure_ShouldReturnFailure()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"message\":\"invalid\"}", Encoding.UTF8, "application/json")
            });
        using var httpClient = new HttpClient(handler);

        var sut = new ResendEmailService(
            httpClient,
            Options.Create(new ResendEmailSettings
            {
                ApiKey = "re_test_key",
                FromEmail = "noreply@babaplay.com",
                FromName = "BabaPlay",
                BaseUrl = "https://api.resend.com"
            }));

        var result = await sut.SendAsync(new EmailMessage("user@club.com", "Subject", "<p>Body</p>"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_SEND_FAILED");
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.RequestUri?.ToString() ?? string.Empty,
                request.Headers.Authorization,
                request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken)));

            return _responseFactory(request);
        }
    }

    private sealed record RecordedRequest(
        string RequestUri,
        AuthenticationHeaderValue? Authorization,
        string Content);
}
