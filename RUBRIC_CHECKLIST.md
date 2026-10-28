# Checklist thang điểm chính thức – Cơ sở dữ liệu NoSQL 15DH

**Nguồn đối chiếu:** PDF `DANH SACH DE TAI CUOI KY VA THANG DIEM_NOSQL (15DH) (Full).pdf`, trang 3–4, đề tài gợi ý số 2 trang 1. Đây là checklist công việc, không phải điểm tự chấm.

| STT | Tiêu chí | Điểm | Bằng chứng cần chuẩn bị | Đã kiểm chứng? |
|---|---|---:|---|---|
| 1 | Lý thuyết và cài đặt Fauxton | 0,5 | Hướng dẫn + ảnh màn hình | Tài liệu PASS; ảnh NOT RUN |
| 2 | Chức năng của Fauxton | 0,5 | Demo Database, Documents, Query, Index | Kịch bản PASS; GUI NOT RUN |
| 3 | So sánh Fauxton với GUI Tool khác | 0,5 | Bảng ưu/nhược, khác biệt phạm vi CSDL | Tài liệu/nguồn chính thức PASS |
| 4 | Điểm mạnh và loại ứng dụng phù hợp | 0,5 | Lập luận liên quan nghiệp vụ đơn/giao hàng | Tài liệu PASS |
| 5a | Tạo database | 0,5 | DB trong Fauxton + script tái lập | API/script thật PASS; ảnh NOT RUN |
| 5b | Insert dữ liệu đầy đủ | 0,5 | JSON seed + ảnh document | Seed 100 tài liệu PASS; ảnh NOT RUN |
| 5c | Dữ liệu đáp ứng truy vấn cơ bản | 0,5 | Query + kết quả minh chứng | 5 query API thật PASS |
| 5d | Dữ liệu đáp ứng truy vấn nâng cao | 0,5 | Mango selector/index + kết quả | 5 query/explain/bookmark PASS |
| 6 | Import/Export; Backup/Restore | 0,5 | Hướng dẫn, tệp demo + kết quả khôi phục | Snapshot logic/restore 104 doc PASS; backup toàn hệ thống NOT RUN |
| 7a | Truy vấn cơ bản trên GUI Tool | 0,5 | Kịch bản thực thi Fauxton | API PASS; GUI/ảnh NOT RUN |
| 7b | Truy vấn nâng cao | 0,5 | Kịch bản thực thi Fauxton | API PASS; GUI/ảnh NOT RUN |
| 8 | Kết nối CSDL với phần mềm | 0,5 | Code + demo HTTP CouchDB | Xác thực + CRUD/IIS thật PASS |
| 9a | Quản trị người dùng | 0,5 | Đăng nhập/phân quyền và kiểm thử | 3 vai trò/login/khóa user PASS |
| 9b | Thao tác CSDL (CRUD, backup...) | 0,5 | CRUD, backup có kiểm thử | CRUD/revision + snapshot logic PASS |
| 9c | Nghiệp vụ mô tả bài toán | 0,5 | Demo đơn hàng/giao hàng từ đầu đến cuối | Workflow/retry/outbox thật PASS |
| 9d | Chức năng hỗ trợ khác | 0,5 | Thống kê, tìm kiếm, lọc, lịch sử... | Dashboard/Shipping/Mango UI/lịch sử PASS; ảnh NOT RUN |
| 10a | Báo cáo đầy đủ, đúng định dạng, không sao chép | 0,5 | Word 50–60 trang | Chưa có file |
| 10b | Báo cáo lưu loát, trả lời giảng viên | 0,5 | Kịch bản và tập dượt | Chưa kiểm chứng |
| 10c | Nộp đúng hạn Word, PowerPoint, source | 0,5 | Bộ file nộp | Chưa kiểm chứng |
| 10d | Phân công nhóm rõ ràng và tích hợp | 0,5 | Bảng phân công, commit/PR, demo | Chưa kiểm chứng |
| | **TỔNG** | **10,0** | | |

## Các yêu cầu chung
- Tối đa 3 thành viên (nhóm hiện có 3).
- Bài báo cáo dài 50–60 trang gồm lý thuyết, thiết kế CSDL, ứng dụng demo, tài liệu tham khảo.
- Font Times New Roman 13, giãn dòng 1.5.
- Các nguồn chỉ được tham khảo, không sao chép.
- Điểm đánh giá theo từng thành viên.
- Yêu cầu 3 phần bất kể đề tài: nghiên cứu tool, thiết kế cơ sở dữ liệu, demo ứng dụng.

**Lưu ý quan trọng:** Tiêu chí 1–4 cộng 2 điểm, áp dụng vì đề tài sử dụng GUI Fauxton/CouchDB mới ngoài các công cụ cơ bản nêu trong tài liệu. Bằng chứng phải được kiểm tra trước khi chuyển trạng thái sang Hoàn thành.

