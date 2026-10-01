# Plan: Hoàn thiện Payment và refactor Order

## Bối cảnh và quyết định đã chốt

- `Payment` là module đầy đủ theo convention hiện có: DTO, mapper, configuration, DbSet, interface, service, controller, authorization requirement/handler và DI.
- Quan hệ một-nhiều: một `Order` có nhiều `Payment`; `Order.Payments` và `Payment.Order` là navigation hai chiều, `Payment.OrderId` là FK.
- Payment mới có `Pending`; cập nhật trạng thái chỉ cho phép `Pending -> Success` hoặc `Pending -> Failed`. Mọi trạng thái/transition khác trả `BadRequestException`.
- Tạo payment bắt buộc có `OrderId`; order phải tồn tại. Owner của order mới được tạo payment cho order đó.
- Khi tạo order, `OrderService` phải gọi `IPaymentService` để tạo payment. Order và payment phải nằm trong cùng EF transaction; payment lỗi thì rollback cả order.
- Không sửa migration/.NET system files trong phạm vi plan, theo yêu cầu người dùng. Do đó phải kiểm tra schema hiện tại và báo rõ nếu cần migration để deploy database; không tự chỉnh migration cũ.
- Không mở rộng sang các security/business vấn đề ngoài yêu cầu (giá sản phẩm, voucher, UTC...) trừ các sửa cần thiết để build và giữ flow hiện tại.

## Phạm vi file/module dự kiến

- `Models/Order.cs`, `Models/Payment.cs`
- `Data/AppDbContext.cs`, `Data/Configurations/PaymentConfiguration.cs`
- `DTOs/Payments/CreatePaymentDTO.cs`, `DTOs/Payments/UpdatePaymentDTO.cs`, `DTOs/Payments/PaymentDTO.cs`
- `DTOs/Mappers/PaymentMapper.cs`, `DTOs/Mappers/OrderMapper.cs`, `DTOs/Orders/OrderDTO.cs`
- `Services/Interfaces/IPaymentService.cs`, `Services/PaymentService.cs`, `Services/Interfaces/IOrderService.cs`, `Services/OrderService.cs`
- `Controllers/PaymentController.cs`, `Controllers/OrderController.cs`
- `Authorization/Requirements/PaymentManagementRequirement.cs`, `Authorization/Handlers/PaymentManagementHandler.cs`, `Authorization/Requirements/OrderManagementRequirement.cs`, `Authorization/Handlers/OrderManagementHandler.cs`
- `Program.cs`
- Chỉ tạo test nếu repo đã có convention/test project; nếu không, validation bằng build và kiểm tra flow.

## Trình tự triển khai

### 1. Chuẩn hóa model và persistence mapping

1. Giữ navigation hai chiều hiện có; kiểm tra nullability và FK trên `Payment.OrderId`.
2. Thêm `DbSet<Payment> Payments` vào `AppDbContext`.
3. Tạo `PaymentConfiguration` theo convention `IEntityTypeConfiguration<Payment>`:
   - khóa chính `Id`;
   - `OrderId` bắt buộc, quan hệ `Payment -> Order` nhiều-một, FK tới `Order.Payments`;
   - delete behavior phù hợp với relation Order hiện tại;
   - decimal `Amount` theo `decimal(18,2)`;
   - độ dài `TransactionId`/`GatewayResponse` theo model;
   - index `OrderId`; không đặt unique index vì một order được phép có nhiều payment.
4. Khi query order, eager-load `Payments`; khi query payment, eager-load `Order` để authorization truy chủ order.
5. Loại bỏ hoặc thay thế các field legacy `PaymentMethod`, `PaymentStatus`, `TransactionId` khỏi `OrderDTO` nếu chúng không còn tồn tại trên entity; thay bằng collection `Payments` (hoặc mapping payment collection theo DTO). Không để AutoMapper trả enum mặc định giả.
6. Xóa tham chiếu compile lỗi `order.PaymentStatus = PaymentStatus.Unpaid` trong OrderService; trạng thái khởi tạo nằm trên payment là `Pending`.

### 2. Xây dựng Payment module theo style các module hiện có

