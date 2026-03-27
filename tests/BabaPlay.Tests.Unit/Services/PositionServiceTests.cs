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
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly PositionService _sut;

    public PositionServiceTests()
    {
        _repo = new Mock<ITenantRepository<Position>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new PositionService(_repo.Object, _uow.Object);
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
}
