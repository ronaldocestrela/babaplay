using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class UpdateMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IGameDayRepository> _gameDayRepository = new();
    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly UpdateMatchCommandHandler _handler;

    public UpdateMatchCommandHandlerTests()
    {
        _handler = new UpdateMatchCommandHandler(_matchRepository.Object, _gameDayRepository.Object, _teamRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnMatchNotFound()
    {
        _matchRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new UpdateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }
}