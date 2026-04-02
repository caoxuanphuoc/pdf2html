using System.Text.Json;
using PDF2html.Models;

namespace PDF2html.Stores;

public sealed class FileDocumentStore : IDocumentStore
{
    private readonly string _resultDirectory;

    public FileDocumentStore(IWebHostEnvironment environment)
    {
        _resultDirectory = Path.Combine(environment.ContentRootPath, "storage", "results");
        Directory.CreateDirectory(_resultDirectory);
    }

    public async Task SaveAsync(DocumentResult documentResult, CancellationToken cancellationToken)
    {
        var filePath = GetResultPath(documentResult.DocumentId);
        var json = JsonSerializer.Serialize(documentResult, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task<DocumentResult?> GetAsync(string documentId, CancellationToken cancellationToken)
    {
        var filePath = GetResultPath(documentId);
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<DocumentResult>(json);
    }

    private string GetResultPath(string documentId)
    {
        var safeId = documentId.Replace("..", string.Empty, StringComparison.Ordinal);
        return Path.Combine(_resultDirectory, $"{safeId}.json");
    }
}
