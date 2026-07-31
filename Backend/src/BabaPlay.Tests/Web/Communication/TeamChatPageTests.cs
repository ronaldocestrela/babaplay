using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Pages.Communication;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.SignalR;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web.Communication;

public class TeamChatPageTests : TestContext
{
    private readonly Mock<ISignalRChatService> _mockChatService = new();

    public TeamChatPageTests()
    {
        this.AddTestAuthorization().SetAuthorized("Test User");

        Services.AddSingleton(_mockChatService.Object);
        Services.AddSingleton(new TenantState());
        Services.AddSingleton(new UserSessionState());
    }

    [Fact]
    public void TeamChat_ShouldRenderMessagesList()
    {
        // Arrange
        var messages = new List<ChatMessageDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Jogador Um", "E aí galera!", DateTime.UtcNow),
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Jogador Dois", "Tudo certo pro jogo!", DateTime.UtcNow)
        };

        _mockChatService
            .Setup(x => x.GetRecentMessagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        // Act
        var cut = RenderComponent<TeamChat>();

        // Assert
        cut.FindAll("[data-testid='chat-message-item']").Should().HaveCount(2);
        cut.Find("[data-testid='chat-messages-list']").TextContent.Should().Contain("E aí galera!");
    }

    [Fact]
    public async Task TeamChat_SendMessage_ShouldCallSendMessageAsync()
    {
        // Arrange
        _mockChatService
            .Setup(x => x.GetRecentMessagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _mockChatService
            .Setup(x => x.SendMessageAsync("Bora jogar!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var cut = RenderComponent<TeamChat>();

        // Act
        var input = cut.Find("[data-testid='input-chat-message']");
        await input.InputAsync(new ChangeEventArgs { Value = "Bora jogar!" });

        var sendBtn = cut.Find("[data-testid='btn-send-message']");
        await sendBtn.ClickAsync(new MouseEventArgs());

        // Assert
        _mockChatService.Verify(x => x.SendMessageAsync("Bora jogar!", It.IsAny<CancellationToken>()), Times.Once);
    }
}