## Bằng chứng P0/P1 cập nhật 28/10/2026 – chưa nghiệm thu toàn bộ rubric

Quy ước: **PASS** = ca đã chạy và đạt; **FAIL** = ca/điều kiện đã kiểm tra nhưng chưa đạt; **NOT RUN** = chưa chạy, không được suy ra là PASS. Các trạng thái dưới đây là bằng chứng kỹ thuật từng phần, không phải điểm tự chấm.

| Mục | Bằng chứng hiện có | Trạng thái và phần còn thiếu |
|---|---|---|
| 5a – Tạo database | Script idempotent; kiểm thử hai lần bằng HTTP giả lập PASS | Tạo DB thật NOT RUN vì thiếu xác thực; GET /qlbanhang chưa xác thực HTTP 404, điều kiện DB sẵn sàng FAIL |
| 5b – Insert dữ liệu | Bốn tài liệu giả có tham chiếu/tổng hợp lệ, test script giả lập PASS | Insert thật NOT RUN; chưa đủ quy mô dữ liệu P2 |
| 5c/5d – Dữ liệu/truy vấn | Code Mango/index/bookmark và kiểm thử handler giả lập PASS | Query/index trên CouchDB thật NOT RUN; chưa đủ bộ truy vấn/ảnh Fauxton |
| 8 – Kết nối CSDL | Health CouchDB thật HTTP 200 PASS; xử lý lỗi giả lập PASS | Bốn biến môi trường đều thiếu; kết nối có xác thực từ app NOT RUN |
| 9b – CRUD | Build/Razor PASS; 44 assertion giả lập PASS; validation/CSRF trên IIS PASS | CRUD thật bốn loại document và conflict _rev NOT RUN; backup thuộc phase sau |
| 9c – Nghiệp vụ | Có validation/tham chiếu đơn/giao hàng trong code | Demo xuyên suốt với DB thật NOT RUN; trạng thái/role đầy đủ chưa làm |
| 9d – Hỗ trợ | Bảy GET trên IIS HTTP 200 PASS trong tình huống thiếu cấu hình | Đối chiếu thống kê/Shipping với CouchDB thật NOT RUN; Shipping chỉ đọc/lọc Delivery |

Chi tiết lệnh, môi trường và kết quả: CODEX_PROGRESS.md, bản ghi “Kiểm tra điều kiện tích hợp P0/P1 và hướng dẫn thiết lập Windows”. Hướng dẫn thiết lập riêng trên Windows và checklist thao tác thật: tests/README.md. Chưa bắt đầu P2; chưa xác nhận đổi mật khẩu, Private repo hoặc xử lý lịch sử từ phía chủ repo.

## Bằng chứng mới nhất 28/10/2026 – tích hợp CouchDB thật

Bảng này cập nhật kết quả mới nhất, thay cho tình trạng thiếu cấu hình ở bản ghi trước; không tự chấm điểm hoặc xác nhận hoàn thành các mục còn thiếu.

| Mục | Kết quả thật | Phần còn thiếu |
|---|---|---|
| 5a | PASS tạo `qlbanhang` bằng script, đọc DB/index có xác thực HTTP 200; chạy lại an toàn | Ảnh Fauxton NOT RUN |
| 5b | PASS bốn seed liên kết; chạy lại giữ nguyên nội dung và revision | Bộ dữ liệu 10/20/20/15 của P2 NOT RUN |
| 5c/5d | PASS Mango cơ bản, nâng cao `$and/$in/$gte/$elemMatch`, sort/index và bookmark limit=1 không trùng/thiếu | Bộ 5+5 truy vấn, ảnh Fauxton và hiệu năng NOT RUN |
| 8 | PASS xác thực `_session` đúng danh tính; ứng dụng IIS đọc/ghi DB thật | Quyền hạn chế 403 thật NOT RUN; việc đổi mật khẩu/Private chưa xác nhận |
| 9b | PASS CRUD 4 loại qua form MVC/IIS và đối chiếu CouchDB; revision cũ, mã trùng, trường phụ, validation/CSRF | Backup/restore và quyền ứng dụng NOT RUN |
| 9c | PASS tạo khách/sản phẩm → đơn tổng tính server → Delivery; kiểm tra tham chiếu thiếu | Quy tắc chuyển trạng thái đầy đủ, ràng buộc xóa và role thuộc P4 chưa triển khai |
| 9d | PASS Dashboard 5 số đếm và Shipping số lượng/ID khớp DB; đổi delivered loại khỏi Shipping; 7 GET IIS HTTP 200 | Kiểm tra bố cục/ảnh GUI và lịch sử đầy đủ NOT RUN |

