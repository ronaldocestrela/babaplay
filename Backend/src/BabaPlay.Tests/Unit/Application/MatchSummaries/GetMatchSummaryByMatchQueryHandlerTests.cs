using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchSummaries;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchSummaries;

public class GetMatchSummaryByMatchQueryHandlerTests
{
    private readonly Mock<IMatchSummaryRepository> _summaryRepository = new();
    private readonly GetMatchSummaryByMatchQueryHandler _handler;

    public GetMatchSummaryByMatchQueryHandlerTests()
    {
        _handler = new GetMatchSummaryByMatchQueryHandler(_summaryRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _summaryRepository
            .Setup(x => x.GetByMatchIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchSummary?)null);

        var result = await _handler.HandleAsync(new GetMatchSummaryByMatchQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_SUMMARY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_Found_ShouldReturnResponse()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant/file.pdf",
            "summary-file.pdf",
            "application/pdf",
            300);

        _summaryRepository
            .Setup(x => x.GetByMatchIdAsync(summary.MatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _handler.HandleAsync(new GetMatchSummaryByMatchQuery(summary.MatchId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MatchId.Should().Be(summary.MatchId);
    }
}
