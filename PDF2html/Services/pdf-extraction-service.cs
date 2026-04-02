using PDF2html.Models;
using UglyToad.PdfPig;

namespace PDF2html.Services;

public sealed class PdfExtractionService : IPdfExtractor
{
    public Task<IReadOnlyList<TextBlock>> ExtractAsync(string pdfPath, CancellationToken cancellationToken)
    {
        var blocks = new List<TextBlock>();
        using var document = PdfDocument.Open(pdfPath);

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lineGroups = page.GetWords()
                .GroupBy(word => Math.Round(word.BoundingBox.Bottom, 1))
                .OrderByDescending(group => group.Key);

            foreach (var line in lineGroups)
            {
                var orderedWords = line.OrderBy(word => word.BoundingBox.Left).ToList();
                if (orderedWords.Count == 0)
                {
                    continue;
                }

                var text = string.Join(" ", orderedWords.Select(word => word.Text)).Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var fontSizes = orderedWords
                    .SelectMany(word => word.Letters)
                    .Select(letter => letter.PointSize)
                    .Where(size => size > 0)
                    .ToList();

                var averageFont = fontSizes.Count == 0 ? 12 : fontSizes.Average();
                blocks.Add(new TextBlock
                {
                    Text = text,
                    PageNumber = page.Number,
                    X = orderedWords.Min(word => word.BoundingBox.Left),
                    Y = orderedWords.Max(word => word.BoundingBox.Bottom),
                    FontSize = Math.Round(averageFont, 2)
                });
            }
        }

        return Task.FromResult<IReadOnlyList<TextBlock>>(blocks);
    }
}
