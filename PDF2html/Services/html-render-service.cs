using System.Net;
using System.Text;
using PDF2html.Models;

namespace PDF2html.Services;

public sealed class HtmlRenderService : IHtmlRenderer
{
    public string Render(string sourceFileName, IReadOnlyList<TextBlock> blocks)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine($"  <title>{WebUtility.HtmlEncode(sourceFileName)}</title>");
        html.AppendLine("  <style>body{font-family:Segoe UI,Tahoma,sans-serif;margin:2rem;line-height:1.5;}h1{font-size:2rem;margin:1.5rem 0 .5rem;}h2{font-size:1.5rem;margin:1rem 0 .4rem;}p{margin:.4rem 0;}</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        foreach (var block in blocks.OrderBy(b => b.PageNumber).ThenByDescending(b => b.Y))
        {
            var text = WebUtility.HtmlEncode(block.Text);
            if (block.FontSize > 16)
            {
                html.AppendLine($"  <h1>{text}</h1>");
                continue;
            }

            if (block.FontSize > 13)
            {
                html.AppendLine($"  <h2>{text}</h2>");
                continue;
            }

            html.AppendLine($"  <p>{text}</p>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    public string RenderStructured(string sourceFileName, IReadOnlyList<StructuredBlock> blocks)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine($"  <title>{WebUtility.HtmlEncode(sourceFileName)}</title>");
        html.AppendLine("  <style>body{font-family:Segoe UI,Tahoma,sans-serif;margin:2rem;line-height:1.5;}h1{font-size:2rem;margin:1.5rem 0 .5rem;}h2{font-size:1.5rem;margin:1rem 0 .4rem;}p{margin:.4rem 0;}ul{margin:.5rem 0;padding-left:1.5rem;}li{margin:.2rem 0;}</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        var inList = false;
        foreach (var block in blocks.OrderBy(b => b.PageNumber).ThenByDescending(b => b.Y))
        {
            var text = WebUtility.HtmlEncode(block.Text);

            if (block.Type == BlockType.List && !inList)
            {
                html.AppendLine("  <ul>");
                inList = true;
            }
            else if (block.Type != BlockType.List && inList)
            {
                html.AppendLine("  </ul>");
                inList = false;
            }

            var htmlLine = block.Type switch
            {
                BlockType.Heading1 => $"  <h1>{text}</h1>",
                BlockType.Heading2 => $"  <h2>{text}</h2>",
                BlockType.List => $"    <li>{text}</li>",
                _ => $"  <p>{text}</p>"
            };
            html.AppendLine(htmlLine);
        }

        if (inList)
        {
            html.AppendLine("  </ul>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }
}
