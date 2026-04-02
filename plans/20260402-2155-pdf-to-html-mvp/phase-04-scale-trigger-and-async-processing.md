# Phase 04 - Scale Trigger and Async Processing

## Context Links
- API baseline: `plans/20260402-2155-pdf-to-html-mvp/phase-03-api-hardening-and-domain-focus.md`
- Product draft: `PDF2html/doc.md`

## Overview
- Priority: P2
- Status: pending
- Timeline: theo nhu cầu tải thực tế
- Goal: Chỉ thêm queue/worker khi có bằng chứng bottleneck rõ ràng.

## Key Insights
- Scale quá sớm làm tăng cost và độ phức tạp vận hành.
- Cần trigger định lượng để quyết định tách worker.
- Luôn ưu tiên tối ưu synchronous path trước khi phân tán.

## Requirements
- Functional:
  - Tích hợp queue (RabbitMQ hoặc Azure Queue).
  - Worker xử lý async và cập nhật trạng thái job.
  - Retry + dead-letter queue cơ bản.
- Non-functional:
  - Theo dõi backlog, throughput, fail rate.
  - Đảm bảo idempotency cho job processing.

## Architecture
- Trigger criteria (ít nhất 1 điều kiện):
  - P95 > 10s ổn định trong giờ cao điểm.
  - Concurrency > 20 request xử lý nặng cùng lúc.
  - Timeout tăng và ảnh hưởng UX/API reliability.
  - Nhu cầu batch processing lớn.
- Nếu trigger đạt:
  - API ghi nhận request -> enqueue -> trả `accepted`.
  - Worker consume queue -> process -> save result.
  - Client poll status/result endpoint.

## Related Code Files
- Modify:
  - `PDF2html/Program.cs`
  - `PDF2html/Controllers/document-controller.cs`
- Create:
  - `PDF2html/Queue/document-job-producer.cs`
  - `PDF2html/Worker/document-job-worker.cs`
  - `PDF2html/Models/document-job-status.cs`
  - `PDF2html/Tests/queue-worker-integration-tests.cs`
- Delete:
  - none

## Implementation Steps
1. Chốt trigger dữ liệu và dashboard theo dõi.
2. Thiết kế job model và trạng thái vòng đời.
3. Tích hợp producer từ API.
4. Xây worker xử lý idempotent.
5. Thêm retry policy và DLQ.
6. Kiểm thử tải và đo cải thiện.

## Todo List
- [ ] Validate trigger bằng số liệu production-like.
- [ ] Rollout queue/worker sau feature flag.
- [ ] Chứng minh giảm timeout và tăng throughput.

## Success Criteria
- Latency API giảm rõ trong tải cao.
- Fail rate giảm khi xử lý file lớn/batch.
- Không mất job và có khả năng truy vết đầy đủ.

## Risk Assessment
- Rủi ro: vận hành queue sai cấu hình gây backlog.
- Mitigation: cảnh báo backlog + auto scaling policy.
- Rủi ro: xử lý trùng job.
- Mitigation: idempotency key + lock logic.

## Security Considerations
- Bảo vệ kết nối queue bằng credential an toàn.
- Giới hạn quyền producer/consumer theo nguyên tắc least privilege.

## Next Steps
- Khi ổn định, xem xét bổ sung AI processor theo domain value.
