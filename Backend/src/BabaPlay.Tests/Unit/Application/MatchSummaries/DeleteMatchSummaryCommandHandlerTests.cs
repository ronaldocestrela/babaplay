using BabaPlay.Application.Commands.MatchSummaries;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchSummaries;

public class DeleteMatchSummaryCommandHandlerTests
{
    private readonly Mock<IMatchSummaryRepository> _summaryRepository = new();
    private readonly Mock<IMatchSummaryStorageService> _storageService = new();
    private readonly DeleteMatchSummaryCommandHandler _handler;

    public DeleteMatchSummaryCommandHandlerTests()
    {
        _handler = new DeleteMatchSummaryCommandHandler(_summaryRepository.Object, _storageService.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _summaryRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchSummary?)null);

        var result = await _handler.HandleAsync(new DeleteMatchSummaryCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_SUMMARY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_StorageDeleteFails_ShouldReturnError()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant/file.pdf",
            "file.pdf",
            "application/pdf",
            120);

        _summaryRepository
            .Setup(x => x.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _storageService
            .Setup(x => x.DeleteAsync(summary.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new DeleteMatchSummaryCommand(summary.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_SUMMARY_FILE_DELETE_FAILED");
        summary.IsActive.Should().BeTrue();
        _summaryRepository.Verify(x => x.UpdateAsync(It.IsAny<MatchSummary>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Found_ShouldDeactivateAndUpdate()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant/file.pdf",
            "file.pdf",
            "application/pdf",
            120);

        _summaryRepository
            .Setup(x => x.GetByIdAsync(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _storageService
            .Setup(x => x.DeleteAsync(summary.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new DeleteMatchSummaryCommand(summary.Id));

        result.IsSuccess.Should().BeTrue();
        summary.IsActive.Should().BeFalse();
        _summaryRepository.Verify(x => x.UpdateAsync(summary, It.IsAny<CancellationToken>()), Times.Once);
    }
}
