# Nhật ký tiến độ Codex – QuanLyBanHang_Nosql

Ngày khởi tạo hồ sơ: 28/10/2026. **Trạng thái: kế hoạch đã soạn, chưa triển khai mã sửa trong repo.**

## Baseline – mã nguồn hiện tại (`master`)
- [x] Có project ASP.NET MVC5 /.NET Framework 4.7.2 và các controller CRUD cơ bản.
- [x] Đã đọc mã nguồn tĩnh các Model, Controller, Views, service CouchDB.
- [ ] Chưa xác thực build/test trong phiên kiểm tra này (người dùng báo Rebuild OK trên máy).
- [ ] Chưa có database `qlbanhang` trên Fauxton máy người dùng; Home/Index đang lỗi.
- [ ] Credential CouchDB đã bị hardcode và repo đã từng Public: chủ repo cần đổi mật khẩu và bảo vệ repo.

## Tiến độ theo giai đoạn
| Giai đoạn | Trạng thái | PR/Commit | Bằng chứng kiểm thử | Ghi chú |
|---|---|---|---|---|
| P0 – Môi trường/bảo mật | Chưa làm | — | — | Ưu tiên |
| P1 – Lỗi nền tảng/CRUD | Chưa làm | — | — | — |
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
