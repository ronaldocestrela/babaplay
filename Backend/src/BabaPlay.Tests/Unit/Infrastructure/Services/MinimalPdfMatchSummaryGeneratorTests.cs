using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Services;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public class MinimalPdfMatchSummaryGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ValidInput_ShouldReturnPdfBytes()
    {
        var sut = new MinimalPdfMatchSummaryGenerator();

        var bytes = await sut.GenerateAsync(CreateInput("normal notes"));
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        bytes.Should().NotBeEmpty();
        content.Should().Contain("%PDF-1.4");
        content.Should().Contain("BabaPlay Match Summary");
    }

    [Fact]
    public async Task GenerateAsync_SpecialCharacters_ShouldEscapePdfText()
    {
        var sut = new MinimalPdfMatchSummaryGenerator();

        var bytes = await sut.GenerateAsync(CreateInput("goal (great) \\ decisive"));
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        content.Should().Contain("goal \\(great\\) \\\\ decisive");
    }

    private static MatchSummaryPdfInput CreateInput(string notes) => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Final match",
        DateTime.UtcNow,
        [new MatchSummaryPdfEventItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 87, notes)]);
}
