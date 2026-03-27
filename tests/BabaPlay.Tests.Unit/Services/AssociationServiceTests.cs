using BabaPlay.Modules.Associations.Entities;
using BabaPlay.Modules.Associations.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class AssociationServiceTests
{
    private readonly Mock<ITenantRepository<Association>> _repo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly AssociationService _sut;

    public AssociationServiceTests()
    {
        _repo = new Mock<ITenantRepository<Association>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new AssociationService(_repo.Object, _uow.Object);
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsAssociationsOrderedByName()
    {
        var data = new List<Association>
        {
            new() { Name = "Zebra FC" },
            new() { Name = "Alpha SC" }
        };
        _repo.Setup(r => r.Query()).Returns(data.AsAsyncQueryable());

        var result = await _sut.ListAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("Alpha SC");
    }

    // ── Get ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_ExistingId_ReturnsAssociation()
    {
        var assoc = new Association { Id = "a1", Name = "Alpha SC" };
        _repo.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(assoc);

        var result = await _sut.GetAsync("a1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alpha SC");
    }

    [Fact]
    public async Task Get_NonExistingId_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((Association?)null);

        var result = await _sut.GetAsync("x", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── UpsertSingle ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Upsert_EmptyName_ReturnsInvalid(string name)
    {
        var result = await _sut.UpsertSingleAsync(null, name, null, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Upsert_NoId_CreatesNewAssociation()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Association>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var result = await _sut.UpsertSingleAsync(null, "Alpha SC", "Rua A, 1", null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alpha SC");
        result.Value.Address.Should().Be("Rua A, 1");
        _repo.Verify(r => r.AddAsync(It.IsAny<Association>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_ExistingId_UpdatesAssociation()
    {
        var existing = new Association { Id = "a1", Name = "Old Name" };
        _repo.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var result = await _sut.UpsertSingleAsync("a1", "New Name", "Rua B, 2", "Regulamento v2", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Regulation.Should().Be("Regulamento v2");
        _repo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_IdProvidedButNotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("a99", It.IsAny<CancellationToken>())).ReturnsAsync((Association?)null);

        var result = await _sut.UpsertSingleAsync("a99", "Name", null, null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