1. DTO:
   - `CreatePaymentDTO`: `OrderId` bắt buộc; các dữ liệu tạo payment phù hợp model (`Amount` nếu nghiệp vụ cho phép client truyền thì service phải đối chiếu order, không tin giá trị tùy ý; `PaymentMethod`, `TransactionId`, `GatewayResponse` theo convention hiện tại).
   - `UpdatePaymentDTO`: chỉ chứa `Status`; không cho sửa `OrderId`, `Amount`, `PaymentMethod` hoặc transaction metadata.
   - `PaymentDTO`: expose id, order id, amount, method, status, transaction/gateway fields, expiry/paid timestamps và navigation representation cần thiết; tránh vòng lặp JSON bằng DTO, không expose entity navigation trực tiếp.
2. `PaymentMapper` map Create -> Payment, Update -> Payment có điều kiện null nếu convention yêu cầu, Payment -> PaymentDTO. Status do service kiểm soát, không để request override ngoài rule.
3. `IPaymentService` gồm các operation cần dùng:
   - tạo payment cho order trong transaction hiện tại;
   - đọc payment theo id và/hoặc theo order id;
   - cập nhật status.
   Interface phải cho phép `OrderService` truyền transaction context dùng chung, không tự commit transaction độc lập trong lúc tạo order.
4. `PaymentService`:
   - create: reject thiếu/không hợp lệ `OrderId` bằng `BadRequestException`, reject order không tồn tại bằng exception theo convention, lấy owner/order amount từ DB, tạo status `Pending`, không tạo payment nếu dữ liệu order không hợp lệ;
   - read: trả DTO với `Order` được include;
   - update: chỉ nhận status; nếu status hiện tại không phải Pending hoặc status mới không phải Success/Failed thì BadRequest; khi Success set `PaidAt`, Failed không set PaidAt; không cho sửa các trường khác.
5. Controller theo pattern `Authorize`, `IAuthorizationService`, `GetCurrentUser`, custom exceptions và response wrapper:
   - `GET` payment (id/order) chỉ qua authorization handler;
   - `POST /api/Payment` authorize Create, kiểm tra owner order trong handler/resource, rồi gọi service;
   - `PATCH /api/Payment/{id}` authorize Update và chỉ bind `UpdatePaymentDTO`.

### 3. Payment authorization

1. Tạo `PaymentOperation` (`Read`, `Create`, `Update`) và `PaymentManagementRequirement` theo mẫu Order.
2. `PaymentManagementHandler` lấy payment/order owner từ resource (`PaymentDTO`, `Payment`, hoặc order id đã load). Create phải xác định order trước khi authorize; chỉ current user đúng `Order.UserId` được tạo payment.
3. Read/Update giữ đúng rule dự án hiện tại và yêu cầu người dùng: tối thiểu owner được thao tác payment thuộc order của mình; không tự mở rộng role nếu chưa có rule tương ứng. Nếu dùng manager/admin/superadmin override theo convention module khác, ghi rõ và áp dụng nhất quán.
4. Parse role case-insensitive như controller để tránh deny sai. Đăng ký handler với lifetime tương thích dependency (nếu handler chỉ dùng claims có thể giữ pattern hiện tại; nếu inject scoped DbContext thì dùng scoped/transient và không singleton).

### 4. Refactor OrderService tạo payment atomically

1. Bỏ `BaseQuery()`; chuyển logic eager-loading thành query rõ ràng trong từng method hoặc private query helper không mang tên BaseQuery nếu helper đó không phải nghiệp vụ dùng chung.
2. `CreateAsync`:
   - giữ validation order detail/address/voucher hiện có;
   - map và tính order như cấu trúc hiện tại, loại bỏ `PaymentStatus.Unpaid`;
   - bắt đầu EF transaction;
   - add order và save để có `order.Id`;
   - gọi `IPaymentService.CreateAsync` với `OrderId`, `PaymentMethod`, `TransactionId`, amount từ order và transaction context hiện tại;
   - commit khi cả hai thành công; exception tự rollback/explicit rollback;
   - query lại order kèm `Payments` và trả DTO.
3. Constructor OrderService inject `IPaymentService`.
4. Đảm bảo PaymentService không tự commit/rollback transaction khiến transaction lồng nhau; sử dụng cùng `DbContext` scoped và transaction do OrderService điều phối.
5. Các query `GetByIdAsync`/GetAll đều include `Payments` để truy vết hai chiều ở service layer (DTO vẫn tránh circular reference).

### 5. Refactor GetAll Order và authorization

