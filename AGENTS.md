# Quy tắc bắt buộc dành cho Codex – Đồ án CSDL NoSQL nhóm 7

## Bối cảnh
- Repo: `TonLeo2863/QuanLyBanHang_Nosql`; nhánh gốc hiện tại `master` (không tự đổi thành `main`).
- Đề tài số 2: Tìm hiểu Fauxton quản trị Apache CouchDB cho hệ thống quản lý đơn hàng và giao hàng của cửa hàng bán lẻ trực tuyến; xây dựng ứng dụng minh họa.
- Công nghệ phải giữ: ASP.NET MVC 5, .NET Framework 4.7.2, C#, Newtonsoft.Json, CouchDB/Fauxton. Không tự ý chuyển sang ASP.NET Core, SQL Server, MongoDB, EF hoặc Docker nếu không có lý do được phê duyệt.
- Tài liệu chuẩn: `ROADMAP_NOSQL.md`, `RUBRIC_CHECKLIST.md`, `CODEX_PROGRESS.md`, tài liệu chính thức của giảng viên do chủ dự án cung cấp.
- Đích đến: chức năng chạy được trên Windows + Visual Studio 2022 + CouchDB 3.x, minh chứng từng mục rubric; không chỉ làm đẹp giao diện.

## An toàn bắt buộc
1. Repo từng có thông tin đăng nhập CouchDB ở `QuanLyBanHang/Services/CouchDbService.cs`; coi thông tin cũ là đã lộ. YÊU CẦU chủ repo đổi mật khẩu, chuyển repository sang Private và xem xét làm sạch lịch sử trước khi tiếp tục chia sẻ. Không in, sao chép, bình luận, commit hoặc tái sử dụng mật khẩu cũ. Không tự đổi credential trên hệ thống bên ngoài mà chưa có quyền.
2. Chuyển URL, tên DB, tài khoản/mật khẩu sang biến môi trường hoặc cấu hình riêng được gitignore; thêm tài liệu cấu hình mẫu KHÔNG chứa bí mật. Không ghi bí mật vào README, log, PR hoặc ảnh chụp màn hình.
3. Mọi POST thay đổi dữ liệu phải có validation, anti-forgery; không đặt credentials của admin CouchDB trong mã JavaScript/HTML trình duyệt. Có thông báo lỗi có kiểm soát; không trả nguyên thông tin nhạy cảm về View.
4. Không force push, không xóa dữ liệu thật, không thay đổi lịch sử Git, không merge vào `master` khi chưa có sự chấp thuận cụ thể. Chỉ dùng dữ liệu mẫu giả.

## Quy trình triển khai
1. Trước khi sửa: đọc `ROADMAP_NOSQL.md` và `CODEX_PROGRESS.md`, kiểm tra `git status` và code hiện tại. Bắt đầu từ phase P0; không làm đè các file đang được tác vụ khác sửa.
2. Làm trên nhánh tính năng `codex/<ten-cong-viec>` hoặc `codex/nosql-completion`; đích PR là `master`. Không push trực tiếp `master`.
3. Mỗi lần chỉ hoàn thiện một nhóm công việc có tiêu chí nghiệm thu cụ thể, kèm kiểm thử/ minh chứng. Tránh viết lại toàn bộ ứng dụng.
4. Trước mỗi commit/push, kiểm tra diff và secret scan; tuyệt đối không commit `bin/`, `obj/`, `.vs/`, database thực, token, tài khoản, mật khẩu. Không giả vờ build/test nếu không có môi trường Windows/.NET Framework/CouchDB.
5. Sau mỗi bước, cập nhật `CODEX_PROGRESS.md`: đã sửa gì, file nào, lỗi nào, build/test thực tế, rubric tương ứng, bước tiếp theo và các điểm chưa kiểm chứng.

## BẮT BUỘC: ghi chú Git bằng tiếng Việt
- **Tất cả tiêu đề commit phải viết bằng tiếng Việt có dấu**, ví dụ `Sửa lỗi trang chủ khi CouchDB chưa có cơ sở dữ liệu`.
- Nội dung commit body (nếu có) cũng phải là tiếng Việt: nguyên nhân, thay đổi, cách kiểm thử.
- **Trước mỗi lần push**: nêu cho chủ dự án bản tóm tắt bằng tiếng Việt: nội dung định push, file thay đổi, kết quả kiểm thử, rủi ro.
- **Sau mỗi lần push lên nhánh đã có Pull Request**: cập nhật comment trong PR bằng tiếng Việt theo mẫu:
  - `### Cập nhật [ngày giờ]`
  - `**Đã thay đổi:** ...`
  - `**File chính:** ...`
  - `**Đã kiểm thử:** ...` (ghi rõ `chưa kiểm thử` nếu chưa chạy)
  - `**Mục rubric:** ...`
  - `**Còn tồn tại:** ...`
  - `**Commit:** <SHA ngắn>`
- Pull Request: **tiêu đề, mô tả và bình luận đều bằng tiếng Việt**. Nếu Codex không có quyền ghi GitHub, tạo sẵn đoạn comment để người dùng dán; đừng báo là đã comment khi chưa thành công.
- Workflow `.github/workflows/ghi-chu-pr-tieng-viet.yml` bổ sung ghi chú tự động sau mỗi lần cập nhật PR trên nhánh `codex/` (khi GitHub Actions được cho phép). Workflow chỉ ghi tiêu đề commit và liên kết, KHÔNG thay thế báo cáo thủ công của Codex.

## Yêu cầu chất lượng
- Dùng `HttpResponseMessage.IsSuccessStatusCode`/status code; xử lý 401/403/404/409/500 và database chưa tồn tại; tránh parse lỗi CouchDB như `rows`.
- Không gọi `_all_docs` để lọc toàn bộ bằng C# đối với báo cáo/truy vấn chính: thêm Mango `_find`, selector, sort, pagination/index phù hợp và giải thích trong báo cáo.
- Giữ `_id`, `_rev` đúng quy tắc CouchDB; xử lý xung đột cập nhật 409; bảo toàn dữ liệu hợp lệ.
- Tạo script idempotent khởi tạo DB, design docs/index và dữ liệu mẫu; chạy lại không làm mất dữ liệu thật.
- Đơn hàng phải kiểm tra khách hàng/sản phẩm, quantity/price, tổng tiền, trạng thái; giao hàng tham chiếu đơn thật và có quy tắc chuyển trạng thái.
- Bao gồm import/export/backup/restore, quyền người dùng ứng dụng, truy vấn nâng cao, demo Fauxton và bài test rõ ràng.
- Không tuyên bố đạt đủ rubric nếu chưa có bằng chứng và demo thực tế.

## Tiếp tục sau P5
- Đọc trạng thái mới nhất trong CODEX_PROGRESS.md và docs/RELEASE_READINESS.md trước khi chọn việc. Không khởi động lại hoặc viết lại P0–P5 đã PASS nếu không có lỗi/hạng mục cụ thể.
- Kiểm thử kỹ thuật PASS không đồng nghĩa an toàn để push: phải kiểm tra cả lịch sử và việc thu hồi credential đã lộ. Chỉ đối chiếu kín, không thử xác thực bằng credential lấy từ commit cũ.
- Giữ nguyên các commit P0–P5; xử lý lịch sử, thay đổi visibility và credential bên ngoài cần chủ repo thực hiện/phê duyệt riêng. Không tự push khi visibility chưa xác minh hoặc còn credential có nguy cơ hiệu lực bị lộ.
