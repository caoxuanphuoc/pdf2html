---
title: "PDF to HTML MVP Implementation Plan"
description: "Kế hoạch triển khai MVP PDF sang HTML theo hướng đơn giản, không queue/worker giai đoạn đầu."
status: in-progress
priority: P1
effort: 7w
branch: n/a
tags: [pdf, html, mvp, dotnet, api]
created: 2026-04-02
updated: 2026-04-02
progress: Phase 1 complete, Phase 2 core impl 60% (metrics pending)
---

# PDF to HTML MVP Plan

## Mục tiêu
Ra MVP xử lý PDF -> HTML ổn định trước, trì hoãn queue/worker đến khi có nhu cầu scale thực tế.

## Phases
- [Phase 1 - End-to-end MVP Flow](./phase-01-end-to-end-mvp-flow.md) - `complete` - 2 tuần ✅
- [Phase 2 - Output Quality & Metrics](./phase-02-output-quality-and-metrics.md) - `in-progress` (60%) - 2 tuần
- [Phase 3 - API Hardening & Domain Focus](./phase-03-api-hardening-and-domain-focus.md) - `pending` - 2-3 tuần
- [Phase 4 - Scale Trigger & Async Processing](./phase-04-scale-trigger-and-async-processing.md) - `pending` - khi đủ tín hiệu tải

## Milestones
1. API nhận file PDF và trả HTML thành công với bộ test cơ bản.
2. Có bộ metric chất lượng đầu ra và hiệu năng tối thiểu.
3. API contract ổn định, sẵn sàng tích hợp client thật.
4. Có tiêu chí rõ ràng để quyết định khi nào thêm queue/worker.

## Dependencies
- .NET 9 project hiện tại (`PDF2html/PDF2html.csproj`).
- Thư viện parse PDF: PdfPig.
- Tập dữ liệu PDF đại diện (ít nhất 50 file cho vòng đầu).

## Technical Decisions (Locked)
- Parser: PdfPig.
- Storage MVP: file system local tạm thời, chưa dùng database.
- Domain ưu tiên Phase 3: hợp đồng.

## KPI cốt lõi cho MVP
- Convert success rate >= 90% trên tập PDF pilot.
- P95 processing time <= 6s cho file <= 10 trang.
- Tỷ lệ lỗi 5xx API <= 1% trên môi trường dev test.

## Out of Scope (giai đoạn này)
- Multi-tenant billing.
- AI extraction production-grade.
- Queue/worker phân tán trước khi có tải thực tế.

## Gating
- Chỉ chuyển sang Phase 4 khi có ít nhất 1 ngưỡng scale bị vượt (latency, concurrency, timeout hoặc backlog).
