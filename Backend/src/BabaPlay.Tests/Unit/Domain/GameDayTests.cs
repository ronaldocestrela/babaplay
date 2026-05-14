using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class GameDayTests
{
    [Fact]
    public void Create_ValidData_ReturnsPendingActiveGameDay()
    {
        var tenantId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddHours(4);

        var gameDay = GameDay.Create(tenantId, "  Rodada de Domingo  ", scheduledAt, "  Campo A  ", "  Jogo semanal  ", 22);

        gameDay.Id.Should().NotBeEmpty();
        gameDay.TenantId.Should().Be(tenantId);
        gameDay.Name.Should().Be("Rodada de Domingo");
        gameDay.NormalizedName.Should().Be("RODADA DE DOMINGO");
        gameDay.ScheduledAt.Should().Be(scheduledAt);
        gameDay.Location.Should().Be("Campo A");
        gameDay.Description.Should().Be("Jogo semanal");
        gameDay.MaxPlayers.Should().Be(22);
        gameDay.Status.Should().Be(GameDayStatus.Pending);
        gameDay.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsValidationException()
    {
        var act = () => GameDay.Create(Guid.Empty, "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_PastScheduledAt_ThrowsValidationException()
    {
        var act = () => GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddMinutes(-5), null, null, 22);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_InvalidMaxPlayers_ThrowsValidationException()
    {
        var act = () => GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 0);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_ValidData_UpdatesFieldsAndMarksUpdated()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22);
        var newScheduledAt = DateTime.UtcNow.AddDays(1);

        gameDay.Update("  Rodada Atualizada  ", newScheduledAt, "  Campo B  ", "  Nova descrição  ", 18);

        gameDay.Name.Should().Be("Rodada Atualizada");
        gameDay.NormalizedName.Should().Be("RODADA ATUALIZADA");
        gameDay.ScheduledAt.Should().Be(newScheduledAt);
        gameDay.Location.Should().Be("Campo B");
        gameDay.Description.Should().Be("Nova descrição");
        gameDay.MaxPlayers.Should().Be(18);
        gameDay.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangeStatus_ValidTransitions_ShouldUpdateStatus()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);

        gameDay.ChangeStatus(GameDayStatus.Confirmed);
        gameDay.ChangeStatus(GameDayStatus.Completed);

        gameDay.Status.Should().Be(GameDayStatus.Completed);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsValidationException()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);

        gameDay.ChangeStatus(GameDayStatus.Cancelled);

        var act = () => gameDay.ChangeStatus(GameDayStatus.Confirmed);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Deactivate_Twice_IsIdempotent()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);
        gameDay.Deactivate();

        var act = () => gameDay.Deactivate();

        act.Should().NotThrow();
        gameDay.IsActive.Should().BeFalse();
    }
}
