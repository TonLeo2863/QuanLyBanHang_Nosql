# Import, export và phục hồi có kiểm chứng

`Business-Backup.py` tạo **snapshot logic dữ liệu nghiệp vụ**, không phải backup đầy đủ CouchDB. Phạm vi: bốn loại customer/product/order/delivery, các trường nghiệp vụ/mở rộng không nhạy cảm, lịch sử trạng thái và JSON Mango indexes. Loại bỏ `_rev`, syncToken và pendingDelivery=null; nếu còn pending thật thì dừng, yêu cầu đồng bộ xong. Revision mới được CouchDB cấp khi restore.

Không xuất app_user (salt/hash/mật khẩu), `_users`, `_security`, cấu hình máy chủ, log, attachments, lịch sử revision/xóa, design views tùy biến ngoài JSON index. Vì thế cần tạo lại tài khoản website bằng bí mật riêng sau phục hồi, cấp quyền DB bằng tài khoản quản trị và cài lại các thành phần máy chủ theo hồ sơ vận hành. File dữ liệu không thay bản sao lưu cấu hình/quyền hệ thống. Không dùng quy trình này để cam kết phục hồi toàn hệ thống.

## Thao tác

Đặt biến môi trường trong terminal riêng theo DEPLOYMENT.md. Không đưa mật khẩu vào tham số command line. Nếu terminal cũ chưa nhận biến, mở terminal mới; wrapper chỉ hỗ trợ truyền ScriptArguments như một mảng khi được gọi trong PowerShell.

```powershell
python scripts/Business-Backup.py export artifacts/backups/demo.local.json
python scripts/Business-Backup.py import artifacts/backups/demo.local.json --target qlbanhang_restore_buoidemo
python scripts/Business-Backup.py verify artifacts/backups/demo.local.json --target qlbanhang_restore_buoidemo
```

Có thể thay prefix bằng `qlbanhang_test_`. Tool **từ chối import vào qlbanhang hoặc tên ngoài hai prefix thử nghiệm**, không có --force. Không xóa hoặc ghi đè DB/tài liệu. Tên file export phải chưa tồn tại; chọn file mới cho mỗi lần. `artifacts/` bị gitignore; không commit snapshot, kể cả khi hiện chỉ là dữ liệu giả.

Export đọc Mango theo type/bookmark và so sánh update_seq trước/sau. Nếu DB thay đổi trong lúc đọc, dừng không tạo snapshot; cần ngừng ghi ở mức vận hành rồi thử lại. Đây không phải giao dịch snapshot của DB. Codec Decimal tránh thêm sai số float ở công cụ; không thể phục hồi độ chính xác đã bị mất từ trước trong nguồn CouchDB. Quy tắc tiền tệ hiện tại giới hạn tối đa 999999999999.99 và 2 số lẻ, đồng nhất với ứng dụng.

Import kiểm tra toàn tệp trước khi ghi: định dạng `qlbh-business-v1`, tối đa 10 MiB/10000 doc, JSON không khóa trùng/NaN, ID/type, trạng thái/ngày/số lượng/giá/tổng, tham chiếu nội bộ, liên kết canonical Order/Delivery và danh sách JSON index. Tệp phải là snapshot đầy đủ, không dùng mảng JSON tùy ý hay file chỉ chứa một phần tham chiếu. Các tên trường nhạy cảm và giá trị trùng mật khẩu cấu hình bị từ chối; người vận hành vẫn phải kiểm soát nội dung dữ liệu nguồn, công cụ không phải bộ phát hiện mọi bí mật tùy ý.

Format của repo yêu cầu tên JSON index chữ thường/số/gạch ngang và design doc `idx-<name>`. Index tùy biến không phù hợp sẽ làm validation từ chối, cần quy trình riêng; không xóa index nguồn để làm test PASS. Cùng tên index nhưng khác định nghĩa ở đích cũng bị chặn trước khi ghi. Số đếm 104 là bốn loại nghiệp vụ; design document/index và app_user không nằm trong số đếm đó.

Checksum SHA256 canonical JSON phát hiện thay đổi nội dung, không phải chữ ký xác thực. Chỉ nhập tệp đáng tin. ID đã có và nội dung tương đương được skip, giữ revision; ID khác nội dung dừng trước khi ghi. Nếu có ghi đồng thời gây 409 giữa chừng, các tài liệu vừa thêm được giữ lại, không rollback bằng xóa; đối chiếu rồi chạy lại an toàn. Không coi nhiều PUT là transaction.

Verify đối chiếu số lượng bốn loại, hash nội dung chuẩn hóa và sự có mặt index; không chỉ kiểm tra HTTP 200. So sánh bỏ revision do DB đích cấp lại và metadata đồng bộ không mang ý nghĩa nghiệp vụ. Khôi phục thành công phải đọc được dữ liệu/truy vấn ở DB đích. DB thử được giữ nguyên sau test để sinh viên xem trong Fauxton.

## Phân biệt các cách sao lưu

| Cách | Có gì | Hạn chế |
|---|---|---|
| Snapshot logic của repo | nghiệp vụ + JSON index, kiểm tra tham chiếu/hash | loại trừ người dùng/quyền/config/revision history/attachments; không phải backup toàn máy chủ |
| Replication CouchDB | sao chép document/revision theo cơ chế replication | thay đổi/xóa có thể truyền tiếp; bản sao đang đồng bộ không tự cung cấp mốc phục hồi an toàn trước lỗi người dùng |
| Backup hệ thống | file DB/index, cấu hình và log theo quy trình quản trị | cần quyền máy chủ, kiểm thử phục hồi riêng và bảo vệ các file có credential; chưa thực hiện ở đây |

Nguồn chính thức: [CouchDB backup](https://docs.couchdb.org/en/stable/maintenance/backups.html), [Replication](https://docs.couchdb.org/en/stable/replication/intro.html). Không gọi tải JSON trên Fauxton là backup đầy đủ. Kết quả thật và DB đích xem TEST_RESULTS.md.
