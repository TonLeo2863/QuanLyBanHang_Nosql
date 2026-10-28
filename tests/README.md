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

## Thiết lập biến môi trường trên Windows, không ghi mật khẩu vào lệnh

1. Nếu chưa thực hiện, đổi mật khẩu CouchDB cũ trong giao diện quản trị bằng thông tin bạn tự quản lý. Không dùng lại thông tin từng có trong Git. Chủ repo cần chuyển repo sang Private và phối hợp xử lý lịch sử trước khi chia sẻ.
2. Nhấn **Win+R**, nhập `rundll32 sysdm.cpl,EditEnvironmentVariables`, chọn **User variables → New**. Không cần sửa Machine variables.
3. Tạo bốn biến dưới đây. Nhập tài khoản/mật khẩu mới trực tiếp trong hộp thoại Windows; không gõ vào lệnh PowerShell, source, Web.config hoặc chat. Không chụp màn hình hộp thoại chứa thông tin xác thực.

| Tên biến | Giá trị cần nhập riêng trên máy |
|---|---|
| COUCHDB_URL | `http://127.0.0.1:5984` |
| COUCHDB_DATABASE | `qlbanhang` để kiểm tra DB mục tiêu; dùng `qlbanhang_demo_p01` nếu muốn tách bộ thử |
| COUCHDB_USERNAME | Tên tài khoản CouchDB mới mà bạn đã xác minh đăng nhập được |
| COUCHDB_PASSWORD | Mật khẩu mới của tài khoản đó; không đưa vào tài liệu |

Biến môi trường là cấu hình riêng của máy, không phải kho lưu trữ được mã hóa. Giữ quyền truy cập tài khoản Windows phù hợp; không chạy `Get-ChildItem Env:COUCHDB*`, `echo` biến hay bật transcript/debug khi xử lý thông tin xác thực.

4. Đóng VS/IIS Express và mở lại từ Windows để nhận biến mới. Nếu terminal/Codex đang chạy từ trước, khởi động lại ứng dụng mở terminal/Codex để tiến trình nhận biến mới. `.env.example` chỉ là mẫu; ứng dụng không tự nạp `.env`.
5. Mở PowerShell mới trong repo, chạy chẩn đoán chỉ đọc:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-CouchDbConnection.ps1
```

Script chỉ in tên biến, cờ configured và HTTP status; không in giá trị hoặc raw error/response. Exit 0 = đọc được DB/index đúng; exit 1 = kiểm tra xác thực/cấu hình/index thất bại; exit 2 = chưa có đủ biến trong Process, không chạy kiểm tra có xác thực. HTTP 200 của health không chứng minh quyền tạo DB; HTTP 404 chưa xác thực không thay cho kiểm tra bằng tài khoản mới.

6. Nếu `qlbanhang` chưa tồn tại, dùng tài khoản được cấp quyền tạo DB trong Fauxton hoặc chạy script sau trong phiên có đủ biến. Nếu tài khoản ứng dụng không có quyền tạo DB/index, bạn cần thực hiện bước này bằng tài khoản quản trị mới của mình. Không suy đoán thông tin đăng nhập và không tự mở quyền anonymous:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-CouchDb.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-CouchDb.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-CouchDbConnection.ps1
```

Script dùng tên DB trong biến môi trường, tạo index `type-id` và bốn tài liệu giả `demo-*`; không xóa DB hoặc tài liệu và không ghi đè ID có sẵn. Nếu dùng Fauxton để tạo DB thủ công, vẫn cần chạy script để tạo index và dữ liệu mẫu. Xác minh Documents/Indexes sau khi chạy; không coi kết quả HTTP giả lập là bằng chứng DB thật.

7. Khôi phục tài khoản ứng dụng đã được cấp quyền database nếu đã dùng tài khoản quản trị cho bước khởi tạo; mở lại VS/IIS Express, chạy checklist CRUD thật ở trên. Shipping là trang đọc/lọc Delivery có status `shipping`, không có CRUD riêng: kiểm thử bằng cách tạo/sửa Delivery mẫu rồi đối chiếu trang Shipping.
8. Khi cấu hình xong, chỉ báo **“đã thiết lập biến môi trường”**; không gửi giá trị tài khoản/mật khẩu. Nếu bị 401/403, kiểm tra đăng nhập và quyền trong Fauxton trước; không gửi raw response hay Authorization header.

## Kết quả kiểm tra gần nhất (28/10/2026)

- **PASS:** build gồm Razor; 44 assertion giả lập; script initializer trên HTTP stub; kiểm tra HTML/form qua IIS Express; health CouchDB thật.
- **FAIL điều kiện sẵn sàng:** bốn biến môi trường chưa cấu hình ở cả ba scope; `/qlbanhang` trả HTTP 404 khi chưa xác thực.
- **NOT RUN tích hợp thật:** tạo DB/index/seed, Mango, CRUD Customer/Product/Order/Delivery, đối chiếu Shipping, quyền và conflict thật. Không có thông tin xác thực mới, không suy đoán hoặc thử lại mật khẩu cũ.

Có thể kiểm tra lại chẩn đoán mà không ghi DB bằng lệnh ở trên. Kiểm thử chẩn đoán với HTTP stub: `python tests/Test-ConnectionDiagnostic.py`; đây là giả lập, không thay thế kiểm tra có xác thực với CouchDB thật. Chưa bắt đầu P2.
