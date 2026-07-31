using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Chat;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Chat;

public class GetRecentChatMessagesQueryHandlerTests
{
    private readonly Mock<IChatMessageRepository> _chatRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly GetRecentChatMessagesQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SenderId = Guid.NewGuid();

    public GetRecentChatMessagesQueryHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new GetRecentChatMessagesQueryHandler(_chatRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnMessagesInChronologicalOrder()
    {
        var msg1 = ChatMessage.Create(TenantId, SenderId, "Jogador 1", "Primeira mensagem");
        var msg2 = ChatMessage.Create(TenantId, SenderId, "Jogador 2", "Segunda mensagem");

        _chatRepo
            .Setup(x => x.GetRecentMessagesAsync(TenantId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync([msg1, msg2]);

        var query = new GetRecentChatMessagesQuery(50);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Content.Should().Be("Primeira mensagem");
        result.Value[1].Content.Should().Be("Segunda mensagem");
    }
}
