---
description: Hướng dẫn triển khai Phase 3 - AI & RAG Integration
---

# Phase 3: AI & RAG Integration Setup Guide

Tài liệu này hướng dẫn chi tiết các bước để triển khai hệ thống RAG (Retrieval-Augmented Generation) sử dụng Flowise và tích hợp vào .NET Core Service.

## 1. Flowise RAG Setup (Manual Step)

Bạn cần truy cập vào Flowise UI (http://localhost:3000) và tạo 2 Chatflow riêng biệt (hoặc 1 nếu dùng InMemory, nhưng production nên dùng Upsert API).

### a. Setup Vector Store (Ingestion Pipeline)
Tạo một Chatflow mới tên là **"RAG Ingestion"**:
1.  **Nodes cần có**:
    *   **Document Loader**: Chọn `PDF Loader` (hoặc `Docx Loader`).
    *   **Text Splitter**: Chọn `Recursive Character Text Splitter` (Chunk Size: 1000, Overlap: 200).
    *   **Embeddings**: Chọn `Google Generative AI Embeddings` (Model: `text-embedding-004`). Cần API Key.
    *   **Vector Store**: Chọn `Postgres Vector` (`pgvector`).
        *   **Host**: `postgres` (tên service trong docker-compose)
        *   **Database**: `ai_agent_db`
        *   **Table Name**: `documents`
        *   **User/Password**: Lấy từ biến môi trường (`postgres`/`postgres123`).
2.  **Kết nối**: Loader -> Splitter -> Vector Store (Document input). Embeddings -> Vector Store.
3.  **Lưu**: Save Chatflow.
4.  **Lấy ID**: Lưu lại `Chatflow ID` (ví dụ: `uuid-ingest`). Đây sẽ là ID dùng để gọi API upload file.

### b. Setup Retrieval Chat (Conversation Pipeline)
Tạo một Chatflow mới tên là **"RAG Chat"**:
1.  **Nodes cần có**:
    *   **LLM**: Chọn `Google Generative AI` (Model: `gemini-1.5-pro` hoặc `gemini-2.0-flash`).
    *   **Vector Store**: Chọn `Postgres Vector` (Cấu hình Y HỆT bước trên: Host, DB, Table, Credentials).
        *   **Quan trọng**: Connect Embeddings node giống hệt bước trên vào đây.
    *   **Chain**: Chọn `Conversational Retrieval QA Chain` (hoặc `Retrieval QA Chain`).
    *   **Memory**: Chọn `Buffer Memory`.
2.  **Kết nối**:
    *   LLM -> Chain.
    *   Vector Store -> Chain (vào input `Vector Store Retriever`).
    *   Memory -> Chain.
3.  **Lưu & Test**: Thử chat xem nó có trả lời từ DB không (sau khi đã ingest data).
4.  **Lấy ID**: Lưu lại `Chatflow ID` (ví dụ: `uuid-chat`). Đây sẽ là ID dùng cho `IFlowiseService.SendMessageAsync`.

---

## 2. .NET Core Service Integration (Automated Steps)

Antigravity có thể giúp bạn tự động hóa các bước code sau:

### a. Cập nhật `IFlowiseService` & `FlowiseService`
Thêm method để upload file vào Vector Store thông qua Flowise API.

```csharp
public interface IFlowiseService
{
    // ... existing methods
    Task<bool> IngestDocumentAsync(string filePath, string fileName);
}
```

Implementation sẽ gọi API: `POST /api/v1/vector/upsert/{ChatflowId}`
*Payload dạng `multipart/form-data` chứa file.*

### b. Cập nhật `UploadDocumentCommandHandler`
Sau khi upload file thành công vào local storage (`_fileStorage.SaveStreamAsync`), code sẽ gọi tiếp `_flowiseService.IngestDocumentAsync`.

### c. Cập nhật `GetDocumentQuery` (Optional)
Cập nhật trạng thái `IsProcessed = true` sau khi ingest thành công.

---

## 3. Deployment Checklist

- [ ] **Docker**: Đảm bảo `pgvector` container đang chạy.
- [ ] **API Keys**: Đảm bảo `OPENAI_API_KEY` (hoặc Google AI Key) đã được set trong Flowise container env.
- [ ] **Config**: Cập nhật `appsettings.json` hoặc User Secrets của CoreService với `Flowise:ChatflowId` (cho Chat) và `Flowise:IngestionFlowId` (cho Ingest).

// turbo
## Execute Code Changes
Nếu bạn đồng ý với kế hoạch trên, hãy yêu cầu tôi thực hiện bước **2. .NET Core Service Integration**.
