<h1 align="center">Agitek PC — Backend API</h1>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4" alt="ASP.NET Core Web API" />
  <img src="https://img.shields.io/badge/EF_Core-10-512BD4" alt="Entity Framework Core 10" />
  <img src="https://img.shields.io/badge/SQL_Server-LocalDB%20%2F%20Express-CC2927?logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Redis-Cache%20%26%20Token-DC382D?logo=redis&logoColor=white" alt="Redis" />
</p>

**Agitek PC Backend** là hệ thống RESTful API viết bằng ASP.NET Core (.NET 10) cung cấp toàn bộ nghiệp vụ bán lẻ máy tính, xây dựng cấu hình PC, thanh toán trực tuyến và quản trị vận hành cho nền tảng Agitek. Dự án đảm bảo tính toàn vẹn dữ liệu giao dịch, phân quyền chi tiết theo vai trò và hỗ trợ tìm kiếm ngữ nghĩa cục bộ không phụ thuộc API AI bên ngoài.

---

## Tính năng nổi bật

- **Xác thực & Phân quyền đa tầng (RBAC + Policy):** Đăng ký, xác minh email, quên/đặt lại mật khẩu, đăng nhập Credentials & Google OAuth, đặt mật khẩu cho tài khoản Google (`POST /api/Auth/set-password`), JWT Access Token + Redis Refresh Token có khóa phân tán và cửa sổ grace window chống race condition; phân quyền cho 7 vai trò (`SuperAdmin`, `Admin`, `Manager`, `Staff`, `Delivery`, `Customer`, `Guest`).
- **Tìm kiếm ngữ nghĩa AI cục bộ (Local Semantic Search):** Tích hợp mô hình nhúng Transformer (`SmartComponents.LocalEmbeddings` chạy ONNX trên CPU), đồng bộ vector sản phẩm chạy nền (`ProductEmbeddingBackgroundService`), cache vector truy vấn (`SemanticSearchCache`) và xếp hạng bằng Cosine Similarity.
- **PC Builder tương thích phần cứng:** Lọc linh kiện tương thích theo `Socket`, `RamType`, `MemoryFormFactor`, `Interface`, `FormFactor`, chiều dài `VGA/Case` và công suất nguồn `PSU >= (Tổng TDP + 50) × 1.3`; lưu tối đa 10 cấu hình/tài khoản.
- **Đặt hàng & Tồn kho an toàn giao dịch:** Hỗ trợ khách đăng nhập & khách vãng lai trong transaction mức `Serializable`, tự tính lại giá từ DB, trừ voucher nguyên tử (`ExecuteUpdateAsync`), tự cập nhật trạng thái tồn kho và luồng duyệt/giao hàng kèm tracking theo kho.
- **Thanh toán SePay (VietQR) & Job hết hạn tự động:** Tạo mã VietQR, nhận webhook SePay xác thực chữ ký **HMAC-SHA256**, tự động hủy đơn & hoàn tồn kho sau 15 phút (`PaymentExpiryBackgroundService`) và hỗ trợ nối lại / tạo mới phiên thanh toán (`POST /api/sepay/payment/{orderId}/resume`).
- **Hậu mãi, Media & Báo cáo:** Quản lý đánh giá + phản hồi, yêu cầu hoàn tiền, phiếu bảo hành, chữ ký upload Cloudinary, nhật ký thao tác (`AuditLog`) và báo cáo doanh thu / nhập–xuất kho theo kỳ.

---

## Công nghệ sử dụng

- **Nền tảng & Ngôn ngữ:** .NET 10, ASP.NET Core Web API, C#
- **CSDL & ORM:** Microsoft SQL Server (LocalDB / SQL Server), Entity Framework Core 10
- **Cache & Session Token:** Redis (`StackExchange.Redis`)
- **AI cục bộ:** `SmartComponents.LocalEmbeddings` (ONNX Runtime, `bge-micro-v2`)
- **Bảo mật & Tích hợp:** JWT Bearer, Google OAuth, BCrypt, MailKit + RazorLight (email HTML), Cloudinary, SePay / VietQR
- **Tiện ích & Tài liệu API:** AutoMapper, Swagger / OpenAPI (`Swashbuckle.AspNetCore`)

---

## Hướng dẫn bắt đầu

### 1. Điều kiện tiên quyết (Prerequisites)

- **.NET 10 SDK** (`10.0+`)
- **SQL Server** hoặc **SQL Server LocalDB**
- **Redis Server** chạy tại `localhost:6379` (có thể chạy qua Docker: `docker run -d -p 6379:6379 redis:7-alpine`)
- Công cụ EF Core CLI *(nếu cần chạy migration)*:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2. Cài đặt (Installation)

```bash
git clone https://github.com/kientruong392/Agitek-BE.git
cd Agitek-BE/AgitekBackend
dotnet restore
```

### 3. Cấu hình (Configuration)

Cập nhật các thông số trong `AgitekBackend/appsettings.Development.json` hoặc cấu hình an toàn bằng **User Secrets** (khuyên dùng cho các khóa nhạy cảm):

```bash
dotnet user-secrets set "GoogleAuth:ClientId" "<your-google-client-id>"
dotnet user-secrets set "GoogleAuth:ClientSecret" "<your-google-client-secret>"
dotnet user-secrets set "SePaySettings:SecretKey" "<your-sepay-webhook-secret>"
```

Các nhóm cấu hình chính trong `appsettings.json` / `appsettings.Development.json`:
- `ConnectionStrings:AgitekConnection`: chuỗi kết nối SQL Server
- `Redis:ConnectionString`: địa chỉ Redis (mặc định `localhost:6379,abortConnect=false`)
- `JwtSettings`: `SecretKey`, `Issuer`, `Audience`, `ExpiryInMinutes`, `RefreshTokenTtlSeconds`
- `GoogleAuth`: `ClientId`, `ClientSecret`
- `EmailSettings`: thông số SMTP (`Host`, `Port`, `Username`, `Password`, `From`)
- `Cloudinary`: `CloudName`, `ApiKey`, `ApiSecret`
- `VietQRSettings` & `SePaySettings`: tài khoản ngân hàng nhận tiền và khóa webhook SePay
- `Frontend:Url`: địa chỉ Frontend (mặc định `http://localhost:3000`)

Áp dụng migration để tạo cơ sở dữ liệu (lần đầu chạy):
```bash
dotnet ef database update
```

### 4. Chạy dự án (Running)

```bash
dotnet run
```

- API mặc định lắng nghe tại **`http://localhost:5106`** (hoặc `https://localhost:7241` với profile `https`).
- Trang tài liệu **Swagger UI** mở trực tiếp tại [http://localhost:5106](http://localhost:5106) ở môi trường `Development`.
- Khi khởi động lần đầu, hệ thống tự động seed dữ liệu mẫu (`SeedDataAsync`) và chạy nền tạo vector tìm kiếm cho sản phẩm.
