# Kiểm thử P0/P1 trên Windows

## Đã tự động hóa

Sau Restore NuGet, chạy trong Developer PowerShell của Visual Studio 2022:

```powershell
msbuild QuanLyBanHang.sln /t:Build /p:Configuration=Debug /p:MvcBuildViews=true /verbosity:minimal /nologo
powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-SmokeTests.ps1
python tests/Test-Initializer.py
```

`ExecutionPolicy Bypass` chỉ áp dụng cho tiến trình kiểm thử này, không thay đổi policy của máy.
SmokeTests dùng HTTP handler giả lập, không đăng nhập CouchDB thật. Test initializer dùng HTTP server giả lập ở cổng loopback ngẫu nhiên, chỉ chạy script với các giá trị kiểm thử giả.

Khi ứng dụng đang chạy IIS Express tại `http://localhost:52515` và **chưa cấu hình CouchDB**, chạy:

```powershell
python tests/Test-WebSmoke.py
```

Test web xác minh cảnh báo kết nối, View tạo đơn, validation bằng dữ liệu không hợp lệ và chặn POST thiếu token. Không dùng test này để kết luận CRUD thành công. HTTP 500 khi thiếu token là cách MVC5 mặc định trả lỗi anti-forgery; request bị chặn trước khi ghi dữ liệu.

## Kiểm thử thủ công với CouchDB thật – chưa chạy trong phiên này

1. Đổi mật khẩu cũ, dùng tài khoản mới; thiết lập bốn biến môi trường theo README. Không gửi mật khẩu qua chat/commit/log. Khởi động lại VS/IIS Express để nhận biến môi trường.
2. Hoàn tất Setup single node trong Fauxton. Dùng database thử riêng (ví dụ `qlbanhang_demo_p01`) và tài khoản có quyền tạo database/index cho bước khởi tạo. Trong PowerShell đã có biến môi trường, chạy `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-CouchDb.ps1` hai lần. Xác minh có bốn document `demo-*`, index `type-id`; sửa tên sản phẩm mẫu rồi chạy lại để xác nhận không ghi đè. Không xóa database thật.
3. Ứng dụng dùng tài khoản được cấp quyền database. Mở Home, Customer, Product, Order, Delivery, Shipping; kiểm tra số lượng từ dữ liệu mẫu và trang Order/Create. Các query dùng `_find`, selector type/status, sort type + _id, bookmark 200 bản ghi/trang; script tạo index type + _id. Chưa đo hiệu năng và chưa đủ bộ query P3.
4. Tạo/sửa/xóa một sản phẩm và khách hàng giả; ghi nhận ID/revision. Kiểm tra giá/số lượng âm, trường bắt buộc, email sai, mã trùng và mã chứa `/`. Không xóa dữ liệu ngoài bộ demo của mình.
5. Tạo đơn tham chiếu khách/sản phẩm thật trong database thử; để trống dòng sản phẩm thứ hai phải được bỏ qua. Kiểm tra số lượng > 0, giá >= 0, tổng tính trên server, ngày `yyyy-MM-dd`, trạng thái hợp lệ. Sửa và xóa đơn vừa tạo sau khi xử lý giao hàng mẫu liên quan.
6. Tạo/sửa/xóa giao hàng giả tham chiếu đơn có thật. Mã đơn không tồn tại hoặc trỏ vào tài liệu khác loại phải bị từ chối. Quy tắc chuyển trạng thái và bảo toàn tham chiếu khi xóa sẽ hoàn thiện ở P4.
7. Mở hai tab sửa cùng một tài liệu: lưu tab thứ nhất, lưu tab thứ hai phải báo xung đột; tab cũ không ghi đè. Kiểm tra xóa bằng revision cũ cũng bị từ chối. Thêm trường JSON phụ bằng Fauxton rồi sửa qua app: trường phụ phải còn.
8. Chọn tên database thử chưa tồn tại rồi khởi động lại app: sáu trang phải hiện cảnh báo có kiểm soát. Dùng mật khẩu thử sai để kiểm tra 401, tài khoản thiếu quyền để kiểm tra 403. Khôi phục cấu hình đúng sau mỗi ca; không ghi mật khẩu vào bằng chứng.
9. Tạm dừng CouchDB để kiểm tra lỗi kết nối; thử môi trường chậm để kiểm tra timeout 15 giây. Không dùng máy chủ đang phục vụ người khác cho thao tác này.
10. Lưu kết quả thật, phiên bản môi trường và ảnh Fauxton đã che thông tin nhạy cảm vào nhật ký trước khi nghiệm thu P0/P1.

Còn ngoài phạm vi P0/P1: login/role ứng dụng, quy tắc chuyển trạng thái đầy đủ, ràng buộc tham chiếu khi xóa, bộ seed 10/20/20/15, import/export, backup/restore, demo Fauxton và báo cáo rubric. Không mở triển khai dùng chung trước khi hoàn thiện phân quyền P4.
