using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Teams;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class GetTeamQueryHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly GetTeamQueryHandler _handler;

    public GetTeamQueryHandlerTests()
    {
        _handler = new GetTeamQueryHandler(_teamRepo.Object);
    }

    [Fact]
    public async Task Handle_TeamNotFound_ShouldReturnTeamNotFound()
    {
        _teamRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var result = await _handler.HandleAsync(new GetTeamQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingActiveTeam_ShouldReturnResponse()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 11);
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var result = await _handler.HandleAsync(new GetTeamQuery(team.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(team.Id);
    }
}
