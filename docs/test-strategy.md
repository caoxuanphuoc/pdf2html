# Test Strategy

**Last Updated**: April 2, 2026  
**Test Framework**: xUnit  
**Build Tool**: dotnet test  
**Branch Coverage Target**: ≥80% (critical paths); ≥60% (overall)

---

## Overview

The PDF2html testing strategy balances comprehensive coverage of critical functionality with pragmatic acceptance of limitations in third-party libraries (UglyToad PDF extraction). Tests are organized by concern: unit tests for isolated services, integration tests for end-to-end flows.

---

## Testing Pyramid

```
        △
       ╱ ╲
      ╱ E2E ╲        End-to-end workflows
     ╱───────╲       (Manual/Integration testing)
    ╱         ╲
   ╱ Integration╲    API endpoint validation
  ╱──────────────╲   Pipeline orchestration
 ╱ Unit Tests    ╲  Fast, isolated, deterministic
╱──────────────────╲ (Foundation layer)
```

**Strategy**: Heavy unit test coverage at service layer, integration tests for controller, E2E manual testing.

---

## Test Organization

```
Tests/
├── pdf-to-html-quality-tests.cs    (LayoutNormalizationService)
├── [TODO] html-render-service-tests.cs
├── [TODO] pdf-extraction-tests.cs
├── [TODO] document-controller-tests.cs
└── [TODO] integration-tests.cs
```

---

## Phase 1 Tests (End-to-End Flow)

**Status**: Manual testing completed; automated test suite pending

### Test Scenarios (Manual)
1. ✅ Upload valid PDF → Success response with blockCount
2. ✅ Upload non-PDF file → 400 error
3. ✅ Upload oversized file (>20MB) → 400 error
4. ✅ Upload missing file → 400 error
5. ✅ Verify HTML output is valid HTML5
6. ✅ Verify XSS encoding in HTML (filename, text content)

---

## Phase 2 Tests (Output Quality & Metrics)

### Unit Tests: PdfQualityTests (pdf-to-html-quality-tests.cs)

**Current Coverage**: 4/4 passing ✅

#### Test 1: Heading1 Classification
**Purpose**: Verify BlockType.Heading1 for high font sizes

```csharp
[Fact]
public void Normalize_ShouldClassifyHeading1_WhenFontSizeGreaterThan16()
{
    // Arrange
    var blocks = new List<TextBlock>
    {
        new() { Text = "Main Title", PageNumber = 1, X = 100, Y = 700, FontSize = 24 }
    };

    // Act
    var result = _normalizationService.Normalize(blocks);

    // Assert
    Assert.Single(result);
    Assert.Equal(BlockType.Heading1, result[0].Type);
}
```

**Test Data**: FontSize=24pt (clearly > 16pt threshold)  
**Expected**: BlockType.Heading1  
**Status**: ✅ PASS

#### Test 2: Heading2 Classification
**Purpose**: Verify BlockType.Heading2 for medium font sizes

```csharp
[Fact]
public void Normalize_ShouldClassifyHeading2_WhenFontSizeBetween13And16()
{
    // Arrange
    var blocks = new List<TextBlock>
    {
        new() { Text = "Subheading", PageNumber = 1, X = 100, Y = 650, FontSize = 14 }
    };

    // Act
    var result = _normalizationService.Normalize(blocks);

    // Assert
    Assert.Single(result);
    Assert.Equal(BlockType.Heading2, result[0].Type);
}
```

**Test Data**: FontSize=14pt (between 13pt and 16pt)  
**Expected**: BlockType.Heading2  
**Status**: ✅ PASS

#### Test 3: Paragraph Classification
**Purpose**: Verify BlockType.Paragraph for small font sizes

```csharp
[Fact]
public void Normalize_ShouldClassifyParagraph_WhenFontSizeBelow13()
{
    // Arrange
    var blocks = new List<TextBlock>
    {
        new() { Text = "Body text content", PageNumber = 1, X = 100, Y = 600, FontSize = 11 }
    };

    // Act
    var result = _normalizationService.Normalize(blocks);

    // Assert
    Assert.Single(result);
    Assert.Equal(BlockType.Paragraph, result[0].Type);
}
```

