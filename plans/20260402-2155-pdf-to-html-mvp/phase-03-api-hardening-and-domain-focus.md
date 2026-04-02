# Phase 03 - API Hardening and Domain Focus

## Context Links
- KPI and quality baseline: `plans/20260402-2155-pdf-to-html-mvp/phase-02-output-quality-and-metrics.md`
- Product draft: `PDF2html/doc.md`

## Overview
- Priority: P1
- Status: pending
- Timeline: 2-3 tuần
- Goal: Ổn định API cho tích hợp thật và chốt 1 use case tạo doanh thu sớm.

## Key Insights
- API contract ổn định quan trọng hơn thêm nhiều tính năng mới.
- Chỉ nên chọn 1 domain trước (invoice hoặc contract) để tránh dàn trải.
- Quan sát lỗi thật từ client để khóa backlog ưu tiên.
- Domain đã chốt: hợp đồng.

## Requirements
- Functional:
  - API versioning cơ bản.
  - Error model thống nhất.
  - Endpoint status/result rõ trạng thái xử lý.
  - Triển khai schema extraction ưu tiên cho hợp đồng.
- Non-functional:
  - Có rate limit nhẹ.
  - Có audit log tối thiểu cho request/response metadata.

## Architecture
- Contract-first API:
  - Định nghĩa `DocumentResponse`, `ErrorResponse`, `ExtractionResult`.
  - Version endpoint `/api/v1/...`.
- Observability:
  - Correlation id xuyên suốt request.
  - Structured logging theo event.

## Related Code Files
- Modify:
  - `PDF2html/Program.cs`
  - `PDF2html/Controllers/document-controller.cs`
- Create:
  - `PDF2html/Contracts/document-response.cs`
  - `PDF2html/Contracts/error-response.cs`
  - `PDF2html/Middleware/request-correlation-middleware.cs`
  - `PDF2html/Tests/api-contract-tests.cs`
- Delete:
  - none

## Implementation Steps
1. Chuẩn hóa response schema và mã lỗi.
2. Áp dụng versioning cho endpoint.
3. Bổ sung middleware correlation id + logging.
4. Khóa domain pilot là hợp đồng.
5. Triển khai extraction schema cho hợp đồng.
6. Viết integration tests cho contract.

## Todo List
- [ ] Publish OpenAPI chuẩn cho v1.
- [ ] Pass toàn bộ contract tests.
- [ ] Domain pilot chạy với dữ liệu thật.
- [ ] Có tài liệu tích hợp cho client.

## Success Criteria
- Không có breaking change trong API v1 sau khi công bố nội bộ.
- Domain pilot đạt mức usable với user nội bộ.
- Số lỗi runtime giảm qua từng sprint.

## Risk Assessment
- Rủi ro: Scope creep do thêm nhiều domain cùng lúc.
- Mitigation: Chỉ cho phép 1 domain active trong phase.
- Rủi ro: API đổi liên tục gây vỡ tích hợp.
- Mitigation: Contract-first và deprecation policy rõ ràng.

## Security Considerations
- Xác thực cơ bản cho API key hoặc token nội bộ.
- Mask dữ liệu nhạy cảm trong log.

## Next Steps
- Sau khi có tín hiệu tải, đánh giá điều kiện tách queue/worker ở Phase 4.
