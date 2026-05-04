using BabaPlay.Application.Commands.MatchSummaries;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.MatchSummaries;

public class GenerateMatchSummaryCommandHandlerTests
{
    private readonly Mock<IMatchSummaryRepository> _summaryRepository = new();
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IMatchEventRepository> _matchEventRepository = new();
    private readonly Mock<IMatchSummaryPdfGenerator> _pdfGenerator = new();
    private readonly Mock<IMatchSummaryStorageService> _storageService = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly GenerateMatchSummaryCommandHandler _handler;

    public GenerateMatchSummaryCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());

        _handler = new GenerateMatchSummaryCommandHandler(
            _summaryRepository.Object,
            _matchRepository.Object,
            _matchEventRepository.Object,
            _pdfGenerator.Object,
            _storageService.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ShouldReturnNotFound()
    {
        _matchRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new GenerateMatchSummaryCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_MatchNotCompleted_ShouldReturnValidationError()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        _matchRepository
            .Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        var result = await _handler.HandleAsync(new GenerateMatchSummaryCommand(match.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_COMPLETED");
    }

    [Fact]
    public async Task Handle_SummaryAlreadyExists_ShouldReturnConflict()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        match.ChangeStatus(MatchStatus.Scheduled);
        match.ChangeStatus(MatchStatus.InProgress);
        match.ChangeStatus(MatchStatus.Completed);

        _matchRepository
            .Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        _summaryRepository
            .Setup(x => x.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MatchSummary.Create(
                Guid.NewGuid(),
                match.Id,
                "match-summaries/tenant/file.pdf",
                "file.pdf",
                "application/pdf",
                200));

        var result = await _handler.HandleAsync(new GenerateMatchSummaryCommand(match.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_SUMMARY_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldPersistSummary()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "final");
        match.ChangeStatus(MatchStatus.Scheduled);
        match.ChangeStatus(MatchStatus.InProgress);
        match.ChangeStatus(MatchStatus.Completed);

        _matchRepository
            .Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        _summaryRepository
            .Setup(x => x.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchSummary?)null);

        _matchEventRepository
            .Setup(x => x.GetActiveByMatchAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _pdfGenerator
            .Setup(x => x.GenerateAsync(It.IsAny<MatchSummaryPdfInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2, 3]);

        _storageService
            .Setup(x => x.SaveAsync(It.IsAny<MatchSummaryFileSaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchSummaryStoredFile(
                "match-summaries/tenant/file.pdf",
                "summary-file.pdf",
                "application/pdf",
                3));

        var result = await _handler.HandleAsync(new GenerateMatchSummaryCommand(match.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MatchId.Should().Be(match.Id);
        _summaryRepository.Verify(x => x.AddAsync(It.IsAny<MatchSummary>(), It.IsAny<CancellationToken>()), Times.Once);
        _summaryRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
