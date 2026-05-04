using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class UpdateTeamCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly UpdateTeamCommandHandler _handler;

    public UpdateTeamCommandHandlerTests()
    {
        _handler = new UpdateTeamCommandHandler(_teamRepo.Object);
    }

    [Fact]
    public async Task Handle_TeamNotFound_ShouldReturnTeamNotFound()
    {
        var id = Guid.NewGuid();
        _teamRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var result = await _handler.HandleAsync(new UpdateTeamCommand(id, "Blue", 11));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_DuplicateName_ShouldReturnTeamAlreadyExists()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 11);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        _teamRepo.Setup(r => r.ExistsByNormalizedNameAsync("RED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new UpdateTeamCommand(team.Id, "red", 11));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateTeam()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 11);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        _teamRepo.Setup(r => r.ExistsByNormalizedNameAsync("RED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new UpdateTeamCommand(team.Id, "Red", 7));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Red");
        result.Value.MaxPlayers.Should().Be(7);
        _teamRepo.Verify(r => r.UpdateAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Once);
        _teamRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
