# System Architecture

**Last Updated**: April 2, 2026  
**Version**: Phase 2 (Core implementation complete)  
**Technology Stack**: ASP.NET Core 9, C# 11, UglyToad PDF Library

---

## High-Level Overview

```
┌─────────────────────────────────────────────────────┐
│                    API Client                        │
└────────────────────┬──────────────────────────────────┘
                     │ POST /api/v1/documents/upload
                     │
┌────────────────────▼──────────────────────────────────┐
│            DocumentController (API Layer)            │
│  • File validation                                   │
│  • Orchestrates extraction → normalization → render  │
└────────────────────┬──────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
┌──────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ PDF          │ │ Layout           │ │ HTML Render      │
│ Extraction   │ │ Normalization    │ │ Service          │
│ Service      │ │ Service          │ │                  │
│              │ │                  │ │ • RenderStructured│
│ • Extract    │ │ • Group by page  │ │ • Semantic HTML  │
│   TextBlock  │ │ • Sort coords    │ │ • XSS protection │
│ • Page #,    │ │ • Classify by    │ │                  │
│   X, Y, Font│ │   font size      │ │                  │
└──────────────┘ └──────────────────┘ └──────────────────┘
        │            │
        │            │
        └────────────┴─────────┬─────────┐
                               │         │
                               ▼         ▼
                        ┌────────────┐ ┌────────────┐
                        │ Models      │ │ Services   │
                        │             │ │ (Interfaces)
                        │ • TextBlock  │ │            │
                        │ • Structured│ │ • IHtmlRend│
                        │   Block     │ │ • IDoc     │
                        │ • BlockType │ │   Store    │
                        │             │ │ • IPdfExt  │
                        └────────────┘ └────────────┘
                               │
                               ▼
                        ┌────────────┐
                        │ Document   │
                        │ Store      │
                        │            │
                        │ • Save     │
                        │ • Retrieve │
                        │ • FileIO   │
                        └────────────┘
```

---

## Component Details

### 1. DocumentController (API Layer)

**File**: `PDF2html/Controllers/document-controller.cs`  
**Responsibility**: HTTP request handling, orchestration, validation

#### Key Methods
- `Upload(IFormFile file)` - Main API endpoint
  - Validates file type (PDF only)
  - Validates file size (max 20MB)
  - Orchestrates the extraction → normalization → rendering pipeline
  - Returns DocumentResult with processed content

#### Dependencies (Injected)
- `IPdfExtractor` - Extract text from PDF files
- `IHtmlRenderer` - Render semantic HTML
- `LayoutNormalizationService` - Normalize extracted blocks
- `IDocumentStore` - Persist results
- `IWebHostEnvironment` - Access file system paths
- `ILogger<DocumentController>` - Structured logging

#### Flow
```
POST /api/v1/documents/upload (file: PDF)
  │
  ├─ Validate file (size, type)
  │
  ├─ Save uploaded file to storage/uploads/
  │
  ├─ Extract blocks from PDF
  │    pdfExtractor.ExtractAsync(uploadPath)
  │    Returns: List<TextBlock>
  │
  ├─ Normalize layout structure
  │    normalizationService.Normalize(blocks)
  │    Returns: List<StructuredBlock>
  │
  ├─ Render semantic HTML
  │    htmlRenderer.RenderStructured(fileName, structured)
  │    Returns: HTML string
  │
  ├─ Create DocumentResult
  │    {documentId, fileName, createdAtUtc, htmlContent, blocks, ocrRequired}
  │
  ├─ Store result
  │    documentStore.SaveAsync(result)
  │
  └─ Return 200 OK
       {documentId, blockCount, structuredBlockCount, ...}
```

---

### 2. PDF Extraction Service

**File**: `PDF2html/Services/pdf-extraction-service.cs`  
**Interface**: `PDF2html/Services/i-pdf-extractor.cs`  
**Responsibility**: Extract text content and metadata from PDF files

#### Key Method
- `ExtractAsync(string filePath, CancellationToken ct) → Task<IReadOnlyList<TextBlock>>`

#### Implementation Details
- Uses **UglyToad** library for PDF parsing
- Extracts per-character bounding box coordinates
- Groups characters into TextBlock objects
- Captures metadata: PageNumber, X, Y (position), FontSize
- Handles multi-page documents
- Thread-safe async operation

#### Output Example
```csharp
new TextBlock {
    Text = "Invoice #2026-04",
    PageNumber = 1,
    X = 100.5,
    Y = 750.0,
    FontSize = 24.0
}
```

---

### 3. Layout Normalization Service

**File**: `PDF2html/Services/layout-normalization-service.cs`  
**Responsibility**: Transform raw TextBlocks into semantic StructuredBlocks

#### Algorithm

