using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Services;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public class EmailDispatchQueueTests
{
    [Fact]
    public async Task EnqueueAndDequeue_ShouldPreserveMessageData()
    {
        var sut = new EmailDispatchQueue();
        var expected = new EmailMessage("player@club.com", "Welcome", "<p>Hello</p>", "Hello");

        await sut.EnqueueAsync(expected);
        var actual = await sut.DequeueAsync();

        actual.Should().BeEquivalentTo(expected);
    }
}
