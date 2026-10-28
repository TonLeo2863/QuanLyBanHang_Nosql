# Lộ trình hoàn thiện đồ án cuối kỳ Cơ sở dữ liệu NoSQL – Nhóm 7

**Repository:** `TonLeo2863/QuanLyBanHang_Nosql` · **Nhánh gốc:** `master` · **Ứng dụng:** ASP.NET MVC5 / .NET Framework 4.7.2 / CouchDB-Fauxton.

**Đề tài chính thức:** Tìm hiểu công cụ Fauxton để quản trị CSDL tài liệu CouchDB cho hệ thống “Quản lý đơn hàng và giao hàng của cửa hàng bán lẻ trực tuyến”. Xây dựng ứng dụng minh họa.

## Trạng thái khi bàn giao sau P5

P0–P5 đã có commit kỹ thuật trên `codex/nosql-completion`; xem CODEX_PROGRESS.md và docs/TEST_RESULTS.md để phân biệt PASS/BLOCKED/NOT RUN. Các mục dưới đây là lộ trình và baseline lịch sử, không phải yêu cầu làm lại mã đã đạt. Dữ liệu P2 theo yêu cầu cập nhật là ít nhất 20 khách/30 sản phẩm/30 đơn/20 giao hàng; bộ hiện có đã kiểm thử đạt.

**Chưa đủ điều kiện phát hành:** audit sau P5 xác nhận mật khẩu CouchDB hiện cấu hình trùng literal từng được commit trong lịch sử. Không in hoặc thử lại credential cũ. Chủ repo phải thay/thu hồi credential và xác minh cấu hình mới; repository Public/Private chưa xác minh được do GitHub trả 404. Không tự push, rewrite lịch sử hoặc merge master. Giữ nguyên commit P0–P5; kế hoạch xử lý lịch sử phải được chủ repo phê duyệt riêng. Xem docs/RELEASE_READINESS.md.

## Kết quả kiểm tra mã nguồn trên `master` – 28/10/2026
- Có `Controllers/{Home,Customer,Product,Order,Delivery,Shipping}Controller.cs`; `Models/{Customer,Product,Order,OrderItem,Delivery}.cs`; `Services/CouchDbService.cs` và các Razor Views.
- `CouchDbService` cấu hình cố định `http://127.0.0.1:5984`, DB `qlbanhang`, có thông tin xác thực hardcode (cần thu hồi/thay đổi; KHÔNG ghi lại trong tài liệu).
- Home/Customer/Product/Order/Delivery/Shipping dùng `_all_docs?include_docs=true`, rồi parse dynamic `result.rows`; thiếu kiểm tra mã lỗi trả về. Người dùng báo gặp NullReferenceException tại `HomeController.cs:32` khi DB chưa tồn tại.
- `Models/Order.cs` có `orderDate`, trong khi `Views/Order/Create.cshtml` tham chiếu `m.date` (sai tên property; có thể chỉ phát hiện khi biên dịch Razor lúc truy cập View).
- Có các trang CRUD cơ bản và trang liệt kê đơn đang giao; thiếu nền tảng login/role, truy vấn Mango, import/export, backup/restore, tài liệu/dữ liệu mẫu và test trong repo.
- `FilterConfig` chỉ dùng `HandleError`; các POST CRUD chưa thấy anti-forgery; nên kiểm tra và bổ sung đồng bộ.
- `packages.config` dùng Bootstrap 5.2.3 nhưng `_Layout.cshtml` còn một số markup navbar theo Bootstrap 3; cần kiểm tra tương thích UI ở giai đoạn cuối.
- Repo lúc khảo sát là `public`, mặc định `master` và có 2 commit; source đọc được, nhưng chưa build/test trên Windows trong phiên rà soát này.

## P0 – Bảo mật + môi trường (ưu tiên số 1)
**Việc làm:** đổi mật khẩu CouchDB cũ (chủ repo thao tác), Private repo, xử lý bí mật đã lên lịch sử Git có phối hợp; chuyển credentials sang biến môi trường hoặc tệp cấu hình ignored, thêm `.example`, hướng dẫn chạy; khởi tạo DB `qlbanhang` và kiểm tra quyền admin/cấu hình single-node CouchDB.
**Nghiệm thu:** không còn bí mật trong file tracked mới; không in credential; README hướng dẫn cấu hình trên máy khác; request đến CouchDB có thông báo rõ nếu DB thiếu hoặc 401/403.
**Minh chứng:** kiểm tra git diff/secret scan, ảnh Fauxton danh sách database, log đã che bí mật.

## P1 – Sửa lỗi cốt lõi và làm ổn định CRUD
**Việc làm:** sửa `Views/Order/Create.cshtml` (`date` → `orderDate`); xử lý `result.rows` null, HTTP 404/401/409, request timeout, URL encoding; dùng response typed thay `dynamic` ở điểm chính; xử lý `_rev` cập nhật/xóa; validate server-side, anti-forgery, lỗi trùng ID; đồng nhất tên trường.
**Nghiệm thu:** trang chủ và các danh sách không còn lỗi 500 khi DB rỗng; cảnh báo hữu ích khi thiếu DB/đăng nhập sai; tạo/sửa/xóa 4 loại document và tải View đơn hàng thành công.
**Kiểm thử:** mở 6 trang; CRUD sản phẩm/khách hàng/đơn/giao hàng; test thiếu DB, sai mật khẩu, xung đột `_rev`.

