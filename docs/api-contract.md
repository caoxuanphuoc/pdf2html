# API Contract & Response Specification

**Last Updated**: April 2, 2026  
**API Version**: v1  
**Base URL**: `/api/v1/documents`

---

## Overview

The PDF2html API provides RESTful endpoints for PDF document processing. The service extracts text content, normalizes layout structure, and returns semantic HTML along with structured block metadata.

---

## Endpoints

### POST /api/v1/documents/upload

Upload a PDF file for processing, extraction, and HTML rendering.

#### Request

**Content-Type**: `multipart/form-data`

**Parameters**:
| Name | Type | Required | Constraints |
|------|------|----------|-------------|
| `file` | File (IFormFile) | Yes | PDF format only, max 20MB |

**cURL Example**:
```bash
curl -X POST http://localhost:5000/api/v1/documents/upload \
  -F "file=@document.pdf"
```

#### Response (Success - 200 OK)

```json
{
  "documentId": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "fileName": "invoice_2026_04.pdf",
  "createdAtUtc": "2026-04-02T14:30:45.123Z",
  "blockCount": 42,
  "structuredBlockCount": 38,
  "htmlContent": "<!DOCTYPE html>...",
  "ocrRequired": false
}
```

**Response Fields**:
| Field | Type | Description | Notes |
|-------|------|-------------|-------|
| `documentId` | string | Unique identifier (GUID) | Format: `[a-z0-9]{32}` (no hyphens) |
| `fileName` | string | Original uploaded filename | HTML-encoded for safety |
| `createdAtUtc` | string (ISO 8601) | Processing timestamp | Example: `2026-04-02T14:30:45.123Z` |
| `blockCount` | integer | Total extracted text blocks | Raw blocks from PDF extraction |
| `structuredBlockCount` | integer | Classified & normalized blocks | After LayoutNormalizationService |
| `htmlContent` | string | Semantic HTML-5 output | Rendered via RenderStructured() |
| `ocrRequired` | boolean | OCR needed (no text extracted) | True if `blockCount == 0` |

#### Response (Validation Errors - 400 Bad Request)

**Missing File**:
```json
{ "error": "File is required." }
```

**Invalid File Type**:
```json
{ "error": "Only PDF files are supported." }
```

**File Too Large**:
```json
{ "error": "File exceeds 20MB limit." }
```

#### Response (Server Error - 500 Internal Server Error)

```json
{ "error": "An error occurred during processing." }
```

**Common Causes**:
- PDF extraction failed (corrupted file, unsupported PDF format)
- File storage failure (disk full, permission denied)
- HTML rendering failure (unexpected block data)

---

## Data Models

### DocumentResult

**Location**: `PDF2html/Models/document-result.cs`

Response envelope containing all processing results.

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

### StructuredDocument

**Location**: `PDF2html/Models/structured-document.cs`

Type-safe representation of a document with semantic structure.

```csharp
public sealed class StructuredDocument
{
    public required string DocumentId { get; init; }
    public required string FileName { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required IReadOnlyList<StructuredBlock> Blocks { get; init; }
    public required bool OcrRequired { get; init; }
}
```

### StructuredBlock

**Location**: `PDF2html/Models/structured-document.cs`

Individual semantic block with type classification.

```csharp
public sealed class StructuredBlock
{
    public required string Text { get; init; }           // HTML-encoded text content
    public required int PageNumber { get; init; }        // 1-based page number
    public required BlockType Type { get; init; }        // Heading1, Heading2, Paragraph, List, Table
    public required double X { get; init; }              // Horizontal position (points)
    public required double Y { get; init; }              // Vertical position (points)
    public required double FontSize { get; init; }       // Font size in points
}
```

### BlockType Enum

**Location**: `PDF2html/Models/structured-document.cs`

Semantic classification of text blocks.

```csharp
public enum BlockType
{
    Heading1,    // FontSize > 16pt
    Heading2,    // 13pt < FontSize ≤ 16pt
    Paragraph,   // FontSize ≤ 13pt (default classification)
    List,        // Future: multi-line list classification
    Table        // Future: table cell classification
}
```

