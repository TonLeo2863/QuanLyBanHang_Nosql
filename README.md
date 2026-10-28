# Chạy ứng dụng trên Windows

Đồ án quản lý đơn hàng và giao hàng – nhóm 7. Giữ ASP.NET MVC 5, .NET Framework 4.7.2, C#, Newtonsoft.Json, Visual Studio 2022 và CouchDB 3.x. Nhánh phát triển `codex/nosql-completion`, nhánh gốc `master`.

1. Chủ repo cần đổi mật khẩu CouchDB đã lộ, chuyển repo sang Private và phối hợp xử lý lịch sử Git trước khi chia sẻ tiếp. Không dùng lại mật khẩu cũ.
2. Cài workload ASP.NET và targeting pack .NET Framework 4.7.2. Restore NuGet rồi mở solution trong VS 2022.
3. Trong Fauxton ở http://127.0.0.1:5984/_utils/, hoàn tất Setup single node bằng tài khoản quản trị mới. Không đưa thông tin đăng nhập vào URL.
4. Thiết lập bốn biến môi trường COUCHDB_URL, COUCHDB_DATABASE, COUCHDB_USERNAME, COUCHDB_PASSWORD theo tên trong `.env.example`. Giá trị tài khoản/mật khẩu để trống trong mẫu: nhập riêng trên máy qua giao diện Environment Variables của Windows. Ứng dụng không tự đọc file .env. Khởi động lại Visual Studio/IIS Express sau khi thay đổi biến.
5. Dùng tài khoản có quyền để chạy `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-CouchDb.ps1` trong phiên đã có biến môi trường; dùng tài khoản được cấp quyền database cho ứng dụng. Bypass chỉ cho tiến trình lệnh này, không đổi policy máy. Script không đổi cấu hình single-node/mật khẩu, không xóa database/tài liệu hoặc ghi đè ID có sẵn.
6. Chạy seed và tạo tài khoản Administrator website bằng mật khẩu nhập riêng khi công cụ hỏi; không có mật khẩu mặc định và không dùng login CouchDB cho website:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/With-CouchEnvironment.ps1 -Script scripts/Seed-Demo.py
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/With-CouchEnvironment.ps1 -Script scripts/Create-AppUser.py
```

7. Build gồm Razor, khởi động IIS Express trong VS, đăng nhập `/Account/Login`. Admin tạo OrderManager/Shipper trong `/Account/Users`. Các trang nghiệp vụ yêu cầu đăng nhập; lỗi DB khi xác thực hiển thị thông báo có kiểm soát, không mở quyền anonymous để demo.

```powershell
msbuild QuanLyBanHang.sln /t:Build /p:Configuration=Debug /p:MvcBuildViews=true /verbosity:minimal /nologo
```

Không commit cấu hình riêng. Không chụp màn hình mật khẩu hoặc Authorization header. P0/P1 chỉ được nghiệm thu sau khi chạy build, Razor và CRUD với CouchDB thực tế; xem CODEX_PROGRESS.md.

## Kiểm thử

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-AllTests.ps1
```

Cần Python 3.9+ và cổng 52517/52518 còn trống. Suite tạo DB thử/restore riêng, giữ để đối chiếu, không xóa DB chính. Snapshot trong artifacts bị gitignore. Xem [tests/README.md](tests/README.md).

## Chức năng và tài liệu bàn giao

Seed giả gồm 20 khách, 30 sản phẩm, 30 đơn nhiều item, 20 giao hàng và 9 index; chạy lại giữ nguyên ID cũ. Ứng dụng có ba vai trò, luồng xác nhận–chuẩn bị–giao–hoàn tất/thất bại; revision, durable outbox và retry; Mango tìm/lọc/bookmark, Shipping theo người giao và Dashboard. Công cụ CLI export/import/verify snapshot logic vào DB thử riêng.

- [Thiết kế NoSQL](docs/DATABASE_DESIGN.md), [10 truy vấn Mango](docs/mango-queries.json).
- [Demo Fauxton và so sánh công cụ](docs/FAUXTON_DEMO.md), [ảnh cần chụp](docs/SCREENSHOT_CHECKLIST.md).
- [Hướng dẫn sử dụng](docs/USER_GUIDE.md), [triển khai từ clone sạch](docs/DEPLOYMENT.md).
- [Backup/import/restore và giới hạn](docs/BACKUP_RESTORE.md), [kết quả kiểm thử](docs/TEST_RESULTS.md).
- [Nhật ký](CODEX_PROGRESS.md), [rubric](RUBRIC_CHECKLIST.md).

Snapshot nghiệp vụ không gồm người dùng/quyền/cấu hình/lịch sử revision/attachments của hệ thống. Order–Delivery dùng durable outbox, không phải transaction xuyên document; admin xử lý đồng bộ chờ ở `/Sync`. Single-node cần sinh viên xác minh bằng quyền quản trị; GUI/ảnh và publish HTTPS không được tính PASS chỉ từ API. Không push/merge hoặc tự viết lại lịch sử trong phiên này.
