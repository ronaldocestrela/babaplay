using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class CreateTeamCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateTeamCommandHandler _handler;

    public CreateTeamCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateTeamCommandHandler(_teamRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnInvalidName()
    {
        var result = await _handler.HandleAsync(new CreateTeamCommand("", 11));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_NAME");
    }

    [Fact]
    public async Task Handle_InvalidMaxPlayers_ShouldReturnInvalidMaxPlayers()
    {
        var result = await _handler.HandleAsync(new CreateTeamCommand("Blue", 0));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_MAX_PLAYERS");
    }

    [Fact]
    public async Task Handle_DuplicateName_ShouldReturnTeamAlreadyExists()
    {
        _teamRepo.Setup(r => r.ExistsByNormalizedNameAsync("BLUE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new CreateTeamCommand("blue", 11));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_ALREADY_EXISTS");
        _teamRepo.Verify(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTeam()
    {
        _teamRepo.Setup(r => r.ExistsByNormalizedNameAsync("BLUE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateTeamCommand("blue", 11));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("blue");
        result.Value.MaxPlayers.Should().Be(11);
        _teamRepo.Verify(r => r.AddAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Once);
        _teamRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
