using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchSummaries;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchSummaries;

public class GetMatchSummaryFileQueryHandlerTests
{
    private readonly Mock<IMatchSummaryRepository> _summaryRepository = new();
    private readonly Mock<IMatchSummaryStorageService> _storageService = new();
    private readonly GetMatchSummaryFileQueryHandler _handler;

    public GetMatchSummaryFileQueryHandlerTests()
    {
        _handler = new GetMatchSummaryFileQueryHandler(_summaryRepository.Object, _storageService.Object);
    }

    [Fact]
    public async Task Handle_SummaryNotFound_ShouldReturnError()
    {
        _summaryRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchSummary?)null);

        var result = await _handler.HandleAsync(new GetMatchSummaryFileQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_SUMMARY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldReturnError()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant/file.pdf",
            "file.pdf",
            "application/pdf",
            100);

        _summaryRepository
            .Setup(x => x.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _storageService
            .Setup(x => x.ReadAsync(summary.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _handler.HandleAsync(new GetMatchSummaryFileQuery(summary.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_SUMMARY_FILE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_FileFound_ShouldReturnResponse()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant/file.pdf",
            "file.pdf",
            "application/pdf",
            100);

        _summaryRepository
            .Setup(x => x.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _storageService
            .Setup(x => x.ReadAsync(summary.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2, 3]);

        var result = await _handler.HandleAsync(new GetMatchSummaryFileQuery(summary.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileName.Should().Be(summary.FileName);
        result.Value.ContentType.Should().Be(summary.ContentType);
        result.Value.Content.Should().Equal([1, 2, 3]);
    }
}