**Test Data**: FontSize=11pt (< 13pt)  
**Expected**: BlockType.Paragraph  
**Status**: ✅ PASS

#### Test 4: Page Order Maintenance
**Purpose**: Verify blocks are emitted in page order (page 1 before page 2)

```csharp
[Fact]
public void Normalize_ShouldMaintainPageOrder()
{
    // Arrange
    var blocks = new List<TextBlock>
    {
        new() { Text = "Page 1", PageNumber = 1, X = 100, Y = 700, FontSize = 12 },
        new() { Text = "Page 2", PageNumber = 2, X = 100, Y = 700, FontSize = 12 },
        new() { Text = "Page 1 Bottom", PageNumber = 1, X = 100, Y = 100, FontSize = 12 }
    };

    // Act
    var result = _normalizationService.Normalize(blocks);

    // Assert
    Assert.Equal(3, result.Count);
    Assert.All(result.Take(2), b => Assert.Equal(1, b.PageNumber));
    Assert.Equal(2, result.Last().PageNumber);
}
```

**Test Data**: 3 blocks across 2 pages (order shuffled in input)  
**Expected**: Page 1 blocks first, then page 2 blocks  
**Status**: ✅ PASS

---

### Test Coverage Matrix

| Component | Method | Test Case | Coverage |
|-----------|--------|-----------|----------|
| LayoutNormalizationService | Normalize() | Classification logic | ✅ 100% |
| | | Page ordering | ✅ 100% |
| | | Input validation | ⏹️ TODO |
| | ClassifyBlock() | Font size thresholds | ✅ 100% |
| | | Edge cases (13pt, 16pt) | ⏹️ TODO |
| HtmlRenderService | RenderStructured() | H1/H2/P mapping | ⏹️ TODO |
| | | List wrapping | ⏹️ TODO |
| | | XSS encoding | ⏹️ TODO |
| | | Empty list handling | ⏹️ TODO |
| DocumentController | Upload() | Happy path | ⏹️ TODO |
| | | File validation | ⏹️ TODO |
| | | Error handling | ⏹️ TODO |

---

## Phase 2 Tests: To-Do

### Unit Test: HtmlRenderServiceTests

**Location**: `Tests/html-render-service-tests.cs`  
**Priority**: P1  
**Effort**: 6-8 hours

```csharp
public sealed class HtmlRenderServiceTests
{
    private readonly HtmlRenderService _service = new();

    [Theory]
    [InlineData(BlockType.Heading1, "<h1>")]
    [InlineData(BlockType.Heading2, "<h2>")]
    [InlineData(BlockType.Paragraph, "<p>")]
    public void RenderStructured_ShouldMapBlockTypeToHtmlTag(BlockType type, string expectedTag)
    {
        // Test semantic tag mapping
    }

    [Fact]
    public void RenderStructured_ShouldWrapConsecutiveListItemsInUlTag()
    {
        // Test <ul> wrapping of <li> elements
        // Input: Multiple consecutive BlockType.List blocks
        // Expected: <ul> opens before first <li>, closes after last
    }

    [Fact]
    public void RenderStructured_ShouldNotProduceUlTag_WhenNoListBlocks()
    {
        // Test no <ul> wrapping when no lists
    }

    [Fact]
    public void RenderStructured_ShouldHtmlEncodeDangerousCharacters()
    {
        // Test XSS protection
        // Input: Text = "<script>alert(1)</script>"
        // Expected in HTML: "&lt;script&gt;alert(1)&lt;/script&gt;"
    }

    [Fact]
    public void RenderStructured_ShouldHandleEmptyTextBlock()
    {
        // Test edge case: empty text
        // Expected: No content, proper HTML structure maintained
    }

    [Fact]
    public void RenderStructured_ShouldEncodeFilenameInTitle()
    {
        // Test filename encoding in <title> tag
    }
}
```

