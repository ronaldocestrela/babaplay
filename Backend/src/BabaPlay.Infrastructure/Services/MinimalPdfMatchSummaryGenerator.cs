using System.Text;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Infrastructure.Services;

/// <summary>
/// Generates a minimal valid PDF document with plain text lines for the match summary.
/// </summary>
public sealed class MinimalPdfMatchSummaryGenerator : IMatchSummaryPdfGenerator
{
    public Task<byte[]> GenerateAsync(MatchSummaryPdfInput input, CancellationToken ct = default)
    {
        var lines = new List<string>
        {
            "BabaPlay Match Summary",
            $"Match: {input.MatchId}",
            $"GameDay: {input.GameDayId}",
            $"HomeTeam: {input.HomeTeamId}",
            $"AwayTeam: {input.AwayTeamId}",
            $"GeneratedAtUtc: {input.GeneratedAtUtc:O}",
        };

        if (!string.IsNullOrWhiteSpace(input.Description))
            lines.Add($"Description: {input.Description}");

        lines.Add("Events:");
        lines.AddRange(input.Events.Select(e =>
            $"{e.Minute}' Team={e.TeamId} Player={e.PlayerId} Type={e.MatchEventTypeId} Notes={e.Notes}"));

        var escapedLines = lines
            .Select(EscapePdfText)
            .Select((line, idx) => idx == 0 ? $"({line}) Tj" : $"0 -14 Td ({line}) Tj")
            .ToArray();

        var contentStream = "BT\n/F1 12 Tf\n72 760 Td\n" + string.Join("\n", escapedLines) + "\nET";
        var contentBytes = Encoding.ASCII.GetBytes(contentStream);

        var objects = new List<string>
        {
            "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
            "2 0 obj\n<< /Type /Pages /Count 1 /Kids [3 0 R] >>\nendobj\n",
            "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n",
            "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n",
            $"5 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n{contentStream}\nendstream\nendobj\n",
        };

        var output = new StringBuilder();
        output.Append("%PDF-1.4\n");

        var xrefPositions = new List<int> { 0 };
        foreach (var obj in objects)
        {
            xrefPositions.Add(output.Length);
            output.Append(obj);
        }

        var xrefStart = output.Length;
        output.Append($"xref\n0 {xrefPositions.Count}\n");
        output.Append("0000000000 65535 f \n");

        for (var i = 1; i < xrefPositions.Count; i++)
            output.Append($"{xrefPositions[i]:D10} 00000 n \n");

        output.Append("trailer\n");
        output.Append($"<< /Size {xrefPositions.Count} /Root 1 0 R >>\n");
        output.Append("startxref\n");
        output.Append($"{xrefStart}\n");
        output.Append("%%EOF");

        return Task.FromResult(Encoding.ASCII.GetBytes(output.ToString()));
    }

    private static string EscapePdfText(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}