**Classification Logic**:
- **Heading1**: Font size exceeds 16pt (typically titles/main headers)
- **Heading2**: Font size between 13pt and 16pt (typically section headers)
- **Paragraph**: Font size 13pt or less (typical body text)
- **List**: Reserved for multi-line list detection (not yet implemented)
- **Table**: Reserved for table structure detection (not yet implemented)

### TextBlock

**Location**: `PDF2html/Models/text-block.cs`

Raw text block extracted directly from PDF.

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

---

## HTML Output Format

### Structure

The `htmlContent` field contains a complete HTML5 document with semantic markup.

**Template**:
```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>{filename}</title>
  <style>
    /* Responsive typography, list formatting, heading hierarchy */
  </style>
</head>
<body>
  <!-- Semantic markup: h1, h2, p, ul/li -->
</body>
</html>
```

### Semantic Tags

| BlockType | HTML Tag | Example |
|-----------|----------|---------|
| Heading1 | `<h1>` | `<h1>Invoice 2026-04</h1>` |
| Heading2 | `<h2>` | `<h2>Payment Terms</h2>` |
| Paragraph | `<p>` | `<p>Amount due: $1,500.00</p>` |
| List | `<li>` (within `<ul>`) | `<ul><li>Item 1</li><li>Item 2</li></ul>` |

### Security Considerations

- All text content is **HTML-encoded** via `WebUtility.HtmlEncode()`
  - Prevents XSS attacks from untrusted PDF content
  - Example: `<script>` becomes `&lt;script&gt;`
- Filenames are encoded in `<title>` tag
- Safe for direct browser rendering

---

## Processing Pipeline

```
1. File Upload → Validation (type, size)
   ↓
2. PDF Extraction → TextBlock list (PdfExtractionService)
   ↓
3. Layout Normalization → StructuredBlock list (LayoutNormalizationService)
   - Group by page
   - Sort by position (Y desc, X asc)
   - Classify by font size
   ↓
4. HTML Rendering → Semantic HTML (HtmlRenderService.RenderStructured)
   - Map BlockType to HTML tags
   - Wrap lists in <ul>
   - Encode all text for XSS safety
   ↓
5. Store Result → DocumentResult saved (FileDocumentStore)
   ↓
6. Return Response → 200 OK with documentId, htmlContent, blockCount
```

---

## Error Handling

### HTTP Status Codes

| Status | Scenario | Example |
|--------|----------|---------|
| 200 | Successful processing | File uploaded, extracted, rendered |
| 400 | Validation failure | Missing file, invalid format, too large |
| 500 | Server error | PDF extraction failure, storage failure |

### Client Responsibilities
- Validate file type before upload (optional, validated on server)
- Handle 400 errors gracefully (show user feedback)
- Retry 500 errors with exponential backoff
- Cache `documentId` for future reference/audit trail

---

## Performance Expectations

### Request Latency
- **P50**: 1-2s per file (typical 5-page PDF)
- **P95**: 4-6s per file (larger 10-page PDF)
- **P99**: 8-10s per file (edge case, many small blocks)

### Constraints
- **Max file size**: 20MB
- **Concurrent requests**: Depends on deployment (dev/testing: 1-5, production: scale horizontally)
- **Block limit**: No artificial limit; performance degrades with 100k+ blocks

---

## Backward Compatibility

### Current Version (v1)
- API path: `/api/v1/documents`
- Supports: Upload, extraction, HTML rendering
- Response: DocumentResult with htmlContent, blockCount

### Future Enhancements (v2+)
- JSON export of StructuredBlock list
- Batch processing endpoints
- Async processing with webhooks
- Advanced filtering/search by BlockType

---

## Related Documentation
- [System Architecture](./system-architecture.md) - Service interactions
- [Test Strategy](./test-strategy.md) - API validation tests
- [Implementation Progress](./implementation-progress.md) - Feature status
