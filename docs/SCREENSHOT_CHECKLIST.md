# Danh sách ảnh minh chứng thật

Tất cả ảnh hiện **NOT RUN / chưa chụp**. Không dùng ảnh tạo bằng AI hoặc HTTP test để giả ảnh GUI. Chỉ chụp dữ liệu giả; loại bỏ bí mật, hộp đăng nhập, cookie, header, hash người dùng và cấu hình nhạy cảm trước khi lưu.

| Tên ảnh đề nghị | Nội dung/chú thích | Rubric |
|---|---|---|
| 01-fauxton-databases.png | Fauxton hiển thị qlbanhang | 1, 2, 5a |
| 02-customer-json.png | JSON customer mẫu, type/id/rev | 5b |
| 03-order-items.png | Order nhúng items và total | 4, 5b |
| 04-document-created.png | Document thử do chính buổi demo tạo | 2 |
| 05-document-edited.png | Sửa doc thử, revision mới | 2 |
| 06-document-deleted.png | Xác nhận đúng ID thử đã xóa | 2 |
| 07-mango-indexes.png | Tên và trường index | 5c, 5d |
| B1..B5-query.png | Selector + kết quả từng query cơ bản | 7a |
| A1..A5-query.png | Selector + kết quả từng query nâng cao | 7b |
| 08-dashboard.png | Số đếm đã đối chiếu DB | 9d |
| 09-workflow.png | Đơn và giao hàng hoàn tất, lịch sử | 9c |
| 10-role-denied.png | Tài khoản không đủ quyền bị từ chối | 9a |
| 11-backup-restore.png | Log an toàn đếm/nội dung phục hồi DB test | 6 |

Ghi ngày, phiên bản và người thao tác dưới mỗi ảnh; ảnh query cần giữ selector và ít nhất một ID kết quả. Quyền quản trị/single-node cần sinh viên xác minh riêng trong Fauxton; không chụp credential.