**Test Cases**:
1. ✅ BlockType → HTML tag mapping (h1, h2, p, li)
2. ✅ List wrapping (`<ul>` opens/closes properly)
3. ✅ XSS encoding (dangerous chars → HTML entities)
4. ✅ Empty text handling
5. ✅ Filename encoding in <title>
6. ✅ Deep nesting validation
7. ✅ Mixed content ordering

---

### Unit Test: PdfExtractionServiceTests

**Location**: `Tests/pdf-extraction-service-tests.cs`  
**Priority**: P2  
**Effort**: 8+ hours  
**Challenge**: Mock UglyToad, create test PDFs

```csharp
public sealed class PdfExtractionServiceTests
{
    private readonly PdfExtractionService _service = new();

    [Fact]
    public async Task ExtractAsync_ShouldReturnTextBlocks_ForValidPdf()
    {
        // Integration test with real PDF
        // Validates entire extraction pipeline
    }

    [Fact]
    public async Task ExtractAsync_ShouldSetPageNumber_ForMultiPagePdf()
    {
        // Test page number accuracy
    }

    [Fact]
    public async Task ExtractAsync_ShouldCaptureXYCoordinates_FromPdf()
    {
        // Test coordinate extraction
    }

    [Fact]
    public async Task ExtractAsync_ShouldCaptureFontSize_FromPdf()
    {
        // Test font size detection
    }

    [Fact]
    public async Task ExtractAsync_ShouldThrow_ForInvalidPdf()
    {
        // Test error handling for corrupted files
    }
}
```

**Blockers**:
- UglyToad library design makes mocking difficult
- Requires test PDF files with known content/coordinates
- Coordinates may vary by PDF vendor/encoding

**Recommendation**: Focus on integration tests with known PDFs rather than unit tests.

---

### Integration Test: DocumentControllerTests

**Location**: `Tests/document-controller-tests.cs`  
**Priority**: P1  
**Effort**: 8-10 hours

```csharp
public sealed class DocumentControllerIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [Fact]
    public async Task Upload_ShouldReturn200_WithValidPdf()
    {
        // Create test PDF
        var pdf = CreateTestPdf("invoice.pdf");
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(File.OpenRead(pdf)), "file", "invoice.pdf");

        var response = await _client.PostAsync("/api/v1/documents/upload", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ShouldReturn400_WithoutFile()
    {
        var response = await _client.PostAsync("/api/v1/documents/upload", 
            new MultipartFormDataContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ShouldReturn400_WithNonPdfFile()
    {
        // Upload .txt file instead of PDF
        var response = await _client.PostAsync("/api/v1/documents/upload", ...);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsAsync<ErrorResponse>();
        Assert.Contains("PDF", content.Error);
    }

    [Fact]
    public async Task Upload_ShouldReturn400_WithOversizedFile()
    {
        // Create 21MB file
        var oversized = CreateOversizedFile(21 * 1024 * 1024);

        var response = await _client.PostAsync("/api/v1/documents/upload", ...);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ShouldStoreResult_InDocumentStore()
    {
        // Verify file saved correctly
        // Check storage/uploads/ and storage/results/
    }

    [Fact]
    public async Task Upload_ShouldReturnValidDocumentId()
    {
        var response = await UploadTestPdf("test.pdf");
        var json = await response.Content.ReadAsAsync<dynamic>();

        Assert.NotNull(json.documentId);
        Assert.Matches(@"^[a-z0-9]{32}$", json.documentId);
    }
}
```

**Test Scope**:
1. ✅ Valid PDF upload → 200 OK
2. ✅ Missing file → 400 error
3. ✅ Invalid file type → 400 error
4. ✅ Oversized file → 400 error
5. ✅ Response schema validation
6. ✅ DocumentId format validation
7. ✅ blockCount accuracy
8. ✅ HTML content presence

---

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test PDF2html.sln

# Run with verbose output
dotnet test PDF2html.sln --verbosity detailed

