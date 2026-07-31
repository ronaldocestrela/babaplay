using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Commands.Chat;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Chat;

public class SendChatMessageCommandHandlerTests
{
    private readonly Mock<IChatMessageRepository> _chatRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly SendChatMessageCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SenderId = Guid.NewGuid();

    public SendChatMessageCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new SendChatMessageCommandHandler(_chatRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyContent_ShouldReturnInvalidContentError()
    {
        var command = new SendChatMessageCommand(SenderId, "Ronaldinho", "");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CONTENT");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistMessageAndReturnDto()
    {
        var command = new SendChatMessageCommand(SenderId, "Ronaldinho", "Fala galera, tudo pronto pro baba!");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SenderName.Should().Be("Ronaldinho");
        result.Value.Content.Should().Be("Fala galera, tudo pronto pro baba!");

        _chatRepo.Verify(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
