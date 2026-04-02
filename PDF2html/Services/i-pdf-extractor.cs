using PDF2html.Models;

namespace PDF2html.Services;

public interface IPdfExtractor
{
    Task<IReadOnlyList<TextBlock>> ExtractAsync(string pdfPath, CancellationToken cancellationToken);
}
