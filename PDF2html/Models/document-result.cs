namespace PDF2html.Models;

public sealed class DocumentResult
{
    public required string DocumentId { get; init; }

    public required string FileName { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required string HtmlContent { get; init; }

    public required IReadOnlyList<TextBlock> Blocks { get; init; }

    public required bool OcrRequired { get; init; }
}
