using PDF2html.Models;

namespace PDF2html.Stores;

public interface IDocumentStore
{
    Task SaveAsync(DocumentResult documentResult, CancellationToken cancellationToken);

    Task<DocumentResult?> GetAsync(string documentId, CancellationToken cancellationToken);
}
