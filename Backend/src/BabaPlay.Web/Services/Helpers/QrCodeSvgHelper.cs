using System;
using System.Text;

namespace BabaPlay.Web.Services.Helpers;

/// <summary>
/// Helper para geracao de representacao visual em SVG de QR Code e Codigo de Barras.
/// </summary>
public static class QrCodeSvgHelper
{
    public static string GenerateQrCodeSvg(string content, int size = 180)
    {
        if (string.IsNullOrWhiteSpace(content)) content = "BABAPLAY-CARD";

        // Gerador deterministico de matriz de quadros para mockup visual de QR Code em SVG
        var hash = content.GetHashCode();
        var rnd = new Random(hash);

        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 25 25\" width=\"{size}\" height=\"{size}\" class=\"qr-code-svg\">");
        sb.AppendLine("  <rect width=\"25\" height=\"25\" fill=\"#ffffff\" rx=\"1\" />");

        // Desenhar padroes dos 3 cantos (Finder Patterns)
        DrawFinderPattern(sb, 1, 1);
        DrawFinderPattern(sb, 17, 1);
        DrawFinderPattern(sb, 1, 17);

        // Preencher pontos aleatorios mas deterministicos baseados no hash da string
        for (int row = 1; row < 24; row++)
        {
            for (int col = 1; col < 24; col++)
            {
                // Ignorar areas dos finder patterns
                if ((row <= 8 && col <= 8) || (row <= 8 && col >= 16) || (row >= 16 && col <= 8))
                    continue;

                if (rnd.Next(100) < 45)
                {
                    sb.AppendLine($"  <rect x=\"{col}\" y=\"{row}\" width=\"0.9\" height=\"0.9\" fill=\"#111827\" rx=\"0.15\" />");
                }
            }
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static void DrawFinderPattern(StringBuilder sb, int x, int y)
    {
        sb.AppendLine($"  <rect x=\"{x}\" y=\"{y}\" width=\"7\" height=\"7\" fill=\"#111827\" rx=\"1\" />");
        sb.AppendLine($"  <rect x=\"{x + 1}\" y=\"{y + 1}\" width=\"5\" height=\"5\" fill=\"#ffffff\" rx=\"0.5\" />");
        sb.AppendLine($"  <rect x=\"{x + 2}\" y=\"{y + 2}\" width=\"3\" height=\"3\" fill=\"#111827\" rx=\"0.3\" />");
    }
}
