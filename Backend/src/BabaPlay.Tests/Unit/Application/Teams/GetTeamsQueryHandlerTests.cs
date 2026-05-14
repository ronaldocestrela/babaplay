using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Teams;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class GetTeamsQueryHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly GetTeamsQueryHandler _handler;

    public GetTeamsQueryHandlerTests()
    {
        _handler = new GetTeamsQueryHandler(_teamRepo.Object);
    }

    [Fact]
    public async Task Handle_WhenNoTeams_ShouldReturnEmptyList()
    {
        _teamRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetTeamsQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenTeamsExist_ShouldReturnMappedList()
    {
        var teamA = Team.Create(Guid.NewGuid(), "Blue", 11);
        var teamB = Team.Create(Guid.NewGuid(), "Red", 7);

        _teamRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([teamA, teamB]);

        var result = await _handler.HandleAsync(new GetTeamsQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value!.Select(x => x.Name).Should().BeEquivalentTo(["Blue", "Red"]);
    }
}
