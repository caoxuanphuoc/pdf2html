using Microsoft.AspNetCore.Mvc;
using PDF2html.Models;
using PDF2html.Services;
using PDF2html.Stores;

namespace PDF2html.Controllers;

[ApiController]
[Route("api/v1/documents")]
public sealed class DocumentController : ControllerBase
{
    private const long MaxUploadBytes = 20 * 1024 * 1024;

    private readonly IPdfExtractor _pdfExtractor;
    private readonly IHtmlRenderer _htmlRenderer;
    private readonly LayoutNormalizationService _normalizationService;
    private readonly IDocumentStore _documentStore;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IPdfExtractor pdfExtractor,
        IHtmlRenderer htmlRenderer,
        LayoutNormalizationService normalizationService,
        IDocumentStore documentStore,
        IWebHostEnvironment environment,
        ILogger<DocumentController> logger)
    {
        _pdfExtractor = pdfExtractor;
        _htmlRenderer = htmlRenderer;
        _normalizationService = normalizationService;
        _documentStore = documentStore;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (file.Length > MaxUploadBytes)
        {
            return BadRequest(new { error = "File exceeds 20MB limit." });
        }

        if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only PDF files are supported." });
        }

        var documentId = Guid.NewGuid().ToString("N");
        var uploadPath = await SaveUploadAsync(documentId, file, cancellationToken);

        try
        {
            var blocks = await _pdfExtractor.ExtractAsync(uploadPath, cancellationToken);
            var structured = _normalizationService.Normalize(blocks);
            var html = _htmlRenderer.RenderStructured(file.FileName, structured);

            var result = new DocumentResult
            {
                DocumentId = documentId,
                FileName = file.FileName,
                CreatedAtUtc = DateTime.UtcNow,
                HtmlContent = html,
                Blocks = blocks,
                OcrRequired = blocks.Count == 0
            };

            await _documentStore.SaveAsync(result, cancellationToken);
            return Ok(new
            {
                documentId,
                result.FileName,
                result.CreatedAtUtc,
                blockCount = blocks.Count,
                structuredBlockCount = structured.Count,
                result.OcrRequired
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process document {DocumentId}", documentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to process PDF file." });
        }
    }

    [HttpGet("{documentId}")]
    public async Task<IActionResult> GetResult(string documentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return BadRequest(new { error = "Document id is required." });
        }

        var result = await _documentStore.GetAsync(documentId, cancellationToken);
        return result is null ? NotFound(new { error = "Document not found." }) : Ok(result);
    }

    private async Task<string> SaveUploadAsync(string documentId, IFormFile file, CancellationToken cancellationToken)
    {
        var uploadDirectory = Path.Combine(_environment.ContentRootPath, "storage", "uploads");
        Directory.CreateDirectory(uploadDirectory);

        var uploadPath = Path.Combine(uploadDirectory, $"{documentId}.pdf");
        await using var stream = System.IO.File.Create(uploadPath);
        await file.CopyToAsync(stream, cancellationToken);
        return uploadPath;
    }
}