1. Đổi `IOrderService.GetAllAsync` để bắt buộc `userId` (không nullable, không nhận `CurrentUser` để service tự quyết role).
2. `GetAllAsync(userId)` chỉ query `Orders.Where(o => o.UserId == userId)`, luôn trả đúng order của user id được truyền vào; không trả toàn bộ theo role trong service.
3. OrderController `GetAll` lấy current user và authorize bằng `OrderManagementRequirement` có resource chứa user id/request target.
4. Order requirement/handler thêm operation phù hợp (`ReadAll` hoặc dùng Read với resource user id) và chuyển toàn bộ quy tắc vào handler:
   - user là chủ của user id truyền vào được getall order của chính mình;
   - Manager/Admin/SuperAdmin được getall toàn bộ order của user id truyền vào;
   - các role khác bị từ chối;
   - Staff/role khác không được ngầm hưởng quyền nếu yêu cầu không cho phép.
5. Controller từ chối thiếu/không hợp lệ current user bằng Unauthorized; authorization fail bằng ForbiddenException; chỉ sau đó gọi service với `userId` đã được authorize.

### 6. Bỏ `static` không cần thiết trong service

1. Rà soát toàn bộ `Services/*.cs` và chỉ đổi các helper `static` khi helper đó chỉ phục vụ nghiệp vụ nội bộ của chính service (ví dụ `OrderService.BuildAddressSnapshot`).
2. Không đổi các hàm static dùng chung giữa service/module hoặc static utility ngoài phạm vi yêu cầu.
3. Giữ accessibility/private và style hiện tại; tránh refactor lan rộng.

### 7. DI, mapping và compile consistency

1. Đăng ký `IPaymentService -> PaymentService` scoped trong `Program.cs`.
2. Đăng ký Payment authorization handler theo cùng convention nhưng bảo đảm lifetime không giữ scoped dependency trong singleton.
3. AutoMapper assembly scan hiện có sẽ tự nhận `PaymentMapper`; không thêm cơ chế đăng ký khác.
4. Cập nhật các mapping Order để dùng Payments DTO và không còn map các field legacy không tồn tại.
5. Kiểm tra mọi nơi gọi `GetAllAsync` sau khi đổi signature.

## Transaction và lỗi

- Transaction bao quanh insert Order + insert Payment. Bất kỳ `BadRequestException`, `NotFoundException`, lỗi authorization sau khi bắt đầu transaction, hoặc lỗi DB đều không commit; order không được lưu nếu payment thất bại.
- Authorization tạo payment phải diễn ra trước insert payment; authorization đọc/cập nhật phải load payment kèm order để xác định owner.
- Không cho phép client dùng status khác Pending lúc tạo hoặc chuyển từ Success/Failed ngược về Pending.
- Nhiều payment trên cùng order không bị unique constraint; mỗi payment là một bản ghi attempt riêng.

## Database/migration constraint

- Người dùng yêu cầu không quan tâm/không chỉnh migration. Vì Payment hiện chưa có `DbSet`/configuration và schema migration cũ có thể chưa có bảng Payments, implementation chỉ chuẩn bị model mapping/code. Cần ghi rõ khi validation nếu database hiện tại chưa có bảng Payment thì runtime sẽ yêu cầu migration/schema deployment riêng; không tự sửa migration cũ trong task này.

## Validation sau triển khai

1. `dotnet build` để bắt compile errors, đặc biệt PaymentStatus.Unpaid, DTO mappings, DI và interface callers.
2. Kiểm tra endpoint contract:
   - create payment thiếu `OrderId` -> 400;
   - user không sở hữu order -> 403;
   - create payment hợp lệ -> Pending;
   - update Pending -> Success/Failed -> thành công;
   - update trạng thái khác hoặc transition không hợp lệ -> 400;
   - đọc payment/order thể hiện navigation dữ liệu qua DTO, không vòng lặp JSON;
   - order create tạo order + payment; mô phỏng lỗi payment xác nhận rollback order.
3. Kiểm tra GetAll:
   - customer/user owner chỉ nhận order của user id đó;
   - manager/admin/superadmin nhận order của user id được truyền;
   - role khác 403;
   - không có endpoint path nào khiến service trả orders của user khác do role.
4. Kiểm tra `git diff` chỉ chứa module Payment, Order refactor, authorization/DI/configuration cần thiết; không chạm migration/system files.
