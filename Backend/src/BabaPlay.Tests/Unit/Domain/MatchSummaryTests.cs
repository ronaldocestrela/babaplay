using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class MatchSummaryTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveSummary()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant-1/file.pdf",
            "summary-file.pdf",
            "application/pdf",
            1024);

        summary.Id.Should().NotBeEmpty();
        summary.IsActive.Should().BeTrue();
        summary.SizeBytes.Should().Be(1024);
        summary.FileName.Should().Be("summary-file.pdf");
        summary.ContentType.Should().Be("application/pdf");
        summary.StoragePath.Should().Be("match-summaries/tenant-1/file.pdf");
    }

    [Fact]
    public void Create_EmptyTenantId_ShouldThrowValidationException()
    {
        var act = () => MatchSummary.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "match-summaries/tenant-1/file.pdf",
            "summary-file.pdf",
            "application/pdf",
            1024);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyMatchId_ShouldThrowValidationException()
    {
        var act = () => MatchSummary.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "match-summaries/tenant-1/file.pdf",
            "summary-file.pdf",
            "application/pdf",
            1024);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyStoragePath_ShouldThrowValidationException()
    {
        var act = () => MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "   ",
            "summary-file.pdf",
            "application/pdf",
            1024);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var summary = MatchSummary.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "match-summaries/tenant-1/file.pdf",
            "summary-file.pdf",
            "application/pdf",
            1024);

        summary.Deactivate();
        var act = () => summary.Deactivate();

        act.Should().NotThrow();
        summary.IsActive.Should().BeFalse();
    }
}
