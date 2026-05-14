using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class DeleteTeamCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly DeleteTeamCommandHandler _handler;

    public DeleteTeamCommandHandlerTests()
    {
        _handler = new DeleteTeamCommandHandler(_teamRepo.Object);
    }

    [Fact]
    public async Task Handle_TeamNotFound_ShouldReturnTeamNotFound()
    {
        _teamRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var result = await _handler.HandleAsync(new DeleteTeamCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingTeam_ShouldDeactivateAndReturnSuccess()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 11);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var result = await _handler.HandleAsync(new DeleteTeamCommand(team.Id));

        result.IsSuccess.Should().BeTrue();
        team.IsActive.Should().BeFalse();
        _teamRepo.Verify(r => r.UpdateAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Once);
        _teamRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
