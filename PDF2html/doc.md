# 🚀 Document Intelligence Platform (Startup Architecture)

## 1. 🎯 Product Vision

Xây dựng nền tảng xử lý tài liệu thông minh:

* Convert PDF → HTML
* Trích xuất dữ liệu (invoice, hợp đồng, báo cáo)
* Cung cấp API cho hệ thống khác

---

## 2. 🧠 High-Level Architecture

```
Client (Web / API / Bot)
        ↓
API Gateway (.NET)
        ↓
Document Processing Service
        ↓
Storage (File + Database)
        ↓
Query / Search API
```

---

## 3. 🧩 Core Modules

### 3.1 API Layer

* Upload PDF
* Lấy HTML / JSON
* Query dữ liệu
* Xử lý trực tiếp trong cùng service (giai đoạn MVP)

Tech:

* ASP.NET Core Web API

---

### 3.2 Queue Layer

* Chưa dùng ở giai đoạn đầu
* Chỉ bổ sung khi có tải lớn hoặc cần xử lý nền

Tech:

* RabbitMQ / Azure Queue (Phase scale)

---

### 3.3 Document Processing Service (Core Engine)

Pipeline:

```
PDF → Extract → Analyze → Structure → Output
```

---

### 3.4 Extract Layer

* Đọc PDF
* Lấy text + tọa độ + font

Output:

```
{
  text: string,
  x: number,
  y: number,
  fontSize: number
}
```

---

### 3.5 Layout Analyzer

* Group text theo dòng
* Detect block
* Phân loại heading / paragraph

Rule ví dụ:

```
fontSize > 16 → H1
fontSize > 13 → H2
else → paragraph
```

---

### 3.6 Structure Builder

Convert sang dạng semantic:

```
[
  { type: "h1", content: "Title" },
  { type: "p", content: "Content" }
]
```

---

### 3.7 HTML Renderer

* Convert JSON → HTML

---

### 3.8 AI Processor (Optional - Key Value)

* Hiểu nội dung
* Extract dữ liệu
* Tóm tắt

Use cases:

* Invoice parsing
* Contract analysis

---

## 4. 🗄️ Storage

### Lưu trữ:

* File PDF gốc
* HTML output
* JSON structure

Tech:

* Object Storage (S3 / MinIO)
* Database (PostgreSQL / MongoDB)

---

## 5. ⚡ Processing Flow

```
User upload PDF
   ↓
API nhận file
   ↓
Service xử lý PDF → HTML
   ↓
Lưu kết quả
   ↓
User fetch kết quả
```

---

## 6. 🧑‍💻 Frontend (Optional)

* Upload file
* Preview HTML
* Highlight dữ liệu

---

## 7. 💰 Monetization Strategy

### Giai đoạn 1 (MVP)

* PDF → HTML
* Extract text

### Giai đoạn 2

* Detect structure

### Giai đoạn 3 (Core Revenue)

* Extract dữ liệu:

  * hóa đơn
  * hợp đồng

### Giai đoạn 4

* AI search
* Hỏi đáp trên tài liệu

---

## 8. 🧠 Competitive Advantage

* Chủ động parsing engine
* Không phụ thuộc lib trả phí
* Dễ tích hợp AI

---

## 9. ⚠️ Risks & Challenges

* PDF không có structure
* Layout phức tạp
* Table detection khó
* Chất lượng output không ổn định giữa các nguồn PDF

---

## 10. 🚀 Development Roadmap

### Phase 1 (2 tuần)

* Build luồng PDF → HTML trong cùng service
* API upload + trả kết quả HTML cơ bản
* Lưu file gốc + HTML output

### Phase 2 (2 tuần)

* Nâng chất lượng layout rules
* Bổ sung JSON structure
* Đo metric: tỷ lệ convert thành công, thời gian xử lý

### Phase 3 (2-3 tuần)

* Chuẩn hóa API contract + mã lỗi
* Ổn định parser với tập PDF thực tế
* Chốt use case ưu tiên (invoice hoặc hợp đồng)

### Phase 4

* Cân nhắc queue + worker khi có nhu cầu scale thật
* AI processing (optional)

---

## 11. 🎯 Long-term Vision

* Document AI platform
* SaaS API
* Tích hợp vào hệ thống doanh nghiệp
