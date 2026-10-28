# Nhật ký tiến độ Codex – QuanLyBanHang_Nosql

Ngày khởi tạo hồ sơ: 28/10/2026. **Trạng thái: đã triển khai mã P0/P1; build/Razor và kiểm thử giả lập/IIS đã chạy. Chưa nghiệm thu CRUD với CouchDB thật.**

## Baseline – mã nguồn hiện tại (`master`)
- [x] Có project ASP.NET MVC5 /.NET Framework 4.7.2 và các controller CRUD cơ bản.
- [x] Đã đọc mã nguồn tĩnh các Model, Controller, Views, service CouchDB.
- [ ] Chưa xác thực build/test trong phiên kiểm tra này (người dùng báo Rebuild OK trên máy).
- [ ] Chưa có database `qlbanhang` trên Fauxton máy người dùng; Home/Index đang lỗi.
- [ ] Credential CouchDB đã bị hardcode và repo đã từng Public: chủ repo cần đổi mật khẩu và bảo vệ repo.

## Tiến độ theo giai đoạn
| Giai đoạn | Trạng thái | PR/Commit | Bằng chứng kiểm thử | Ghi chú |
|---|---|---|---|---|
| P0 – Môi trường/bảo mật | Đã sửa mã; chờ xác minh môi trường thật | fa57030 | Cú pháp/script giả lập đạt; CouchDB HTTP 200, DB 404 | Chưa có cấu hình đăng nhập mới |
| P1 – Lỗi nền tảng/CRUD | Đã sửa mã; chờ CRUD thật | Commit chứa bản ghi P1 này | Build/Razor đạt; 44 assertion; IIS 7 GET + 4 POST invalid + CSRF | Chưa nghiệm thu toàn bộ P1 |
| P2 – DB/seed/schema | Chưa làm | — | — | — |
| P3 – Mango/Fauxton query | Chưa làm | — | — | — |
| P4 – Nghiệp vụ/roles | Chưa làm | — | — | — |
| P5 – Backup/tests | Chưa làm | — | — | — |
| P6 – Báo cáo/demo | Chưa làm | — | — | — |

## Mẫu bản ghi sau MỖI LẦN CẬP NHẬT
### YYYY-MM-DD – Giai đoạn PX – [Tên]
- **Đã làm:**
- **Files thay đổi:**
- **Liên quan rubric:**
- **Build/test:** Lệnh chạy; môi trường; thành công/thất bại/chưa chạy; số lượng test.
- **Bằng chứng:** liên kết PR/commit, log được che thông tin nhạy cảm, ảnh demo.
- **Còn thiếu / lỗi đã biết:**
- **Bước tiếp theo:**
- **Comment tiếng Việt sau push:** [link GitHub comment hoặc ghi chưa có quyền comment].

### 2026-10-28 – Giai đoạn P0 – Cấu hình kết nối an toàn
- **Đã làm:** bỏ cấu hình/xác thực cố định; dùng bốn biến môi trường; kiểm tra HTTP status trước khi trả JSON, mã hóa ID/rev, timeout 15 giây; thông báo lỗi không chứa phản hồi máy chủ. Thêm script khởi tạo DB/index/bốn tài liệu giả không ghi đè dữ liệu.
- **Files thay đổi:** Services/CouchDbService.cs, .gitignore, .env.example, README.md, scripts/Initialize-CouchDb.ps1.
- **Liên quan rubric:** 5a, nền tảng 5b, 8. Chưa đủ bộ dữ liệu P2.
- **Build/test:** đã đọc toàn bộ bốn tài liệu, kiểm tra nhánh và trạng thái Git; chưa chạy build/CouchDB ở bước P0. Controller sẽ được bổ sung xử lý exception trong P1.
- **Còn thiếu:** chủ repo đổi mật khẩu, Private repo và quyết định xử lý lịch sử; chưa xác minh single-node/quyền trên máy chủ; chưa chạy script lên CouchDB.
- **Bước tiếp theo:** P1: xử lý lỗi ở controller, validation, anti-forgery và kiểm thử.
- **Comment tiếng Việt sau push:** chưa push; sẽ chuẩn bị bình luận với SHA sau commit.

