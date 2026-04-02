namespace PDF2html.Models;

public sealed class TextBlock
{
    public required string Text { get; init; }

    public required int PageNumber { get; init; }

    public required double X { get; init; }

    public required double Y { get; init; }

    public required double FontSize { get; init; }
}