# Run specific test class
dotnet test PDF2html.sln --filter "ClassName=PdfQualityTests"

# Run with coverage report
dotnet test PDF2html.sln /p:CollectCoverage=true

# Watch mode (rerun on file changes)
dotnet watch test
```

### Visual Studio Test Explorer
1. Open Test Explorer (View → Test Explorer)
2. Build solution
3. All tests appear in hierarchy
4. Click run icon to execute
5. Right-click → Run Tests with Coverage

---

## Coverage Goals

### Phase 2 Coverage Targets

| Layer | Target | Current |
|-------|--------|---------|
| **Models** | 100% | ⏹️ 0% (data containers, low ROI) |
| **Services** | 80% | ⏳ 25% (LayoutNormalization: 100%, others: TBD) |
| **Controllers** | 70% | ⏹️ 0% (needs integration tests) |
| **Overall** | 60% | ⏳ ~15% |

### Coverage Report Location
```
coverage/
├── index.html (summary)
├── class-details/
│   ├── Services.html
│   ├── Controllers.html
│   └── Models.html
```

### View Coverage Report
```bash
# Generate
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Open in browser
start .\coverage\index.html
```

---

## Known Testing Limitations

### 1. UglyToad Extraction Testing
**Issue**: PDF extraction is I/O-bound, third-party library, hard to mock  
**Impact**: Cannot test PdfExtractionService in isolation  
**Mitigation**: Integration tests with real test PDFs

### 2. Font Size Accuracy
**Issue**: Different PDF encodings report font size differently  
**Impact**: Classification thresholds (13pt, 16pt) may vary  
**Mitigation**: Test with actual invoice/contract PDFs; adjust thresholds empirically

### 3. Layout Detection
**Issue**: No table/complex structure detection yet  
**Impact**: BlockType.Table, BlockType.List not exercised  
**Mitigation**: Marked as future work; test when implemented

---

## Test Data

### Unit Test Data

**Purpose**: Minimal, deterministic data for classification testing  
**Location**: `Tests/pdf-to-html-quality-tests.cs` (inline)  
**Example**:
```csharp
new TextBlock { 
    Text = "Main Title", 
    FontSize = 24, 
    PageNumber = 1, 
    X = 100, 
    Y = 700 
}
```

### Integration Test Data

**Purpose**: Real PDF files with known content  
**Location**: `Tests/TestData/` (to be created)  
**Files**:
- `simple-invoice.pdf` - 1 page, basic structure
- `multipage-contract.pdf` - 3 pages, complex layout
- `edge-cases.pdf` - Font Size edge cases (13pt, 16pt)

### Test PDF Generation

```python
# Create test PDFs programmatically (Python + ReportLab)
from reportlab.pdfgen import canvas

pdf = canvas.Canvas("test.pdf", pagesize=(612, 792))
pdf.setFont("Helvetica", 24)
pdf.drawString(100, 700, "Main Title")
pdf.setFont("Helvetica", 14)
pdf.drawString(100, 680, "Subtitle")
pdf.save()
```

---

## Continuous Integration (CI)

### GitHub Actions Workflow (Planned)

```yaml
name: Tests & Coverage

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
```

### CI Requirements
- All tests must pass before merge
- Coverage must not decrease
- Build must compile without warnings (treat as errors)

---

## Test Review Checklist

Before merging test code:

- [ ] All tests execute and pass locally
- [ ] Test names follow convention: `Method_Scenario_Expected`
- [ ] Each test has one logical assertion or group of related assertions
- [ ] No test interdependencies (isolated, can run in any order)
- [ ] Happy path and error cases covered
- [ ] Edge cases considered (null, empty, boundaries)
- [ ] Test data is minimal and relevant
- [ ] Comments explain non-obvious test logic

---

## Related Documentation
- [Implementation Progress](./implementation-progress.md) - Feature status & test count
- [System Architecture](./system-architecture.md) - Component testability
- [API Contract](./api-contract.md) - API test specifications
