using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchSummaries;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchSummaries;

public class GetMatchSummaryQueryHandlerTests
{
    private readonly Mock<IMatchSummaryRepository> _summaryRepository = new();
    private readonly GetMatchSummaryQueryHandler _handler;

    public GetMatchSummaryQueryHandlerTests()
    {
        _handler = new GetMatchSummaryQueryHandler(_summaryRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _summaryRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchSummary?)null);

        var result = await _handler.HandleAsync(new GetMatchSummaryQuery(Guid.NewGuid()));

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
            "file.pdf",
            "application/pdf",
            200);

        _summaryRepository
            .Setup(x => x.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _handler.HandleAsync(new GetMatchSummaryQuery(summary.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(summary.Id);
        result.Value.MatchId.Should().Be(summary.MatchId);
    }
}
