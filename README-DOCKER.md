# 🤖 AI Agent - Docker Deployment Guide

Hệ thống Trợ lý ảo giáo dục thông minh đa nền tảng (Telegram & Web Plugin).

## 📋 Yêu cầu hệ thống

- Docker 20.10+
- Docker Compose 2.0+
- Tối thiểu 4GB RAM
- 10GB dung lượng ổ đĩa trống

## 🚀 Quick Start

### 1. Chuẩn bị môi trường

```bash
# Clone hoặc cd vào thư mục project
cd /path/to/AI-Agent

# Copy file environment mẫu
cp .env.example .env

# Chỉnh sửa file .env với các giá trị thực tế
nano .env  # hoặc code .env
```

### 2. Cấu hình bắt buộc

Cập nhật các giá trị sau trong file `.env`:

| Biến | Mô tả | Bắt buộc |
|------|-------|----------|
| `POSTGRES_PASSWORD` | Password database | ✅ |
| `FLOWISE_PASSWORD` | Password đăng nhập Flowise | ✅ |
| `OPENAI_API_KEY` | API key OpenAI | ✅ |
| `TELEGRAM_BOT_TOKEN` | Token bot từ @BotFather | ✅ |
| `JWT_SECRET` | Secret key (min 32 ký tự) | ✅ |

### 3. Build và chạy

```bash
# Build tất cả images
docker-compose build

# Khởi động tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f

# Xem logs của service cụ thể
docker-compose logs -f core-service
```

### 4. Kiểm tra trạng thái

```bash
# Xem trạng thái các containers
docker-compose ps

# Kiểm tra health
docker-compose exec postgres pg_isready
```

## 🌐 Truy cập các services

| Service | URL | Mô tả |
|---------|-----|-------|
| **Dashboard** | http://localhost:8080 | Giao diện quản trị |
| **Web Plugin** | http://localhost:8081 | Chat Widget nhúng web |
| **Core API** | http://localhost:5000 | Backend API |
| **Flowise** | http://localhost:3000 | AI Orchestration UI |
| **PostgreSQL** | localhost:5432 | Database |

## 📦 Cấu trúc Project

```
AI-Agent/
├── CoreService/              # .NET Core Backend
│   └── Dockerfile
├── Agent-dashboard/          # Vue 3 Dashboard
│   ├── Dockerfile
│   └── nginx.conf
├── Agent-plugin/             # Vue 3 Chat Widget
│   ├── Dockerfile
│   └── nginx.conf
├── docker/
│   └── init-db/
│       └── 01-init.sql       # Database initialization
├── docker-compose.yml        # Docker Compose configuration
├── .env.example              # Environment template
└── README-DOCKER.md          # This file
```

## 🔧 Thiết lập Telegram Bot

1. Mở Telegram, tìm **@BotFather**
2. Gửi `/newbot` và làm theo hướng dẫn
3. Copy token và paste vào `TELEGRAM_BOT_TOKEN` trong `.env`
4. (Tùy chọn) Thiết lập webhook cho production:
   ```bash
   curl -X POST "https://api.telegram.org/bot<YOUR_TOKEN>/setWebhook?url=<YOUR_WEBHOOK_URL>"
   ```

## 📚 Thiết lập Flowise (RAG)

1. Truy cập http://localhost:3000
2. Đăng nhập với credentials từ `.env`
3. Tạo Flow mới với các node:
   - **Document Loaders**: PDF / DOCX
   - **Text Splitters**: Recursive Character
   - **Embeddings**: OpenAI Embeddings
   - **Vector Store**: Postgres (pgvector)
   - **LLM**: ChatOpenAI
   - **Chains**: Conversational Retrieval QA

## 🛠️ Commands hữu ích

```bash
# Dừng tất cả services
docker-compose down

# Dừng và xóa volumes (⚠️ mất dữ liệu)
docker-compose down -v

# Rebuild một service cụ thể
docker-compose build core-service
docker-compose up -d core-service

# Xem logs real-time
docker-compose logs -f --tail=100

# Truy cập shell của container
docker-compose exec core-service /bin/bash
docker-compose exec postgres psql -U postgres -d ai_agent_db
```

## 🔒 Production Checklist

- [ ] Đổi tất cả passwords mặc định
- [ ] Cấu hình HTTPS/SSL
- [ ] Thiết lập Telegram Webhook URL
- [ ] Backup database định kỳ
- [ ] Monitor với Prometheus/Grafana (tùy chọn)
- [ ] Rate limiting cho API

## ❓ Troubleshooting

### Container không khởi động
```bash
# Xem logs chi tiết
docker-compose logs <service-name>

# Rebuild image
docker-compose build --no-cache <service-name>
```

### Database connection failed
```bash
# Kiểm tra postgres đã ready chưa
docker-compose exec postgres pg_isready

# Xem logs postgres
docker-compose logs postgres
```

### Flowise không kết nối được database
- Đảm bảo postgres container healthy
- Kiểm tra `FLOWISE_DB` đã được tạo trong init script

---

📧 **Liên hệ hỗ trợ**: [your-email@example.com]
