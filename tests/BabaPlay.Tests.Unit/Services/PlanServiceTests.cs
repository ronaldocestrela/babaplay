using BabaPlay.Modules.Platform.Entities;
using BabaPlay.Modules.Platform.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class PlanServiceTests
{
    private readonly Mock<IPlatformRepository<Plan>> _repo;
    private readonly Mock<IPlatformUnitOfWork> _uow;
    private readonly PlanService _sut;

    public PlanServiceTests()
    {
        _repo = new Mock<IPlatformRepository<Plan>>();
        _uow = new Mock<IPlatformUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new PlanService(_repo.Object, _uow.Object);
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsAllPlansOrderedByName()
    {
        var plans = new List<Plan>
        {
            new() { Id = "p2", Name = "Premium" },
            new() { Id = "p1", Name = "Basic" }
        };
        _repo.Setup(r => r.Query()).Returns(plans.AsAsyncQueryable());

        var result = await _sut.ListAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Basic");
    }

    // ── Get ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_ExistingId_ReturnsPlan()
    {
        var plan = new Plan { Id = "p1", Name = "Basic" };
        _repo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var result = await _sut.GetAsync("p1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Basic");
    }

    [Fact]
    public async Task Get_NonExistingId_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((Plan?)null);

        var result = await _sut.GetAsync("missing", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyName_ReturnsInvalid(string name)
    {
        var result = await _sut.CreateAsync(name, null, 29.90m, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        _repo.Verify(r => r.AddAsync(It.IsAny<Plan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidData_PersistsAndReturnsPlan()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Plan>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync("Pro", "Pro plan", 99.90m, 200, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Pro");
        result.Value.MonthlyPrice.Should().Be(99.90m);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_NonExistingId_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("p99", It.IsAny<CancellationToken>())).ReturnsAsync((Plan?)null);

        var result = await _sut.UpdateAsync("p99", "X", null, 10m, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Update_ValidData_UpdatesPlanAndReturnsIt()
    {
        var plan = new Plan { Id = "p1", Name = "Old" };
        _repo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var result = await _sut.UpdateAsync("p1", "New Name", "desc", 49.90m, 100, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.MonthlyPrice.Should().Be(49.90m);
        _repo.Verify(r => r.Update(plan), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("p99", It.IsAny<CancellationToken>())).ReturnsAsync((Plan?)null);

        var result = await _sut.DeleteAsync("p99", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Delete_ExistingId_RemovesAndReturnsSuccess()
    {
        var plan = new Plan { Id = "p1", Name = "Basic" };
        _repo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var result = await _sut.DeleteAsync("p1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.Remove(plan), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
