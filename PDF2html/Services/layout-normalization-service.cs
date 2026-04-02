using PDF2html.Models;

namespace PDF2html.Services;

public sealed class LayoutNormalizationService
{
    public IReadOnlyList<StructuredBlock> Normalize(IReadOnlyList<TextBlock> blocks)
    {
        var normalized = new List<StructuredBlock>();

        var groupedByPage = blocks
            .GroupBy(block => block.PageNumber)
            .OrderBy(group => group.Key);

        foreach (var pageGroup in groupedByPage)
        {
            var pageBlocks = pageGroup
                .OrderByDescending(block => block.Y)
                .ThenBy(block => block.X)
                .ToList();

            normalized.AddRange(pageBlocks.Select(block => ClassifyBlock(block)));
        }

        return normalized;
    }

    private static StructuredBlock ClassifyBlock(TextBlock block)
    {
        var type = block.FontSize switch
        {
            > 16 => BlockType.Heading1,
            > 13 => BlockType.Heading2,
            _ => BlockType.Paragraph
        };

        return new StructuredBlock
        {
            Text = block.Text,
            PageNumber = block.PageNumber,
            Type = type,
            X = block.X,
            Y = block.Y,
            FontSize = block.FontSize
        };
    }
}
