using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PositionTests
{
    [Fact]
    public void Create_ValidData_ReturnsActivePosition()
    {
        var tenantId = Guid.NewGuid();

        var position = Position.Create(tenantId, "  gk  ", "  Goleiro  ", "  Defesa do gol  ");

        position.Id.Should().NotBeEmpty();
        position.TenantId.Should().Be(tenantId);
        position.Code.Should().Be("gk");
        position.NormalizedCode.Should().Be("GK");
        position.Name.Should().Be("Goleiro");
        position.Description.Should().Be("Defesa do gol");
        position.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsValidationException()
    {
        var act = () => Position.Create(Guid.Empty, "GK", "Goleiro", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WhitespaceCode_ThrowsValidationException()
    {
        var act = () => Position.Create(Guid.NewGuid(), "   ", "Goleiro", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WhitespaceName_ThrowsValidationException()
    {
        var act = () => Position.Create(Guid.NewGuid(), "GK", "   ", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_ValidData_UpdatesFieldsAndMarksUpdated()
    {
        var position = Position.Create(Guid.NewGuid(), "GK", "Goleiro", null);

        position.Update("  gl  ", "  Goleiro Linha  ", "  Joga adiantado  ");

        position.Code.Should().Be("gl");
        position.NormalizedCode.Should().Be("GL");
        position.Name.Should().Be("Goleiro Linha");
        position.Description.Should().Be("Joga adiantado");
        position.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Twice_IsIdempotent()
    {
        var position = Position.Create(Guid.NewGuid(), "GK", "Goleiro", null);
        position.Deactivate();

        var act = () => position.Deactivate();

        act.Should().NotThrow();
        position.IsActive.Should().BeFalse();
    }
}
