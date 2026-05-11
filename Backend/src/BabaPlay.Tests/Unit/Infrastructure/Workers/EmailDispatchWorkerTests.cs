using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Workers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BabaPlay.Tests.Unit.Infrastructure.Workers;

public class EmailDispatchWorkerTests
{
    [Fact]
    public async Task StartAsync_WithQueuedMessage_ShouldDispatchThroughEmailService()
    {
        var emailService = new Mock<IEmailService>();
        var dispatched = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        emailService
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Callback(() => dispatched.TrySetResult(true));

        var queue = new TestEmailDispatchQueue(new EmailMessage("user@club.com", "Subject", "<p>Body</p>"));
        var sut = new EmailDispatchWorker(queue, emailService.Object, NullLogger<EmailDispatchWorker>.Instance);

        await sut.StartAsync(CancellationToken.None);

        var completed = await Task.WhenAny(dispatched.Task, Task.Delay(TimeSpan.FromSeconds(2)));
        completed.Should().Be(dispatched.Task);

        await sut.StopAsync(CancellationToken.None);

        emailService.Verify(
            x => x.SendAsync(
                It.Is<EmailMessage>(m => m.To == "user@club.com" && m.Subject == "Subject"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class TestEmailDispatchQueue : IEmailDispatchQueue
    {
        private readonly Queue<EmailMessage> _messages;

        public TestEmailDispatchQueue(params EmailMessage[] messages)
        {
            _messages = new Queue<EmailMessage>(messages);
        }

        public Task EnqueueAsync(EmailMessage message, CancellationToken ct = default)
        {
            _messages.Enqueue(message);
            return Task.CompletedTask;
        }

        public async Task<EmailMessage> DequeueAsync(CancellationToken ct = default)
        {
            if (_messages.Count > 0)
                return _messages.Dequeue();

            await Task.Delay(Timeout.Infinite, ct);
            throw new OperationCanceledException(ct);
        }
    }
}
