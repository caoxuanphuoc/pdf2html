#nullable enable
using Xunit;
using PDF2html.Models;
using PDF2html.Services;

namespace PDF2html.Tests;

public sealed class PdfQualityTests
{
    private readonly LayoutNormalizationService _normalizationService = new();

    [Fact]
    public void Normalize_ShouldClassifyHeading1_WhenFontSizeGreaterThan16()
    {
        var blocks = new List<TextBlock>
        {
            new()
            {
                Text = "Main Title",
                PageNumber = 1,
                X = 100,
                Y = 700,
                FontSize = 24
            }
        };

        var result = _normalizationService.Normalize(blocks);

        Assert.Single(result);
        Assert.Equal(BlockType.Heading1, result[0].Type);
    }

    [Fact]
    public void Normalize_ShouldClassifyHeading2_WhenFontSizeBetween13And16()
    {
        var blocks = new List<TextBlock>
        {
            new()
            {
                Text = "Subheading",
                PageNumber = 1,
                X = 100,
                Y = 650,
                FontSize = 14
            }
        };

        var result = _normalizationService.Normalize(blocks);

        Assert.Single(result);
        Assert.Equal(BlockType.Heading2, result[0].Type);
    }

    [Fact]
    public void Normalize_ShouldClassifyParagraph_WhenFontSizeBelow13()
    {
        var blocks = new List<TextBlock>
        {
            new()
            {
                Text = "Body text content",
                PageNumber = 1,
                X = 100,
                Y = 600,
                FontSize = 11
            }
        };

        var result = _normalizationService.Normalize(blocks);

        Assert.Single(result);
        Assert.Equal(BlockType.Paragraph, result[0].Type);
    }

    [Fact]
    public void Normalize_ShouldMaintainPageOrder()
    {
        var blocks = new List<TextBlock>
        {
            new() { Text = "Page 1", PageNumber = 1, X = 100, Y = 700, FontSize = 12 },
            new() { Text = "Page 2", PageNumber = 2, X = 100, Y = 700, FontSize = 12 },
            new() { Text = "Page 1 Bottom", PageNumber = 1, X = 100, Y = 100, FontSize = 12 }
        };

        var result = _normalizationService.Normalize(blocks);

        Assert.Equal(3, result.Count);
        Assert.All(result.Take(2), b => Assert.Equal(1, b.PageNumber));
        Assert.Equal(2, result.Last().PageNumber);
    }
}
