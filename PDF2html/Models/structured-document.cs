namespace PDF2html.Models;

public sealed class StructuredDocument
{
    public required string DocumentId { get; init; }

    public required string FileName { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required IReadOnlyList<StructuredBlock> Blocks { get; init; }

    public required bool OcrRequired { get; init; }
}

public sealed class StructuredBlock
{
    public required string Text { get; init; }

    public required int PageNumber { get; init; }

    public required BlockType Type { get; init; }

    public required double X { get; init; }

    public required double Y { get; init; }

    public required double FontSize { get; init; }
}

public enum BlockType
{
    Heading1,
    Heading2,
    Paragraph,
    List,
    Table
}
