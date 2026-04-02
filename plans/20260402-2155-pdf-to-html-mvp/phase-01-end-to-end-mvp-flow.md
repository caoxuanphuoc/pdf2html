# Phase 01 - End-to-end MVP Flow

## Context Links
- Product architecture draft: `PDF2html/doc.md`
- Current app entry: `PDF2html/Program.cs`
- Project file: `PDF2html/PDF2html.csproj`

## Overview
- Priority: P1
- Status: pending
- Timeline: 2 tuần
- Goal: Chạy luồng hoàn chỉnh upload PDF -> parse -> render HTML -> trả kết quả qua API.

## Key Insights
- Cần vertical slice chạy thật sớm để validate giá trị sản phẩm.
- Tránh tách queue/worker sớm để giảm độ phức tạp vận hành.
- Cần kiến trúc module nội bộ sạch để sau này tách worker dễ.
- Chốt parser PdfPig ngay từ đầu để giảm thời gian thử nghiệm công nghệ.

## Requirements
- Functional:
  - Endpoint upload PDF.
  - Parse text + basic layout metadata bằng PdfPig.
  - Render HTML cơ bản.
  - Endpoint lấy kết quả theo jobId/documentId.
  - Lưu output tạm thời trên file system local (không database).
- Non-functional:
  - Không crash khi PDF lỗi.
  - Có logging cơ bản theo request id.
  - Thời gian xử lý chấp nhận được cho file nhỏ.

## Architecture
- API synchronous flow (cùng process):
  - `UploadController` nhận file.
  - `PdfExtractionService` trích xuất dữ liệu text/position qua PdfPig.
  - `HtmlRenderService` dựng HTML output.
  - `DocumentResultRepository` lưu metadata + output vào file system local.
- Tách interface ngay từ đầu để chuẩn bị scale:
  - `IPdfExtractor`
  - `IHtmlRenderer`
  - `IDocumentStore`

## Related Code Files
- Modify:
  - `PDF2html/Program.cs`
  - `PDF2html/PDF2html.csproj`
- Create:
  - `PDF2html/Controllers/document-controller.cs`
  - `PDF2html/Services/pdf-extraction-service.cs`
  - `PDF2html/Services/html-render-service.cs`
  - `PDF2html/Models/document-result.cs`
  - `PDF2html/Stores/file-document-store.cs`
- Delete:
  - none

## Implementation Steps
1. Tích hợp PdfPig vào project .NET 9.
2. Tạo model dữ liệu trung gian cho text block.
3. Viết service extraction tách text + toạ độ cơ bản.
4. Viết renderer chuyển model trung gian sang HTML.
5. Tạo API upload và API get-result.
6. Lưu kết quả vào file system local tạm thời.
7. Thêm error handling + logging tối thiểu.

## Todo List
- [ ] Tích hợp PdfPig và POC 3 file mẫu.
- [ ] Hoàn thành API upload + get result.
- [ ] Render HTML basic cho heading/paragraph.
- [ ] Lưu output và expose metadata.
- [ ] Viết smoke tests cho flow chính.

## Success Criteria
- End-to-end chạy thành công trên >= 10 file PDF mẫu.
- API trả lỗi có cấu trúc khi input không hợp lệ.
- HTML output mở được trên browser với nội dung đọc được.

## Risk Assessment
- Rủi ro: PDF scan không extract được text.
- Mitigation: Gắn cờ `ocrRequired=true` để xử lý sau, không block MVP.
- Rủi ro: Encoding tiếng Việt lỗi.
- Mitigation: Chuẩn hóa UTF-8, test tài liệu tiếng Việt sớm.

## Security Considerations
- Giới hạn kích thước file upload.
- Validate MIME type và extension.
- Chặn path traversal khi lưu file.

## Next Steps
- Sau khi xong Phase 1, chuyển sang đo chất lượng theo dataset thật ở Phase 2.
