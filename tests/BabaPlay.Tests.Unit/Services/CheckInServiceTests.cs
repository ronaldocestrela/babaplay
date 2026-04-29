using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.Modules.CheckIns.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class CheckInServiceTests
{
    private readonly Mock<ITenantRepository<CheckInSession>> _sessionRepo;
    private readonly Mock<ITenantRepository<CheckIn>> _checkInRepo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly CheckInService _sut;

    public CheckInServiceTests()
    {
        _sessionRepo = new Mock<ITenantRepository<CheckInSession>>();
        _checkInRepo = new Mock<ITenantRepository<CheckIn>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new CheckInService(_sessionRepo.Object, _checkInRepo.Object, _uow.Object);
    }

    // ── StartSession ─────────────────────────────────────────────────────────

    [Fact]
    public async Task StartSession_CreatesAndReturnsSession()
    {
        _sessionRepo.Setup(r => r.Query()).Returns(new List<CheckInSession>().AsAsyncQueryable());
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var result = await _sut.StartSessionAsync("user1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedByUserId.Should().Be("user1");
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartSession_NullUserId_StillCreatesSession()
    {
        _sessionRepo.Setup(r => r.Query()).Returns(new List<CheckInSession>().AsAsyncQueryable());
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var result = await _sut.StartSessionAsync(null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedByUserId.Should().BeNull();
    }

    [Fact]
    public async Task StartSession_SessionAlreadyExistsToday_ReturnsConflict()
    {
        var today = DateTime.UtcNow.Date;
        var existing = new List<CheckInSession>
        {
            new() { Id = "s-old", StartedAt = today.AddHours(9) }
        };
        _sessionRepo.Setup(r => r.Query()).Returns(existing.AsAsyncQueryable());

        var result = await _sut.StartSessionAsync("user1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Error.Should().Contain("session");
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartSession_SessionExistsYesterday_CreatesNewSession()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var existing = new List<CheckInSession>
        {
            new() { Id = "s-old", StartedAt = yesterday.AddHours(10) }
        };
        _sessionRepo.Setup(r => r.Query()).Returns(existing.AsAsyncQueryable());
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var result = await _sut.StartSessionAsync("user1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartSession_OnlyOldSessions_CreatesNewSession()
    {
        var old = new List<CheckInSession>
        {
            new() { Id = "s1", StartedAt = DateTime.UtcNow.Date.AddDays(-7) },
            new() { Id = "s2", StartedAt = DateTime.UtcNow.Date.AddDays(-14) }
        };
        _sessionRepo.Setup(r => r.Query()).Returns(old.AsAsyncQueryable());
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<CheckInSession>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var result = await _sut.StartSessionAsync("user1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ── RegisterCheckIn ──────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterCheckIn_SessionNotFound_ReturnsNotFound()
    {
        _sessionRepo.Setup(r => r.GetByIdAsync("s99", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((CheckInSession?)null);

        var result = await _sut.RegisterCheckInAsync("s99", "a1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task RegisterCheckIn_AssociateAlreadyCheckedInToday_ReturnsConflict()
    {
        var session = new CheckInSession { Id = "s1" };
        _sessionRepo.Setup(r => r.GetByIdAsync("s1", It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var today = DateTime.UtcNow.Date;
        var existing = new List<CheckIn>
        {
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = today.AddHours(8) }
        };
        _checkInRepo.Setup(r => r.Query()).Returns(existing.AsAsyncQueryable());

        var result = await _sut.RegisterCheckInAsync("s1", "a1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Error.Should().Contain("already checked in");
    }

    [Fact]
    public async Task RegisterCheckIn_ValidData_CreatesCheckIn()
    {
        var session = new CheckInSession { Id = "s1" };
        _sessionRepo.Setup(r => r.GetByIdAsync("s1", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _checkInRepo.Setup(r => r.Query()).Returns(new List<CheckIn>().AsAsyncQueryable());
        _checkInRepo.Setup(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var result = await _sut.RegisterCheckInAsync("s1", "a1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SessionId.Should().Be("s1");
        result.Value.AssociateId.Should().Be("a1");
        _checkInRepo.Verify(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ListForSession ────────────────────────────────────────────────────────

    [Fact]
    public async Task ListForSession_ReturnsCheckInsOrderedByCheckedInAt()
    {
        var first = new CheckIn { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow.AddMinutes(-10) };
        var second = new CheckIn { SessionId = "s1", AssociateId = "a2", CheckedInAt = DateTime.UtcNow };
        _checkInRepo.Setup(r => r.Query())
                    .Returns(new List<CheckIn> { second, first }.AsAsyncQueryable());

        var result = await _sut.ListForSessionAsync("s1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AssociateId.Should().Be("a1");
    }

    [Fact]
    public async Task ListForSession_OtherSessionCheckInsNotReturned()
    {
        var checkIns = new List<CheckIn>
        {
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow },
            new() { SessionId = "s2", AssociateId = "a2", CheckedInAt = DateTime.UtcNow }
        };
        _checkInRepo.Setup(r => r.Query()).Returns(checkIns.AsAsyncQueryable());

        var result = await _sut.ListForSessionAsync("s1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].AssociateId.Should().Be("a1");
    }
}
