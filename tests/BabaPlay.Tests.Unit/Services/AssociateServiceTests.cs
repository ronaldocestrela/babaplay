using BabaPlay.Modules.Associates.Dtos;
using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class AssociateServiceTests
{
    private readonly Mock<ITenantRepository<Associate>> _associateRepo;
    private readonly Mock<ITenantRepository<AssociatePosition>> _linkRepo;
    private readonly Mock<ITenantRepository<Position>> _positionRepo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly Mock<IAssociateUserProvisioner> _provisioner;
    private readonly AssociateService _sut;

    public AssociateServiceTests()
    {
        _associateRepo = new Mock<ITenantRepository<Associate>>();
        _linkRepo = new Mock<ITenantRepository<AssociatePosition>>();
        _positionRepo = new Mock<ITenantRepository<Position>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _provisioner = new Mock<IAssociateUserProvisioner>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _provisioner
            .Setup(p => p.ProvisionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("user-1"));

        _sut = new AssociateService(
            _associateRepo.Object, _linkRepo.Object, _positionRepo.Object, _uow.Object, _provisioner.Object);
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsAllAssociatesOrderedByName()
    {
        var associates = new List<Associate>
        {
            new() { Id = "a2", Name = "Zé" },
            new() { Id = "a1", Name = "Ana" }
        };
        _associateRepo.Setup(r => r.Query()).Returns(associates.AsAsyncQueryable());

        Result<IReadOnlyList<AssociateResponse>> result = await _sut.ListAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Ana");
    }

    // ── Get ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_NonExistingId_ReturnsNotFound()
    {
        _associateRepo.Setup(r => r.Query()).Returns(new List<Associate>().AsAsyncQueryable());

        var result = await _sut.GetAsync("missing", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Get_ExistingId_ReturnsAssociate()
    {
        var associate = new Associate { Id = "a1", Name = "Ana" };
        _associateRepo.Setup(r => r.Query())
                      .Returns(new List<Associate> { associate }.AsAsyncQueryable());

        Result<AssociateResponse> result = await _sut.GetAsync("a1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Ana");
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyName_ReturnsInvalid(string name)
    {
        var result = await _sut.CreateAsync(name, null, null, ["p1"], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Create_ZeroPositions_ReturnsInvalid()
    {
        var result = await _sut.CreateAsync("Ana", "ana@test.com", null, [], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Contain("between 1 and 3");
    }

    [Fact]
    public async Task Create_FourPositions_ReturnsInvalid()
    {
        var result = await _sut.CreateAsync("Ana", "ana@test.com", null, ["p1", "p2", "p3", "p4"], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Create_PositionNotFound_ReturnsInvalid()
    {
        _positionRepo.Setup(r => r.GetByIdAsync("p-missing", It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Position?)null);

        var result = await _sut.CreateAsync("Ana", "ana@test.com", null, ["p-missing"], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EmptyEmail_ReturnsInvalid(string? email)
    {
        var result = await _sut.CreateAsync("Ana", email, null, ["p1"], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Be("Email is required.");
        _provisioner.Verify(
            p => p.ProvisionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_ValidData_PersistsAssociateAndLinks()
    {
        var position = new Position { Id = "p1", Name = "Forward" };
        _positionRepo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(position);

        // Capture the associate created inside CreateAsync so GetAsync (called at the end) can find it
        Associate? captured = null;
        _associateRepo.Setup(r => r.AddAsync(It.IsAny<Associate>(), It.IsAny<CancellationToken>()))
                      .Callback<Associate, CancellationToken>((a, _) => captured = a)
                      .Returns(Task.CompletedTask);
        _linkRepo.Setup(r => r.AddAsync(It.IsAny<AssociatePosition>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        // Query() is called by GetAsync at the end of CreateAsync; return the captured associate
        _associateRepo.Setup(r => r.Query())
                      .Returns(() => new List<Associate> { captured! }.AsAsyncQueryable());

        var result = await _sut.CreateAsync("Ana", "ana@test.com", null, ["p1"], CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.UserId.Should().Be("user-1");
        _provisioner.Verify(
            p => p.ProvisionAsync(captured.Id, "ana@test.com", It.IsAny<CancellationToken>()),
            Times.Once);
        _associateRepo.Verify(r => r.AddAsync(It.IsAny<Associate>(), It.IsAny<CancellationToken>()), Times.Once);
        _linkRepo.Verify(r => r.AddAsync(It.IsAny<AssociatePosition>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_EmptyName_ReturnsInvalid(string name)
    {
        var result = await _sut.UpdateAsync("a1", name, null, null, ["p1"], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Update_AssociateNotFound_ReturnsNotFound()
    {
        var position = new Position { Id = "p1" };
        _positionRepo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(position);
        _associateRepo.Setup(r => r.GetByIdAsync("a99", It.IsAny<CancellationToken>())).ReturnsAsync((Associate?)null);

        var result = await _sut.UpdateAsync("a99", "Ana", null, null, ["p1"], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Update_ValidData_UpdatesAssociateAndReplacesLinks()
    {
        var position = new Position { Id = "p1" };
        _positionRepo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(position);

        var associate = new Associate { Id = "a1", Name = "Old" };
        _associateRepo.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(associate);

        var existingLinks = new List<AssociatePosition>
        {
            new() { AssociateId = "a1", PositionId = "old-pos" }
        };
        _linkRepo.Setup(r => r.Query()).Returns(existingLinks.AsAsyncQueryable());
        _linkRepo.Setup(r => r.AddAsync(It.IsAny<AssociatePosition>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var updated = new Associate { Id = "a1", Name = "Ana" };
        _associateRepo.Setup(r => r.Query())
                      .Returns(new List<Associate> { updated }.AsAsyncQueryable());

        var result = await _sut.UpdateAsync("a1", "Ana", "ana@test.com", null, ["p1"], CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _associateRepo.Verify(r => r.Update(associate), Times.Once);
        _linkRepo.Verify(r => r.Remove(It.IsAny<AssociatePosition>()), Times.Once);
        _linkRepo.Verify(r => r.AddAsync(It.IsAny<AssociatePosition>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── SetActive ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetActive_NotFound_ReturnsNotFound()
    {
        _associateRepo.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((Associate?)null);

        var result = await _sut.SetActiveAsync("missing", false, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
        _associateRepo.Verify(r => r.Update(It.IsAny<Associate>()), Times.Never);
    }

    [Fact]
    public async Task SetActive_DeactivatesAndReturnsAssociate()
    {
        var associate = new Associate { Id = "a1", Name = "Ana", IsActive = true, Positions = new List<AssociatePosition>() };
        _associateRepo.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(associate);
        _associateRepo.Setup(r => r.Query()).Returns(new List<Associate> { associate }.AsAsyncQueryable());

        var result = await _sut.SetActiveAsync("a1", false, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        associate.IsActive.Should().BeFalse();
        associate.UpdatedAt.Should().NotBeNull();
        _associateRepo.Verify(r => r.Update(associate), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetActive_Activates()
    {
        var associate = new Associate { Id = "a1", Name = "Ana", IsActive = false, Positions = new List<AssociatePosition>() };
        _associateRepo.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(associate);
        _associateRepo.Setup(r => r.Query()).Returns(new List<Associate> { associate }.AsAsyncQueryable());

        var result = await _sut.SetActiveAsync("a1", true, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        associate.IsActive.Should().BeTrue();
    }
}
