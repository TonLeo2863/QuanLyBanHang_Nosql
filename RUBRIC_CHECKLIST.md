# Checklist thang điểm chính thức – Cơ sở dữ liệu NoSQL 15DH

**Nguồn đối chiếu:** PDF `DANH SACH DE TAI CUOI KY VA THANG DIEM_NOSQL (15DH) (Full).pdf`, trang 3–4, đề tài gợi ý số 2 trang 1. Đây là checklist công việc, không phải điểm tự chấm.

| STT | Tiêu chí | Điểm | Bằng chứng cần chuẩn bị | Đã kiểm chứng? |
|---|---|---:|---|---|
| 1 | Lý thuyết và cài đặt Fauxton | 0,5 | Hướng dẫn + ảnh màn hình | Chưa |
| 2 | Chức năng của Fauxton | 0,5 | Demo Database, Documents, Query, Index | Chưa |
| 3 | So sánh Fauxton với GUI Tool khác | 0,5 | Bảng ưu/nhược, khác biệt phạm vi CSDL | Chưa |
| 4 | Điểm mạnh và loại ứng dụng phù hợp | 0,5 | Lập luận liên quan nghiệp vụ đơn/giao hàng | Chưa |
| 5a | Tạo database | 0,5 | DB trong Fauxton + script tái lập | Chưa |
| 5b | Insert dữ liệu đầy đủ | 0,5 | JSON seed + ảnh document | Chưa |
| 5c | Dữ liệu đáp ứng truy vấn cơ bản | 0,5 | Query + kết quả minh chứng | Chưa |
| 5d | Dữ liệu đáp ứng truy vấn nâng cao | 0,5 | Mango selector/index + kết quả | Chưa |
| 6 | Import/Export; Backup/Restore | 0,5 | Hướng dẫn, tệp demo + kết quả khôi phục | Chưa |
| 7a | Truy vấn cơ bản trên GUI Tool | 0,5 | Kịch bản thực thi Fauxton | Chưa |
| 7b | Truy vấn nâng cao | 0,5 | Kịch bản thực thi Fauxton | Chưa |
| 8 | Kết nối CSDL với phần mềm | 0,5 | Code + demo HTTP CouchDB | Có code, chưa test thực tế |
| 9a | Quản trị người dùng | 0,5 | Đăng nhập/phân quyền và kiểm thử | Chưa |
| 9b | Thao tác CSDL (CRUD, backup...) | 0,5 | CRUD, backup có kiểm thử | Một phần code |
| 9c | Nghiệp vụ mô tả bài toán | 0,5 | Demo đơn hàng/giao hàng từ đầu đến cuối | Một phần code |
| 9d | Chức năng hỗ trợ khác | 0,5 | Thống kê, tìm kiếm, lọc, lịch sử... | Một phần code |
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
