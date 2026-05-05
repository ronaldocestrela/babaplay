using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class NotificationTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateUnreadNotification()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.Checkin,
            "Check-in realizado",
            "Seu check-in foi confirmado.",
            "{\"gameDayId\":\"abc\"}");

        notification.Type.Should().Be(NotificationType.Checkin);
        notification.Title.Should().Be("Check-in realizado");
        notification.Message.Should().Be("Seu check-in foi confirmado.");
        notification.IsRead.Should().BeFalse();
        notification.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTitle_ShouldThrowValidationException()
    {
        var act = () => Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.Match,
            string.Empty,
            "Mensagem",
            null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void MarkAsRead_FirstCall_ShouldSetReadState()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.Match,
            "Partida confirmada",
            "A partida foi confirmada para hoje.",
            null);

        notification.MarkAsRead(DateTime.UtcNow);

        notification.IsRead.Should().BeTrue();
        notification.ReadAtUtc.Should().NotBeNull();
        notification.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsRead_SecondCall_ShouldBeIdempotent()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.MatchEvent,
            "Gol",
            "Gol registrado.",
            null);

        notification.MarkAsRead(DateTime.UtcNow);

        var act = () => notification.MarkAsRead(DateTime.UtcNow.AddMinutes(1));

        act.Should().NotThrow();
        notification.IsRead.Should().BeTrue();
    }
}
