# Cài đặt từ bản clone sạch

Mục tiêu: Windows, Visual Studio 2022 (workload **ASP.NET and web development**, .NET Framework **4.7.2 targeting pack**), IIS Express, CouchDB 3.x, Python 3.9+ trên PATH. Giữ ASP.NET MVC 5 và Newtonsoft.Json trong packages.config; không cài EF/SQL/MongoDB/Docker để thay DB.

1. Clone repository được chủ repo cho phép vào thư mục làm việc; checkout `codex/nosql-completion`. Không đổi `master` thành `main`. Chủ repo phải đổi credential cũ, chuyển Private và phối hợp xử lý lịch sử trước khi tiếp tục chia sẻ; các commit hiện tại không làm sạch lịch sử đã lộ.
2. Mở `QuanLyBanHang.sln` trong VS 2022. Chọn **Restore NuGet Packages** ở solution; nếu cần bật NuGet restore trong Tools → Options. Thư mục packages được tái tạo theo packages.config. Nếu NuGet bị chặn mạng, ghi BLOCKED; không giả build thành công hoặc commit DLL/bin/obj.
3. Cài/chạy CouchDB; mở `http://127.0.0.1:5984/_utils/`, hoàn tất Setup single-node bằng tài khoản quản trị riêng. Tài khoản cấu hình trong phiên này đọc/ghi DB được nhưng `/_membership` 403, nên sinh viên vẫn phải xác minh Setup. Không tự mở quyền anonymous.
4. Trong Windows Environment Variables → User variables, tự nhập 4 biến theo `.env.example`: COUCHDB_URL (loopback HTTP hoặc HTTPS từ xa, không có userinfo), COUCHDB_DATABASE (`qlbanhang`), COUCHDB_USERNAME, COUCHDB_PASSWORD. Không gõ mật khẩu vào lệnh/screenshot/chat. `.env` không được ứng dụng tự nạp. Đóng/mở lại VS/IIS để nhận biến mới.
5. PowerShell khởi tạo (chỉ với tài khoản được cấp quyền tạo DB/index):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-CouchDb.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/With-CouchEnvironment.ps1 -Script scripts/Seed-Demo.py
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-CouchDbConnection.ps1
```

Initializer dùng Process variables; terminal mới cần nhận biến từ Windows. Wrapper Python có thể lấy biến User khi Process thiếu, chỉ nạp cho tiến trình. Không thay Machine/User variables. Chạy seed lại không xóa/ghi đè ID cũ; 409 giữ nguyên. Bộ P2 có 100 tài liệu, cộng 4 smoke `demo-*` nếu đã chạy initializer.

6. Tạo Administrator website bằng công cụ, nhập mật khẩu **riêng** khi được hỏi:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/With-CouchEnvironment.ps1 -Script scripts/Create-AppUser.py
```

Chọn role Administrator; không dùng credential CouchDB làm mật khẩu website. Tool chỉ tạo ID chưa có, không reset tài khoản tồn tại. Không có tài khoản/mật khẩu mặc định. App_user không được đưa vào snapshot nghiệp vụ.

7. Trong Developer PowerShell build cả Razor:

```powershell
msbuild QuanLyBanHang.sln /t:Build /p:Configuration=Debug /p:MvcBuildViews=true /verbosity:minimal /nologo
```

8. Đặt QuanLyBanHang làm Startup Project, chạy IIS Express bằng VS. Đăng nhập `/Account/Login`; tạo OrderManager/Shipper trong `/Account/Users`; làm theo USER_GUIDE.md. Cổng debug VS lấy từ Project Properties, không đồng nhất với cổng test.
9. Chạy `powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-AllTests.ps1` từ repo. Cần cổng 52517/52518 trống, không chạy hai suite đồng thời. Script tạo DB thử/restore riêng, giữ để đối chiếu; các mật khẩu thử ngẫu nhiên không được xuất. Không dừng/xóa DB chính để minh họa lỗi.

## Triển khai dùng chung

Publish Release tới IIS/Windows theo quy trình của nhóm; cấu hình HTTPS và quyền tiến trình riêng. Release transform bật requireSSL cho forms cookie và httpCookies, bỏ debug. Đặt credential ở môi trường của đúng tiến trình app pool, cấp quyền đọc/ghi DB cần thiết; không cấp admin máy chủ cho người dùng website. Không nhúng credential ở JavaScript hoặc Web.config tracked. Bảo vệ Fauxton/CouchDB qua quyền DB và mạng; người có credential trực tiếp DB có thể vượt quy tắc website.

ASP.NET Forms Authentication dùng khóa bảo vệ cookie của môi trường. Một máy có thể dùng khóa tự sinh; nếu triển khai nhiều máy phải quản lý machineKey giống nhau bằng cấu hình riêng được bảo vệ, không commit bí mật. Bộ đếm login thất bại trong bộ nhớ không thay giới hạn truy cập ở cổng công khai. HTTPS publish, multi-node và chứng chỉ **NOT RUN** trong phiên này; môi trường nghiệm thu là IIS Express loopback.

## Chẩn đoán

| Hiện tượng | Cách xử lý |
|---|---|
| Process thiếu biến, User đủ | mở lại VS/IIS/terminal; kiểm tra chỉ cờ configured, không liệt kê giá trị |
| 401/403 | kiểm tra tài khoản mới/quyền riêng trong Fauxton; không log raw body/header; sau nhiều lần sai có thể cần chờ cơ chế bảo vệ máy chủ |
| DB 404 | kiểm tra đúng tên và chạy initializer bằng tài khoản đủ quyền |
| Index/sort lỗi | chạy Seed-Demo.py; đối chiếu `_explain`, không xóa index có sẵn |
| 409 | tải lại tài liệu, đối chiếu thay đổi; không tự retry ghi đè |
| Order chờ đồng bộ | admin vào `/Sync`; xung đột bên ngoài cần đối chiếu, không xóa pending để che lỗi |
| Backup từ chối dữ liệu | đọc lỗi an toàn; xử lý dữ liệu không hợp lệ riêng, không dùng --force/overwrite |

Giá/tổng tiền hợp lệ: 0..999999999999.99, tối đa 2 số lẻ; nhập dấu `.` cho phần thập phân. Đây là hợp đồng số tiền của demo để tránh nhận các decimal quá lớn/quá nhiều số lẻ không thể giữ ổn định qua JSON/CouchDB.