### 2026-10-28 – Giai đoạn P1 – Ổn định kết nối, Razor và CRUD
- **Khôi phục sau gián đoạn:** kiểm tra git status, nhánh, log, diff và đọc lại toàn bộ AGENTS/ROADMAP/RUBRIC/PROGRESS. Giữ nguyên thay đổi chưa commit, không đụng các tài liệu/thư mục .github chưa tracked có sẵn. Nhánh vẫn codex/nosql-completion; P0 đã commit fa57030.
- **Đã làm:** sửa orderDate ở Create/Delete và cú pháp if trong Order/Edit; bỏ parse dynamic rows, dùng kết quả typed Mango _find theo type/status với sort type/_id và bookmark 200/trang. Sáu controller hiển thị lỗi đã kiểm soát; CRUD có ModelState validation, anti-forgery ở toàn bộ 12 POST và 12 form, kiểm tra loại document, revision và lỗi trùng mã. Đơn hàng kiểm tra tham chiếu khách/sản phẩm, ngày, số lượng/giá/tổng; giao hàng kiểm tra tham chiếu đơn. Cập nhật giữ trường phụ, thay mảng đúng quy tắc và từ chối revision cũ. Chặn ID dành riêng/dot path; bổ sung cấu hình URI giữ %2F trên .NET Framework. Seed đồng nhất trạng thái đơn/giao hàng; script chạy lại không ghi đè.
- **Files thay đổi:** Controllers/{Home,Customer,Product,Order,Delivery,Shipping}Controller.cs; Models/{Customer,Product,Order,Delivery}.cs; Services/CouchDbService.cs; 12 Views CRUD và Shared/_Layout.cshtml; Web.config; scripts/Initialize-CouchDb.ps1; README.md; tests/{SmokeTests.cs,SmokeTests.config,Run-SmokeTests.ps1,Test-Initializer.py,Test-WebSmoke.py,README.md}; CODEX_PROGRESS.md.
- **Liên quan rubric:** nền tảng 5a/5b, 8, 9b/9c/9d. Không tự đánh dấu đạt rubric; thiếu demo CouchDB/Fauxton thật và các phase sau.
- **Build/test thực tế (Windows, VS 2022 Community, .NET Framework 4.7.2):**
  - MSBuild QuanLyBanHang.sln /t:Build /p:Configuration=Debug /p:MvcBuildViews=true /verbosity:minimal /nologo: exit 0, gồm biên dịch Razor. Build ban đầu phát hiện lỗi Order/Edit và Order/Delete; đã sửa và build lại đạt.
  - powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-SmokeTests.ps1: exit 0, PASS 44 assertion: 401/403/404/409/500, timeout, mất kết nối, JSON sai/thiếu docs, DB rỗng, phân trang, encoding ID/rev, loại tài liệu sai, revision cũ, bảo toàn trường phụ, validation, 12 POST anti-forgery, sáu controller thiếu cấu hình. HTTP giả lập, không phải CRUD CouchDB thật. Test ban đầu phát hiện URI giải mã %2F; đã bổ sung schemeSettings và test lại đạt.
  - python tests/Test-Initializer.py: exit 0; script khởi tạo chạy hai lần trên HTTP stub, bốn tài liệu liên kết hợp lệ/tổng đúng, index tồn tại, giữ nguyên dữ liệu đã có, 401 dừng với thông báo không lộ giá trị xác thực thử.
  - python tests/Test-WebSmoke.py qua IIS Express localhost:52515: exit 0; Home/Customer/Product/Order/Delivery/Shipping/Order-Create đều HTTP 200; bốn POST Create dữ liệu invalid hiển thị validation; POST thiếu CSRF bị từ chối HTTP 500 theo MVC5 mặc định. Không tạo/sửa/xóa dữ liệu thật. Lần thăm Home đầu tiên timeout 15 giây lúc app khởi động; chạy lại bộ web sau khởi động đạt cả bảy GET.
  - Script khởi tạo qua PowerShell parser: 0 lỗi cú pháp (P0). Lệnh chạy .ps1 ban đầu bị ExecutionPolicy chặn; chạy lại chỉ đặt Bypass cho tiến trình test, không sửa policy máy.
- **CouchDB thật đã kiểm tra chỉ đọc:** GET localhost:5984/ và /_up HTTP 200; /_session HTTP 200, phiên anonymous không có quyền _admin; /qlbanhang HTTP 404. Không có bốn biến COUCHDB_* trong Process/User/Machine. Không dùng lại credential cũ, không tự đổi mật khẩu/quyền, không khởi tạo DB thật khi chưa có cấu hình mới. Chưa chạy seed/CRUD/index trên CouchDB thật.
- **Còn thiếu:** chủ repo cần đổi mật khẩu cũ, Private repo và phối hợp làm sạch lịch sử trước khi chia sẻ. Chưa xác minh single-node/quyền bằng tài khoản mới; cần chạy checklist tests/README.md với DB thử riêng. Quy tắc trạng thái/role, ràng buộc tham chiếu khi xóa, đủ bộ seed và backup thuộc P2–P5, chưa hoàn thành. Kết quả Mango thật/chỉ mục và hiệu năng chưa kiểm chứng.
- **Bước tiếp theo:** thiết lập biến môi trường với credential mới trên máy; khởi động lại VS/IIS Express; chạy script hai lần trên DB demo riêng, kiểm thử CRUD/revision/lỗi theo tests/README.md; bổ sung ảnh Fauxton trước nghiệm thu P0/P1.
- **Kiểm tra trước commit:** git diff --check đạt; quét mẫu bí mật trên 35 file ứng viên không có phát hiện. Không in credential, không đưa bin/obj/.vs hoặc dữ liệu thật vào commit. Đây là quét mã hiện tại, không phải kiểm toán/làm sạch lịch sử Git.
- **Git/PR:** không push, không merge và không gửi bình luận PR theo yêu cầu mới nhất. Commit tiếng Việt sau diff/secret scan; hồ sơ AGENTS/ROADMAP/RUBRIC và .github có sẵn vẫn được giữ nguyên, chưa đưa vào commit này.
