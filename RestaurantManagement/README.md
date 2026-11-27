# Restaurant Management

Ứng dụng WinForms đăng nhập cho quản lý nhà hàng, tổ chức theo các project DTO, DAL, BUS, DB và GUI. Solution dùng .NET 8 và SQL Server.

## Chuẩn bị cơ sở dữ liệu

1. Mở `RestaurantManagement.DB/init.sql` trong SQL Server Management Studio và chạy script để tạo database `RestaurantManagement`, bảng `dbo.Managers` và bảng bàn ăn `dbo.BanAn` theo ERD.
2. Mở file `database.connection.json` trong thư mục `RestaurantManagement` và sửa chuỗi `Server` cho đúng SQL Server của bạn. Mẫu hiện tại dùng Windows Authentication với server `DESKTOP-TT7U6R7`:

   ```json
   {
     "ConnectionStrings": {
       "RestaurantManagement": "Server=DESKTOP-TT7U6R7;Database=RestaurantManagement;Integrated Security=true;TrustServerCertificate=true"
     }
   }
   ```

   Nếu dùng SQL Server Express có thể dùng `Server=DESKTOP-TT7U6R7\\SQLEXPRESS;Database=RestaurantManagement;Integrated Security=true;TrustServerCertificate=true`. Tài khoản Windows cần quyền truy cập database. Nếu muốn ghi đè cấu hình file, có thể đặt biến môi trường `RESTAURANT_DB_CONNECTION`.

## Tạo tài khoản quản lý đầu tiên

Chạy lệnh sau từ thư mục `RestaurantManagement` sau khi cấu hình kết nối:

```powershell
dotnet run --project .\RestaurantManagement.DB
```

Chương trình hỏi tên đăng nhập, họ tên và mật khẩu (tối thiểu 12 ký tự; ký tự mật khẩu không hiển thị). Công cụ chỉ tạo tài khoản khi chưa có bản ghi nào trong `dbo.Managers`. Mật khẩu được lưu dưới dạng PBKDF2-HMAC-SHA256 với salt ngẫu nhiên.

Tài khoản ở LocalDB không tự được sao chép sang SQL Server khác. Nếu `dbo.Managers` trên server đang cấu hình chưa có tài khoản, hãy chạy lệnh khởi tạo trên để tạo tài khoản đầu tiên trong đúng database đó.

## Chạy ứng dụng

```powershell
dotnet run --project .\RestaurantManagement.GUI
```

Đăng nhập bằng tài khoản quản lý vừa khởi tạo. Sau khi xác thực thành công, ứng dụng mở thẳng màn hình quản lý bàn ăn; đóng màn hình hoặc chọn đăng xuất sẽ quay lại đăng nhập. Tài khoản bị vô hiệu hóa (`IsActive = 0`) không thể đăng nhập. File cấu hình được copy vào thư mục chạy của GUI và công cụ khởi tạo DB khi build.

## Quản lý bàn ăn

Màn hình quản lý bàn ăn hỗ trợ xem danh sách, tìm theo mã bàn/khu vực/trạng thái, xem chi tiết, thêm, chỉnh sửa khu vực và sức chứa, cập nhật trạng thái, và xóa sau khi xác nhận. Mã bàn là khóa chính; trạng thái gồm `Trống`, `Đã đặt`, `Đang phục vụ`, `Đang dọn`. Sức chứa phải lớn hơn 0. Chỉ xóa bàn đang `Trống`; khóa ngoại của dữ liệu liên quan cũng sẽ ngăn xóa bàn đang được tham chiếu.

Nếu database đã được tạo từ trước, chạy lại toàn bộ `RestaurantManagement.DB/init.sql` trong SSMS để bổ sung bảng `dbo.BanAn`; script giữ nguyên dữ liệu và bảng đã có.
