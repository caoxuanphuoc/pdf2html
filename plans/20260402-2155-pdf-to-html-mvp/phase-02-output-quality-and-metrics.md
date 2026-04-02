# Phase 02 - Output Quality and Metrics

## Context Links
- Previous phase output: `plans/20260402-2155-pdf-to-html-mvp/phase-01-end-to-end-mvp-flow.md`
- Product draft: `PDF2html/doc.md`

## Overview
- Priority: P1
- Status: in-progress (60% complete - core impl done, metrics/dataset pending)
- Timeline: 2 tuần
- Goal: Tăng chất lượng HTML output và đưa metric thành chuẩn quyết định.
- **Code Review**: [Phase 02 Review Report](../reports/phase-02-code-review.md)
- **Blocking Issues**: 0 critical (fixed), 4 high-priority, 3 medium-priority
- **Estimate remaining**: 2-3 weeks (dataset + KPI validation)

## Key Insights
- Không có metric thì không biết đã đủ tốt để ship chưa.
- Rule-based layout cần được kiểm định bằng tập PDF đại diện.
- Cải thiện có kiểm soát theo vòng lặp: đo -> sửa rule -> đo lại.

### Code Review Findings (2026-04-02)
- ✅ **Strengths**: Clean separation of concerns, good security (XSS prevention), type-safe models
- � **Critical Blockers Resolved** (3):
  1. ✓ List items now wrapped in `<ul>` tags (valid HTML)
  2. ✓ `RenderStructured()` added to interface contract and DI
  3. ✓ Normalization service integrated in controller (production-ready)
- ⚠️ **High Priority** (4): Missing JSON output, zero test coverage of RenderStructured, Table type never classified, magic numbers duplicated
- 📋 **Medium Priority** (3): Edge case tests missing, no text validation, coordinate ordering not validated
- **Requirement Status**: 60% complete (core implementation done, metrics & dataset collection pending)

## Requirements
- Functional:
  - Group line/block tốt hơn.
  - Mapping heading/paragraph/list rõ ràng hơn.
  - JSON structure song song HTML output.
- Non-functional:
  - Có benchmark định kỳ trên dataset cố định.
  - Có báo cáo metric theo build hoặc theo lần chạy test.

## Architecture
- Bổ sung pipeline normalization:
  - sort theo trang + toạ độ.
  - merge text spans cùng dòng.
  - split block theo khoảng cách dọc.
- Bổ sung evaluator module:
  - chấm success/fail theo rule.
  - log lý do fail để triage.

## Related Code Files
- Modify:
  - `PDF2html/Services/pdf-extraction-service.cs`
  - `PDF2html/Services/html-render-service.cs`
- Create:
  - `PDF2html/Services/layout-normalization-service.cs`
  - `PDF2html/Models/structured-document.cs`
  - `PDF2html/Tests/pdf-to-html-quality-tests.cs`
  - `PDF2html/TestData/pdfs/` (dataset mẫu)
- Delete:
  - none

## Implementation Steps
1. Thu thập dataset 50-200 file (invoice, hợp đồng, báo cáo).
2. Định nghĩa tiêu chí pass/fail cho từng loại file.
3. Cải thiện rule layout theo vòng lặp ngắn.
4. Sinh JSON structure cùng HTML.
5. Viết test quality regression.
6. Tạo báo cáo metric mỗi lần chạy test.

## Todo List
### Critical Fixes (Blocking Merge)
- [x] Fix: Wrap List items in `<ul>` tags in RenderStructured()
- [x] Fix: Add RenderStructured() method signature to IHtmlRenderer interface
- [x] Fix: Integrate LayoutNormalizationService into DocumentController
- [x] Fix: Register LayoutNormalizationService in dependency injection (Program.cs)

### Phase 2 Core Requirements
- [ ] Chốt dataset version v1 (50-200 PDFs).
- [ ] Thêm JSON output với classified blocks trong response.
- [ ] Implement evaluator module (metrics collection + logging).
- [ ] Có benchmark script cho conversion quality.
- [ ] Viết test quality regression (RenderStructured + edge cases).
- [ ] Tạo báo cáo metric mỗi lần chạy test.

### High Priority Enhancements  
- [ ] Add test coverage for RenderStructured() (8-10 tests)
- [ ] Extract font size thresholds to constants (DRY violation)
- [ ] Implement table detection logic or remove BlockType.Table

### Polish Before Release
- [ ] Add edge case tests (empty blocks, special chars, whitespace)
- [ ] Add text validation and filtering in normalization
- [ ] Validate coordinate system with semantic test cases
- [ ] Đạt KPI chất lượng tối thiểu (success rate >= 90%)
- [ ] Chặn regression bằng test tự động

## Success Criteria
- Convert success rate >= 90% dataset v1.
- P95 time <= 6s/file <= 10 trang.
- Sai lệch heading/paragraph dưới ngưỡng chấp nhận nội bộ.

## Risk Assessment
- **Rủi ro cao**: Insufficient dataset (target 50-200 PDFs, currently 0) → Cannot validate KPI 90% success rate.
  - **Impact**: Phase blockers until dataset collected (invoice, contract, report samples from multiple sources).
  - **Mitigation**: Parallel dataset sourcing; quality metrics benchmarking deferred until v1 dataset ready.
- Rủi ro: Quy tắc layout overfit theo dataset nhỏ.
  - Mitigation: Bổ sung tài liệu từ nhiều nguồn khác nhau.
- Rủi ro: Table khiến output rối.
  - Mitigation: Đánh dấu table fallback, chưa tối ưu sâu trong phase này.

## Security Considerations
- Làm sạch text trước khi render HTML để tránh script injection.
- Không ghi log raw nội dung tài liệu nhạy cảm ở môi trường chia sẻ.

## Next Steps
- Khi đạt KPI, chuyển sang hardening API và chốt use case thương mại ở Phase 3.