## P2 – Thiết kế CSDL + dữ liệu mẫu có thể tái lập
**Việc làm:** ghi sơ đồ document, giải thích document database/embedded order items/references; script tạo DB và tạo dữ liệu giả: ít nhất 10 khách hàng, 20 sản phẩm, 20 đơn và 15 giao hàng, đủ trạng thái; IDs nhất quán, liên kết hợp lệ; script tạo chỉ mục và khởi tạo lại an toàn.
**Nghiệm thu:** người khác clone repo và khởi tạo dữ liệu demo bằng hướng dẫn, không cần nhập tay; truy vấn dữ liệu cơ bản/nâng cao chạy được; không có đơn tham chiếu khách/sản phẩm không tồn tại.
**Minh chứng:** ảnh Fauxton Database, Documents, Indexes; tài liệu mô hình dữ liệu và mẫu JSON.

## P3 – Mango Query / chỉ mục / demo Fauxton
**Việc làm:** query theo loại document, đơn hàng theo ngày/khách/trạng thái, giao hàng theo shipper/trạng thái, lọc tổng tiền, tổng hợp báo cáo bằng dữ liệu thật; tạo Mango indexes phù hợp; truy vấn phân trang, sort; so sánh query với/không index trong khả năng demo.
**Nghiệm thu:** tối thiểu 5 truy vấn cơ bản + 5 truy vấn nâng cao có câu lệnh JSON selector, đầu ra mong đợi, dữ liệu đáp ứng và ảnh thực thi trong Fauxton. Không quảng cáo các toán tử hoặc số liệu hiệu năng chưa kiểm chứng.

## P4 – Nghiệp vụ đơn hàng/giao hàng + quản trị người dùng
**Việc làm:** chọn khách/sản phẩm thực, quantity > 0, price >= 0, tính tổng chính xác; tạo đơn → giao nhiệm vụ → đang giao → giao thành công/thất bại/hủy; chặn chuyển trạng thái sai; xem lịch sử/trạng thái đơn; login/logout và quyền admin/nhân viên, lưu mật khẩu dưới dạng hash chuẩn, không dùng tài khoản CouchDB admin để xác thực người dùng ứng dụng.
**Nghiệm thu:** demo xuyên suốt: thêm khách + sản phẩm → tạo đơn → phân công shipper → cập nhật giao hàng → báo cáo tình trạng. Người không có quyền không thể sửa/xóa qua URL; có test nghiệp vụ và các trường hợp dữ liệu xấu.
**Ghi chú:** tính năng bổ sung giới hạn trong phạm vi đề tài, không mở rộng thành sàn TMĐT đầy đủ.

## P5 – Import/Export, Backup/Restore và kiểm thử
**Việc làm:** có phương án export/import dữ liệu demo (JSON hoặc công cụ phù hợp), backup/restore có xác nhận thành công, quy trình phục hồi sang DB test riêng; kiểm thử dữ liệu/ID/revision, không xóa database đang sử dụng để minh họa; thêm unit tests cho logic và smoke/integration tests có điều kiện CouchDB sẵn sàng.
**Nghiệm thu:** sau export/backup rồi restore trên DB demo mới, số lượng và nội dung document được đối chiếu; script kiểm thử có chỉ dẫn chạy trên Windows và kết quả thật.

## P6 – Fauxton GUI, báo cáo, slide và bàn giao
**Việc làm:** tài liệu cài đặt Fauxton/CouchDB, mô tả từng chức năng Fauxton sử dụng, so sánh với một GUI khác (VD MongoDB Compass – nêu rõ khác hệ CSDL), lý do phù hợp Document DB; ảnh minh chứng chính chủ; Word 50–60 trang Times New Roman 13, giãn dòng 1.5; PowerPoint và demo script; bảng phân công 3 thành viên.
**Nghiệm thu:** checklist rubric đánh dấu có bằng chứng; báo cáo có mô hình, truy vấn, ảnh kết quả, backup/restore, test, đánh giá giới hạn; demo chạy trên máy nộp; có source + Word + slide.

## Quy ước phát hành
- Mỗi phase có branch/PR riêng hoặc các commit nhỏ trên `codex/nosql-completion` → PR vào `master`.
- Mỗi commit, mô tả PR và comment sau mỗi push phải bằng tiếng Việt; xem `AGENTS.md`.
- Không merge khi phase chưa được kiểm thử trên Windows + CouchDB nếu điều kiện nghiệm thu đòi hỏi chạy thực tế.
- Cập nhật `CODEX_PROGRESS.md` sau mỗi phase, ghi cả những việc chưa thể kiểm chứng.
