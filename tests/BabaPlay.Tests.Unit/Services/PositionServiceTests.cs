using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class PositionServiceTests
{
    private readonly Mock<ITenantRepository<Position>> _repo;
    private readonly Mock<ITenantRepository<AssociatePosition>> _associatePositions;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly PositionService _sut;

    public PositionServiceTests()
    {
        _repo = new Mock<ITenantRepository<Position>>();
        _associatePositions = new Mock<ITenantRepository<AssociatePosition>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new PositionService(_repo.Object, _associatePositions.Object, _uow.Object);
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsPositionsOrderedBySortOrderThenName()
    {
        var positions = new List<Position>
        {
            new() { Name = "Winger",   SortOrder = 2 },
            new() { Name = "Forward",  SortOrder = 1 },
            new() { Name = "Attacker", SortOrder = 1 }
        };
        _repo.Setup(r => r.Query()).Returns(positions.AsAsyncQueryable());

        var result = await _sut.ListAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].Name.Should().Be("Attacker");
        result.Value[1].Name.Should().Be("Forward");
        result.Value[2].Name.Should().Be("Winger");
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyName_ReturnsInvalid(string name)
    {
        var result = await _sut.CreateAsync(name, 1, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        _repo.Verify(r => r.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidData_PersistsAndReturnsPosition()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync("Forward", 1, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Forward");
        result.Value.SortOrder.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((Position?)null);

        var result = await _sut.UpdateAsync("x", "Name", 1, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_EmptyName_ReturnsInvalid(string name)
    {
        _repo.Setup(r => r.GetByIdAsync("id", It.IsAny<CancellationToken>()))
             .ReturnsAsync(new Position { Id = "id", Name = "Old", SortOrder = 0 });

        var result = await _sut.UpdateAsync("id", name, 1, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        _repo.Verify(r => r.Update(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async Task Update_ValidData_PersistsAndReturnsPosition()
    {
        var existing = new Position { Id = "id", Name = "Old", SortOrder = 0 };
        _repo.Setup(r => r.GetByIdAsync("id", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var result = await _sut.UpdateAsync("id", "New", 5, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New");
        result.Value.SortOrder.Should().Be(5);
        result.Value.UpdatedAt.Should().NotBeNull();
        _repo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Delete ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((Position?)null);

        var result = await _sut.DeleteAsync("x", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Delete_InUse_ReturnsConflict()
    {
        var position = new Position { Id = "pid", Name = "G", SortOrder = 1 };
        _repo.Setup(r => r.GetByIdAsync("pid", It.IsAny<CancellationToken>())).ReturnsAsync(position);
        _associatePositions.Setup(r => r.Query()).Returns(
            new[] { new AssociatePosition { PositionId = "pid" } }.AsAsyncQueryable());

        var result = await _sut.DeleteAsync("pid", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
        _repo.Verify(r => r.Remove(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NotInUse_RemovesAndSaves()
    {
        var position = new Position { Id = "pid", Name = "G", SortOrder = 1 };
        _repo.Setup(r => r.GetByIdAsync("pid", It.IsAny<CancellationToken>())).ReturnsAsync(position);
        _associatePositions.Setup(r => r.Query()).Returns(Array.Empty<AssociatePosition>().AsAsyncQueryable());

        var result = await _sut.DeleteAsync("pid", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.Remove(position), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
