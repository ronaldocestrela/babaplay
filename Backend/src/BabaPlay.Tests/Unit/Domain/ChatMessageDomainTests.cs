using System;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Unit.Domain;

public class ChatMessageDomainTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SenderId = Guid.NewGuid();
    private const string SenderName = "Ronaldinho";
    private const string Content = "Galera, amanhã o jogo é às 9h no campo principal!";

    [Fact]
    public void Create_WithValidData_ShouldCreateChatMessage()
    {
        var msg = ChatMessage.Create(TenantId, SenderId, SenderName, Content);

        msg.Should().NotBeNull();
        msg.TenantId.Should().Be(TenantId);
        msg.SenderId.Should().Be(SenderId);
        msg.SenderName.Should().Be("Ronaldinho");
        msg.Content.Should().Be(Content);
        msg.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyContent_ShouldThrowValidationException()
    {
        var act = () => ChatMessage.Create(TenantId, SenderId, SenderName, "   ");
        act.Should().Throw<ValidationException>().Where(ex => ex.Errors.ContainsKey("Content"));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var msg = ChatMessage.Create(TenantId, SenderId, SenderName, Content);
        msg.Deactivate();

        msg.IsActive.Should().BeFalse();
    }
}
