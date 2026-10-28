# Hướng dẫn sử dụng ứng dụng

Lần đầu, người vận hành chạy `Create-AppUser.py` trong terminal đã nạp biến CouchDB (hoặc qua With-CouchEnvironment.ps1), tự nhập tên và mật khẩu 12–128 ký tự để tạo Administrator. Không có mật khẩu mặc định. Không đăng nhập website bằng tài khoản CouchDB. Mật khẩu chỉ nhập riêng trên máy, không đưa vào lệnh/Git/chat.

1. Vào `/Account/Login`, đăng nhập. Administrator vào `/Account/Users` tạo OrderManager/Shipper; danh sách không hiển thị salt/hash. Có thể khóa người dùng khác; tài khoản khóa bị từ chối ở request tiếp theo. Không tự khóa tài khoản đang dùng.
2. OrderManager hoặc Administrator tạo khách và sản phẩm; giá/số lượng không âm. Khi có đơn tham chiếu, không xóa khách/sản phẩm. Quantity sản phẩm hiện là số liệu quản lý, chưa phải cơ chế giữ/chốt tồn kho tự động.
3. Tạo Order với trạng thái `processing`; nhập customerId và productId tồn tại, quantity > 0, price >= 0, ngày ISO. Tổng do server tính; dòng sản phẩm trống hoàn toàn bị loại. Giá trên item là giá giao dịch được người có quyền nhập, không bị thay theo giá sản phẩm sau này.
4. Sửa Order: `processing → confirmed → preparing`; có thể hủy trước giao theo luật. Lịch sử trạng thái ở trang sửa. Không nhảy trực tiếp tới shipping/delivered. Khi đã có phiếu giao, giữ nguyên nội dung đơn và cập nhật qua phiếu.
5. Khi Order `preparing`, tạo Delivery `pending`. Mỗi đơn có một phiếu trong quy trình mới. Nhập `shipperId` bằng **username** của tài khoản Shipper đang hoạt động; chuyển phiếu `assigned` để phân công.
6. Shipper chỉ xem/sửa phiếu được giao. Chuyển `assigned → shipping → delivered` hoặc `failed`. Thay đổi shipperId/địa chỉ trên POST của shipper không thay đổi dữ liệu gốc. Manager có thể phân công lại từ failed/cancelled sang assigned; sau đó giao lại.
7. Order đồng bộ shipping/delivered; failed/cancelled trả Order về preparing để xử lý lại. Phiếu pending mới được xóa; sau khi đã giao, giữ lịch sử. Không xóa Order có phiếu giao. Trang Shipping chỉ hiển thị trạng thái shipping (và lọc theo người dùng với Shipper).
8. Các danh sách Customer/Product/Order/Delivery có bộ lọc và bookmark (20 bản ghi/trang). Từ khóa tìm tên với khách/sản phẩm, tìm ID với đơn/giao. Khoảng tiền dùng cho Product/Order; khoảng ngày/mã khách dùng cho Order; danh mục dùng Product. Đổi điều kiện lọc phải bắt đầu từ trang đầu. Trang Shipping có 100 bản ghi/trang.
9. Hai người sửa cùng tài liệu: request revision cũ bị từ chối, tải lại trước khi nhập lại; không tự retry ghi đè. Chưa có transaction xuyên document. Khi lỗi ghi phiếu, Order có `pendingDelivery`, bị khóa chỉnh sửa; Administrator vào `/Sync` bấm thử lại. Order là nguồn trạng thái chính khi đang chờ; Shipping/Dashboard có thể tạm lệch cho đến khi đồng bộ xong. Xung đột bên ngoài được giữ nguyên và cần đối chiếu, không xóa intent để che lỗi.
10. Đăng xuất bằng nút POST có token. Cookie hết hạn sau 30 phút, không nhớ đăng nhập dài hạn. Môi trường dùng chung phải HTTPS.

Document cũ không có `deliveryId`: khi sửa phiếu, ứng dụng chỉ nhận vào quy trình nếu duy nhất một phiếu tham chiếu đơn và trạng thái đang khớp. Phiếu assigned của đơn confirmed cần chuẩn bị đơn trước. Không tự chuyển hàng loạt dữ liệu cũ. Trường hợp nhiều phiếu hoặc trạng thái lệch cần người vận hành đối chiếu riêng; không sửa JSON tùy tiện trên DB chính.

Quyền kiểm tra phía server ở cả URL và POST. Ẩn/hiện menu không phải ranh giới phân quyền. Người có quyền truy cập trực tiếp CouchDB/Fauxton có thể vượt quy tắc ứng dụng; chỉ cấp quyền đó cho người vận hành được tin cậy. Ràng buộc tham chiếu được kiểm tra trước khi xóa; không coi đây là foreign key có tính serializable với thao tác đồng thời hoặc ghi ngoài ứng dụng.

P5 bổ sung miền tiền: giá mỗi item/sản phẩm và tổng đơn từ 0 đến 999999999999.99, tối đa 2 số thập phân, nhập dấu `.`. Giá trị vượt miền bị từ chối trước khi ghi, không âm thầm làm tròn để vừa CouchDB. Import/export thực hiện bằng CLI và DB thử riêng theo BACKUP_RESTORE.md; không tải snapshot có tài khoản/mật khẩu lên website.
