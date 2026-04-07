using BabaPlay.Modules.Associations.Entities;
using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.Modules.TeamGeneration.Entities;
using BabaPlay.Modules.TeamGeneration.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class TeamGenerationServiceTests
{
    private readonly Mock<ITenantRepository<Association>> _assocRepo;
    private readonly Mock<ITenantRepository<CheckIn>> _checkInRepo;
    private readonly Mock<ITenantRepository<Team>> _teamRepo;
    private readonly Mock<ITenantRepository<TeamMember>> _memberRepo;
    private readonly Mock<ITenantUnitOfWork> _uow;
    private readonly TeamGenerationService _sut;

    public TeamGenerationServiceTests()
    {
        _assocRepo = new Mock<ITenantRepository<Association>>();
        _checkInRepo = new Mock<ITenantRepository<CheckIn>>();
        _teamRepo = new Mock<ITenantRepository<Team>>();
        _memberRepo = new Mock<ITenantRepository<TeamMember>>();
        _uow = new Mock<ITenantUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _assocRepo.Setup(r => r.Query()).Returns(new List<Association> { new() { PlayersPerTeam = 5 } }.AsAsyncQueryable());

        _sut = new TeamGenerationService(
            _assocRepo.Object, _checkInRepo.Object, _teamRepo.Object, _memberRepo.Object, _uow.Object);
    }

    // ── GenerateFromSession ───────────────────────────────────────────────────

    [Fact]
    public async Task Generate_AssociationPlayersPerTeamInvalid_ReturnsInvalid()
    {
        _assocRepo.Setup(r => r.Query()).Returns(new List<Association> { new() { PlayersPerTeam = 1 } }.AsAsyncQueryable());
        _checkInRepo.Setup(r => r.Query()).Returns(new List<CheckIn>
        {
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow }
        }.AsAsyncQueryable());

        var result = await _sut.GenerateFromSessionAsync("s1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Contain("Players per team");
    }

    [Fact]
    public async Task Generate_NoCheckIns_ReturnsInvalid()
    {
        _checkInRepo.Setup(r => r.Query()).Returns(new List<CheckIn>().AsAsyncQueryable());

        var result = await _sut.GenerateFromSessionAsync("s1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Contain("No check-ins");
    }

    [Fact]
    public async Task Generate_ValidData_CreatesTeamsFromPlayersPerTeamSetting()
    {
        var checkIns = new List<CheckIn>
        {
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow.AddMinutes(-30) },
            new() { SessionId = "s1", AssociateId = "a2", CheckedInAt = DateTime.UtcNow.AddMinutes(-20) },
            new() { SessionId = "s1", AssociateId = "a3", CheckedInAt = DateTime.UtcNow.AddMinutes(-10) },
            new() { SessionId = "s1", AssociateId = "a4", CheckedInAt = DateTime.UtcNow }
        };

        _checkInRepo.Setup(r => r.Query()).Returns(checkIns.AsAsyncQueryable());
        _teamRepo.Setup(r => r.Query()).Returns(new List<Team>().AsAsyncQueryable());
        _memberRepo.Setup(r => r.Query()).Returns(new List<TeamMember>().AsAsyncQueryable());
        _teamRepo.Setup(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _memberRepo.Setup(r => r.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.GenerateFromSessionAsync("s1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // 4 associates, playersPerTeam 5 → max(2, 4/5) = 2 teams
        result.Value.Should().HaveCount(2);
        _teamRepo.Verify(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _memberRepo.Verify(r => r.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
    }

    [Fact]
    public async Task Generate_DuplicateCheckIns_CountsAssociateOnce()
    {
        var checkIns = new List<CheckIn>
        {
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow.AddMinutes(-10) },
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow.AddMinutes(-5) },
            new() { SessionId = "s1", AssociateId = "a2", CheckedInAt = DateTime.UtcNow }
        };

        _checkInRepo.Setup(r => r.Query()).Returns(checkIns.AsAsyncQueryable());
        _teamRepo.Setup(r => r.Query()).Returns(new List<Team>().AsAsyncQueryable());
        _memberRepo.Setup(r => r.Query()).Returns(new List<TeamMember>().AsAsyncQueryable());
        _teamRepo.Setup(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _memberRepo.Setup(r => r.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.GenerateFromSessionAsync("s1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _memberRepo.Verify(r => r.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Generate_ExistingTeamsForSession_RemovesThemFirst()
    {
        var checkIns = new List<CheckIn>
        {
            new() { SessionId = "s1", AssociateId = "a1", CheckedInAt = DateTime.UtcNow }
        };
        var existingTeam = new Team { Id = "t-old", SessionId = "s1" };
        var existingMember = new TeamMember { TeamId = "t-old", AssociateId = "a99" };

        _checkInRepo.Setup(r => r.Query()).Returns(checkIns.AsAsyncQueryable());
        _teamRepo.Setup(r => r.Query()).Returns(new List<Team> { existingTeam }.AsAsyncQueryable());
        _memberRepo.Setup(r => r.Query()).Returns(new List<TeamMember> { existingMember }.AsAsyncQueryable());
        _teamRepo.Setup(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _memberRepo.Setup(r => r.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.GenerateFromSessionAsync("s1", CancellationToken.None);

        _memberRepo.Verify(r => r.Remove(existingMember), Times.Once);
        _teamRepo.Verify(r => r.Remove(existingTeam), Times.Once);
    }

    [Fact]
    public async Task Generate_FifteenAssociatesPlayersPerTeam5_CreatesThreeTeams()
    {
        var checkIns = Enumerable.Range(1, 15).Select(i =>
            new CheckIn { SessionId = "s1", AssociateId = $"a{i}", CheckedInAt = DateTime.UtcNow.AddMinutes(-i) }).ToList();

        _checkInRepo.Setup(r => r.Query()).Returns(checkIns.AsAsyncQueryable());
        _teamRepo.Setup(r => r.Query()).Returns(new List<Team>().AsAsyncQueryable());
        _memberRepo.Setup(r => r.Query()).Returns(new List<TeamMember>().AsAsyncQueryable());
        _teamRepo.Setup(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _memberRepo.Setup(r => r.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.GenerateFromSessionAsync("s1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        _teamRepo.Verify(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    // ── GetWithMembers ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetWithMembers_ReturnsTeamsForSessionOrderedByName()
    {
        var teams = new List<Team>
        {
            new() { SessionId = "s1", Name = "Team 2" },
            new() { SessionId = "s1", Name = "Team 1" },
            new() { SessionId = "s2", Name = "Team X" }
        };
        _teamRepo.Setup(r => r.Query()).Returns(teams.AsAsyncQueryable());

        var result = await _sut.GetWithMembersAsync("s1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Team 1");
    }
}