```
Input: List<TextBlock> (raw, unsorted)
  │
  ├─ GROUP BY PageNumber
  │    Organized by page for multi-page documents
  │
  ├─ FOR EACH PAGE:
  │    │
  │    ├─ SORT BY Y coordinate (descending)
  │    │    Top-to-bottom reading order
  │    │    (PDF Y=0 at bottom, reverse sort gives top-first)
  │    │
  │    ├─ SUB-SORT BY X coordinate (ascending)
  │    │    Left-to-right reading order within same Y level
  │    │
  │    └─ CLASSIFY EACH BLOCK
  │         FontSize switch:
  │         • > 16pt → Heading1
  │         • > 13pt → Heading2
  │         • ≤ 13pt → Paragraph
  │
  └─ Output: List<StructuredBlock> (normalized, classified)
```

#### Classification Thresholds

| BlockType | Font Size Threshold | Use Case |
|-----------|-------------------|----------|
| Heading1 | > 16pt | Document titles, chapter headers |
| Heading2 | 13pt - 16pt | Section headers, subsections |
| Paragraph | ≤ 13pt | Body text, content |
| List | TBD | Multi-line list detection (future) |
| Table | TBD | Table cell detection (future) |

#### Key Property: Immutability
- All properties use `required` keyword (C# 11)
- Sealed class prevents inheritance
- Immutable init-only setters
- Thread-safe, no state mutations

---

### 4. HTML Render Service

**File**: `PDF2html/Services/html-render-service.cs`  
**Interface**: `PDF2html/Services/i-html-renderer.cs`  
**Responsibility**: Generate semantic HTML-5 output

#### Methods

##### Render() (Phase 1)
- Takes raw TextBlock list
- Generates basic HTML with h1, h2, p tags
- Simple font-size-based mapping
- **Used for**: Legacy support, backup rendering

##### RenderStructured() (Phase 2)
- Takes classified StructuredBlock list
- Generates semantic HTML with proper structure
- Implements intelligent list wrapping
- Maps BlockType → HTML tags
- **Used for**: Production rendering

#### Semantic Tag Mapping

```csharp
BlockType.Heading1   → <h1>{text}</h1>
BlockType.Heading2   → <h2>{text}</h2>
BlockType.List       → <li>{text}</li> (wrapped in <ul>)
BlockType.Paragraph  → <p>{text}</p>
BlockType.Table      → <p>{text}</p> (placeholder until table detection)
```

#### List Handling Algorithm

```
inList = false

For each block in order:
  If BlockType == List AND not inList:
    output <ul>
    inList = true
  
  If BlockType != List AND inList:
    output </ul>
    inList = false
  
  output <li> or <h1>/<h2>/<p> based on type

After loop:
  If inList still true:
    output </ul>
```

#### Security: XSS Protection

```csharp
// All text content is encoded before output
var text = WebUtility.HtmlEncode(block.Text);
// Example: "<script>" → "&lt;script&gt;"
```

#### HTML Template

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>{filename}</title>
  <style>
    body { font-family: Segoe UI, Tahoma, sans-serif; margin: 2rem; }
    h1 { font-size: 2rem; margin: 1.5rem 0 .5rem; }
    h2 { font-size: 1.5rem; margin: 1rem 0 .4rem; }
    p { margin: .4rem 0; }
    ul { margin: .5rem 0; padding-left: 1.5rem; }
    li { margin: .2rem 0; }
  </style>
</head>
<body>
  <!-- Semantic content generated here -->
</body>
</html>
```

---

### 5. Document Store (Persistence Layer)

**File**: `PDF2html/Stores/file-document-store.cs`  
**Interface**: `PDF2html/Stores/i-document-store.cs`  
**Responsibility**: Persist and retrieve processed documents

#### Key Methods
- `SaveAsync(DocumentResult result, CancellationToken ct)` - Store result
- `RetrieveAsync(string documentId, CancellationToken ct)` - Retrieve by ID
- `ListAsync(CancellationToken ct)` - List all documents

#### Storage Structure
```
storage/
├── uploads/
│   └── {documentId} (original PDF)
└── results/
    └── {documentId}.json (DocumentResult serialized)
```

#### Implementation
- File-based JSON serialization
- Creates directories automatically
- Thread-safe for concurrent requests
- Suitable for development/small-scale deployments

---

### 6. Models (Data Contracts)

#### TextBlock
**File**: `PDF2html/Models/text-block.cs`

```csharp
public sealed class TextBlock
{
    public required string Text { get; init; }
    public required int PageNumber { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public required double FontSize { get; init; }
}
```
- **Source**: PdfExtractionService
- **Usage**: Intermediate representation between extraction and normalization

#### StructuredBlock & BlockType
**File**: `PDF2html/Models/structured-document.cs`

```csharp
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
    Heading1, Heading2, Paragraph, List, Table
}
```
- **Source**: LayoutNormalizationService
- **Usage**: Classified blocks for rendering and analysis
- **Properties**: All required (null-safety)

#### DocumentResult
**File**: `PDF2html/Models/document-result.cs`

```csharp
public sealed class DocumentResult
{
    public required string DocumentId { get; init; }
    public required string FileName { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string HtmlContent { get; init; }
    public required IReadOnlyList<TextBlock> Blocks { get; init; }
    public required bool OcrRequired { get; init; }
}
```
- **Source**: DocumentController after rendering
- **Usage**: API response, stored for audit trail
- **Consumers**: Clients, audit logs, future metrics engine

---

## Dependency Injection (DI) Configuration

**File**: `PDF2html/Program.cs`

```csharp
builder.Services.AddSingleton<IPdfExtractor, PdfExtractionService>();
builder.Services.AddSingleton<IHtmlRenderer, HtmlRenderService>();
builder.Services.AddSingleton<LayoutNormalizationService>();
builder.Services.AddSingleton<IDocumentStore, FileDocumentStore>();
```

### Lifetime Strategy
- **Singleton**: All services are stateless, safe for concurrent requests
- **Benefits**:
  - Single instance per application lifetime
  - Reduced memory footprint
  - Fast dependency resolution
  - Thread-safe by design (no mutable state)

---

## Data Flow Diagram

```
PDF File Upload
    │
    ▼
┌─────────────────────┐
│ DocumentController  │ ← File validation, orchestration
└─────────────────────┘
    │
    │ (1) Extract
    ▼
┌─────────────────────────────┐
│ PdfExtractionService        │
│ Input: PDF binary           │
│ Output: TextBlock list      │
└─────────────────────────────┘
    │
    │ (2) Normalize
    ▼
┌──────────────────────────────┐
│ LayoutNormalizationService   │
│ Input: TextBlock list        │
│ Output: StructuredBlock list │
└──────────────────────────────┘
    │
    │ (3) Render
    ▼
┌──────────────────────────┐
│ HtmlRenderService        │
│ Input: StructuredBlock   │
│ Output: HTML string      │
└──────────────────────────┘
    │
    │ (4) Store
    ▼
┌──────────────────────────┐
│ DocumentStore            │
│ Input: DocumentResult    │
│ Output: Persisted file   │
└──────────────────────────┘
    │
    │ (5) Return
    ▼
API Response (200 OK)
  {documentId, blockCount, htmlContent, ...}
```

---

## Technology Choices

### PDF Library: UglyToad
- **Rationale**: Pure C# implementation, no native dependencies
- **Capabilities**: Text extraction, coordinate data, font metrics
- **Limitations**: No table detection, no image extraction
- **Future**: May need specialized library for advanced document types

### Persistence: File-based JSON
- **Rationale**: Simple, no external database dependencies
- **Suitable for**: MVP, development, small-scale deployments
- **Limitations**: I/O bottleneck at scale, no indexing
- **Future**: Migrate to database (PostgreSQL, MongoDB) for Phase 3+

### Web Framework: ASP.NET Core
- **Rationale**: Type-safe, built-in DI, modern C# language features
- **Benefits**: MVC pattern, middleware pipeline, performance
- **Version**: .NET 9 (LTS support until November 2028)

---

## Performance Characteristics

### Extraction Phase
- **Bottleneck**: PDF parsing (I/O + computation)
- **Time**: 0.5-3s per file (depends on page count, block density)
- **Scaling**: CPU-bound, parallelizable by file, not within file

### Normalization Phase
- **Bottleneck**: Sort operation (O(n log n))
- **Time**: <100ms for typical documents (hundreds of blocks)
- **Scaling**: Linear with block count, negligible overhead

### Rendering Phase
- **Bottleneck**: String building (StringBuilder + encoding)
- **Time**: <50ms for typical documents
- **Scaling**: Linear with block count, negligible overhead

### Storage Phase
- **Bottleneck**: File I/O (disk write)
- **Time**: 50-200ms per operation
- **Scaling**: Disk I/O bound; use fast storage (SSD)

### Overall P95 Latency
- **Typical 5-page PDF**: 2-4 seconds
- **Larger 10-page PDF**: 4-6 seconds
- **Edge case 50-page PDF**: 10-15 seconds

---

## Scalability Considerations

### Current Limitations
- File-based storage: Single-server only
- Synchronous processing: Client blocks until complete
- No request queuing: Concurrent requests share resources

### Phase 3+ Improvements
- Database migration: Multi-server support
- Async processing: Webhooks for completion notification
- Message queue: Decouple upload from processing
- Horizontal scaling: Multiple processor instances

---

## Related Documentation
- [API Contract](./api-contract.md) - Endpoint specifications
- [Test Strategy](./test-strategy.md) - Testing approach
- [Implementation Progress](./implementation-progress.md) - Feature status
