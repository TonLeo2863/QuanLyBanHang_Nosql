# Chạy ứng dụng trên Windows

Giữ ASP.NET MVC 5, .NET Framework 4.7.2, Visual Studio 2022 và CouchDB 3.x.

1. Chủ repo cần đổi mật khẩu CouchDB đã lộ, chuyển repo sang Private và phối hợp xử lý lịch sử Git trước khi chia sẻ tiếp. Không dùng lại mật khẩu cũ.
2. Cài workload ASP.NET và targeting pack .NET Framework 4.7.2. Restore NuGet rồi mở solution trong VS 2022.
3. Trong Fauxton ở http://127.0.0.1:5984/_utils/, hoàn tất Setup single node bằng tài khoản quản trị mới. Không đưa thông tin đăng nhập vào URL.
4. Thiết lập bốn biến môi trường COUCHDB_URL, COUCHDB_DATABASE, COUCHDB_USERNAME, COUCHDB_PASSWORD theo tên trong `.env.example`. Giá trị tài khoản/mật khẩu để trống trong mẫu: nhập riêng trên máy qua giao diện Environment Variables của Windows. Ứng dụng không tự đọc file .env. Khởi động lại Visual Studio/IIS Express sau khi thay đổi biến.
5. Dùng tài khoản quản trị riêng để chạy `powershell -File scripts/Initialize-CouchDb.ps1` trong phiên đã có biến môi trường; dùng tài khoản được cấp quyền database cho ứng dụng. Script không đổi cấu hình single node hay mật khẩu, không xóa database/tài liệu, không ghi đè tài liệu đã có.
6. Chạy ứng dụng và kiểm tra sáu trang Home, Customer, Product, Order, Delivery, Shipping. Khi thiếu cấu hình, mất kết nối hoặc HTTP 401/403/404/409, ứng dụng phải hiện thông báo có kiểm soát.

Không commit cấu hình riêng. Không chụp màn hình mật khẩu hoặc Authorization header. P0/P1 chỉ được nghiệm thu sau khi chạy build, Razor và CRUD với CouchDB thực tế; xem CODEX_PROGRESS.md.

## Kiểm thử

Xem [tests/README.md](tests/README.md) để build cả Razor, chạy smoke test giả lập và thực hiện checklist CRUD/CouchDB thật. Truy vấn chính dùng Mango `_find` có bookmark và index `type-id`; script khởi tạo phải chạy trước khi dùng database mới. Bốn tài liệu mẫu chỉ phục vụ smoke test P0/P1, chưa đáp ứng quy mô dữ liệu P2. Ứng dụng hiện chưa có đăng nhập/phân quyền P4.