Build gồm Razor PASS; 44 assertion giả lập PASS; initializer/diagnostic giả lập PASS. IIS với DB không tồn tại và mật khẩu thử sai PASS cảnh báo có kiểm soát, validation/CSRF. Không có FAIL còn tồn tại trong bộ tự động đã chạy cuối phiên; các ca NOT RUN không được tính PASS. Xem CODEX_PROGRESS.md, bản ghi “Tích hợp P0/P1 với CouchDB thật và IIS Express”, và hướng dẫn chạy lại tests/README.md. Chưa chuyển sang P2, chưa tuyên bố đạt đủ rubric.

Kiểm tra quản trị bổ sung: `/_membership` trả HTTP 403 bằng tài khoản cấu hình (**FAIL quyền truy cập API quản trị**); single-node chưa được xác minh. Không tự nâng quyền; điều này không phủ định CRUD/DB/index đã PASS. Chủ repo cần kiểm tra Setup single-node bằng tài khoản quản trị trong Fauxton trước nghiệm thu P0 đầy đủ.

### P2 – bằng chứng mới
- PASS: 20 khách, 30 sản phẩm, 30 đơn nhiều item, 20 giao hàng giả; 9 index; seed chạy lại không đổi nội dung/revision; toàn bộ tham chiếu/tổng tiền trong bộ seed hợp lệ.
- PASS: Mango sort và `_explain` thực tế chọn `type-date`. Mục 5b đã có đủ quy mô mới theo yêu cầu; ảnh document/index NOT RUN. Nguồn bằng chứng: tests/Test-P2.py và docs/DATABASE_DESIGN.md. Chưa tự chấm điểm.

### P3 – Fauxton/Mango
- PASS tài liệu: FAUXTON_DEMO.md (10 thao tác, phạm vi GUI/API, so sánh Compass, ưu/nhược/ứng dụng và nguồn chính thức).
- PASS thực thi API: 5 query cơ bản + 5 nâng cao, số lượng seed đúng, _explain chọn đúng index; phân trang limit 7 đầy đủ/không trùng.
- NOT RUN: 7a/7b trên GUI, ảnh Fauxton theo SCREENSHOT_CHECKLIST.md; chưa đánh dấu toàn bộ mục GUI hoàn thành. Không có số liệu benchmark được tuyên bố.

### P4 – kiểm thử ứng dụng thật
- PASS 9a: PBKDF2/salt, đăng nhập/đăng xuất, ba vai trò kiểm tra server, admin tạo/khóa user, tài khoản khóa bị chặn request tiếp theo; không có mật khẩu mặc định.
- PASS kỹ thuật 9b/9c: CRUD có auth, revision cũ/unknown fields, tham chiếu, tổng server, luồng đơn–giao đầy đủ kể cả thất bại/giao lại; durable outbox/retry/xung đột có unit và tích hợp thật. Không tuyên bố ACID xuyên document.
- PASS 9d: Mango UI tìm/lọc/bookmark, Shipping theo nhân viên; tiếng Việt/CSRF/logout qua IIS. Build/Razor PASS, 49 smoke + 19 unit PASS, Test-Workflow.py exit 0 trên DB riêng.
- NOT RUN: ảnh GUI, deployment HTTPS thực tế. Admin DB chính phải được sinh viên tạo bằng mật khẩu riêng; source có tool/hướng dẫn, không phát hành mật khẩu mặc định. Lịch sử Git/single-node vẫn chưa xác minh.

### P5 – nghiệm thu cuối
- PASS mục 6/9b ở phạm vi snapshot logic: export 104 doc + 9 JSON index, restore DB riêng, hash/count/định nghĩa index khớp; nhập lại skip 104 không đổi revision; xung đột ID/index và file không hợp lệ bị từ chối trước ghi. Không export app_user/quyền/config; không gọi đây là backup toàn hệ thống.
- PASS bộ Run-AllTests.ps1 exit 0: build/Razor, 49 smoke, 21 unit, stub initializer/diagnostic, P2/Mango thật, backup/restore, IIS auth/CRUD/workflow/outbox/Dashboard/Shipping/filters/CSRF, ba lỗi backend thật. Giữ log lần đầu FAIL và cách sửa trong CODEX_PROGRESS.md.
- Đủ README + DATABASE_DESIGN/FAUXTON_DEMO/TEST_RESULTS/USER_GUIDE/DEPLOYMENT/SCREENSHOT_CHECKLIST/BACKUP_RESTORE; NOT RUN ảnh GUI, demo máy thành viên khác, HTTPS publish và backup toàn máy chủ. BLOCKED xác minh single-node/quyền admin và xử lý credential/lịch sử/Private. Không tự đánh dấu hoàn thành toàn bộ rubric/P6.
