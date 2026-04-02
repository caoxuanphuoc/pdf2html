using PDF2html.Models;

namespace PDF2html.Services;

public interface IHtmlRenderer
{
    string Render(string sourceFileName, IReadOnlyList<TextBlock> blocks);

    string RenderStructured(string sourceFileName, IReadOnlyList<StructuredBlock> blocks);
}
