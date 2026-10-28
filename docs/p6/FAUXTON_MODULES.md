# Năm chức năng quản trị Fauxton phục vụ báo cáo và demo

Theo đính chính, “ít nhất 3 công cụ” là ít nhất ba chức năng/công cụ **trong Fauxton**, không phải ba phần mềm GUI. Chọn F1 CRUD/revision, F2 Mango/index và F3 replication; bổ sung F4 security và F5 backup/restore. Mỗi card bên dưới phân biệt tính năng GUI, REST/script bổ trợ, kết quả mong đợi và bằng chứng đã có. Không dùng checklist này để tự đánh dấu đã demo.

## Điều kiện chung

Chủ repo phải thu hồi/thay credential đã lộ, cung cấp cấu hình mới riêng trên máy và xác minh quyền; không gửi secret vào Word/URL/log. Chỉ đọc p2-* trên qlbanhang. Mọi ghi dùng ID mới trong DB thử riêng được bảo vệ; không sửa/xóa seed, document có sẵn hoặc `_security` của DB chính. Nếu thiếu quyền tạo DB/index/replication, dừng ca đó và ghi BLOCKED, không tự cấp `_admin`.

## F1 Database Document CRUD và revision

**Khái niệm/mục đích.** Database chứa document; `_id` xác định tài liệu, `_rev` điều phối cập nhật. Mục tiêu là minh họa CRUD và cập nhật cạnh tranh, không phải trình diễn xóa database. [C1/C9]

**Nguyên lý.** Tạo document mới không gửi revision; cập nhật/xóa dùng revision hiện hành. Thay đổi thành công sinh revision mới; request stale bị từ chối thay vì ghi đè. Xóa tạo tombstone, không phải bảo đảm nội dung mọi revision luôn được giữ để khôi phục.

**Thao tác thực tế cần làm.** Vào Databases, chọn qlbanhang, đọc `p2-customer-001` và `p2-order-001`; ghi lại type/id/quan hệ items. Chuyển sang DB thử đã được chủ máy tạo, dùng ID `p6-crud-<mã-buổi-demo>` chưa tồn tại. Tạo customer giả với name, email miền example.invalid và address giả. Mở hai tab cùng document; lưu thay đổi tên ở tab 1, thử lưu tab 2 để quan sát stale revision. Tải lại và đối chiếu. Chỉ xóa document vừa tạo khi không có document khác tham chiếu; giữ log/ảnh trước-sau và GET 404 sau xóa.

**Ví dụ.** JSON đầu vào tạo mới: `{"_id":"p6-crud-CODE","type":"customer","name":"Khách mô phỏng","email":"demo@example.invalid","address":"Địa chỉ giả"}`. CODE phải được thay bằng mã riêng, không dùng nếu ID đã tồn tại. Đây là mẫu, không kết quả thực thi.

**Mong đợi/kết quả.** Mong đợi GET/PUT thành công, revision đổi, stale edit 409 hoặc thông báo conflict GUI, record thử không còn live sau xóa. P5 CRUD/revision qua API/IIS đã PASS; GUI P6 **NOT RUN**, ghi hiện tại **BLOCKED credential**. Không lấy ảnh Word cũ tự động chứng minh ca này.

**Ảnh.** F1-01 danh sách DB; F1-02 document p2 đọc chéo; F1-03 tạo ID mới; F1-04 trước/sau cùng ID và revision; F1-05 conflict; F1-06 xác nhận xóa đúng ID thử. Không chụp hộp mật khẩu.

**Ưu điểm/hạn chế/phản biện.** Editor cho thấy JSON/revision rõ; không tự áp dụng state machine website. Hỏi “Có sửa Order shipping trực tiếp trên Fauxton được không?”: người có quyền DB có thể vượt quy tắc app; demo nghiệp vụ phải đi qua website, không coi quyền DB là role website.

## F2 Mango Query Index sort bookmark và explain

**Khái niệm/mục đích.** Selector mô tả tập kết quả; index hỗ trợ truy cập theo trường/sort. `_explain` kiểm tra kế hoạch thay cho khẳng định query đã tối ưu chỉ vì có use_index. [C3]

**Nguyên lý.** Index có thứ tự field; query/sort cần tương thích. Server vẫn có thể phải lọc phần điều kiện chưa được index giải quyết. Bookmark gắn với selector/sort và không cung cấp snapshot ổn định khi dữ liệu thay đổi.

**Thao tác.** Trong qlbanhang mở Mango Query, dán phần query của B1..B5/A1..A5 trong mango-queries.json. Dùng Manage Indexes xem type-id, type-status-id, type-date, type-price/type-total và field `_id` phân định thứ tự. Chạy query, đối chiếu ID p2-* và expectedSeedCount. Với limit=7, lưu bookmark trang đầu, dùng cho trang sau, giữ điều kiện nguyên vẹn. Gọi POST `/qlbanhang/_explain` bằng REST client/script an toàn với cùng query; nếu UI không có Explain thì nói rõ phần đó thực hiện qua API, không giả nút GUI.

**Ví dụ.** B4 lọc type=order/status=shipping, sort type/status/_id, use_index=idx-type-status-id. A4 tìm item có productId cụ thể và quantity đủ lớn bằng `$elemMatch`. Khác biệt giữa “có chọn index” và “mọi điều kiện dùng index” phải được giải thích.

**Mong đợi/kết quả.** Bộ seed không đổi: B1..B5 có 20/10/2/5/5 kết quả p2-*; A1..A5 có 11/11/4/1/6. Đây là kết quả API P3/P5 đã đo, không phải số live đã đếm lại P6. GUI/đọc có xác thực hiện **BLOCKED**; không đưa số mong đợi vào cột thực tế. Số record khác prefix demo-* có thể làm tổng query lớn hơn expectedSeedCount.

**Ảnh.** F2-01 index fields; B1..B5 và A1..A5 selector+kết quả; F2-02 explain đúng tên index; F2-03 hai trang không lặp ID. Chỉ chọn một số hình đại diện vào chương, các hình còn lại ở phụ lục/tập minh chứng.

**Ưu điểm/hạn chế/phản biện.** Query JSON gần document, dễ kiểm tra dữ liệu nhúng; regex/array predicate có thể còn phải scan. Hỏi “Nhanh hơn bao nhiêu sau index?”: chưa benchmark, chỉ xác nhận plan và đúng kết quả, không dựng số ms/tỷ lệ tăng tốc.

## F3 Replication và đối chiếu nguồn đích

**Khái niệm/mục đích.** Replication đồng bộ document/revision từ nguồn sang đích; có hướng, checkpoint và xử lý thay đổi. Minh họa khả năng quản trị đặc trưng CouchDB, tách khỏi sao lưu logic của ứng dụng. [C6/C7]

**Nguyên lý.** Changes feed cho biết revision mới; đích nhận phần chưa có. Job một lần kết thúc khi theo kịp changes; continuous tiếp tục theo dõi. Tombstone cũng có thể lan truyền, nên replication liên tục không tự bảo vệ khỏi xóa nhầm.

**Thao tác.** Chủ máy tạo hai DB thử riêng có quyền phù hợp, ví dụ qlbanhang_test_p6_src_CODE và qlbanhang_test_p6_dst_CODE; không thay security của qlbanhang. Nguồn chỉ có 3 document giả customer/product/order, không app_user/hash/credential. Trong Fauxton Replication hoặc API hỗ trợ, chọn nguồn/đích local, one-shot, target đã tồn tại, không gửi credential trong URI/payload. Chạy; ghi response và đọc lại đích. Sửa một document nguồn của chính buổi demo, chạy lại; đối chiếu `_id`, nội dung và `_rev`. Nếu muốn minh họa delete propagation, chỉ xóa document thử tự tạo không bị tham chiếu rồi replicate lại; không xóa DB.

**Ví dụ payload mô tả, chưa chạy:**

```json
{"source":"qlbanhang_test_p6_src_CODE","target":"qlbanhang_test_p6_dst_CODE","create_target":false,"continuous":false}
```

API là POST `/_replicate` có xác thực mới ở header của công cụ riêng. Không ghi job vào `_replicator` kèm password; không sao chép toàn bộ qlbanhang có thể chứa app_user vào DB demo khác.

**Mong đợi/kết quả.** Đích có 3 business document, cùng nội dung/revision nguồn sau khi hoàn tất; lần chạy lại không nhân đôi ID. HTTP ok chưa đủ: phải GET/đếm/query đích. **NOT RUN/BLOCKED credential và quyền**, chưa có bằng chứng replication thật trong P0–P5. Không thay bằng kết quả export/restore vì restore logic cấp revision mới, khác replication.

**Ảnh.** F3-01 nguồn/đích trước chạy; F3-02 cấu hình không secret; F3-03 response/job; F3-04 cùng ID/revision hai DB; F3-05 cập nhật lần hai; F3-06 tombstone chỉ nếu ca đó thật sự thực hiện.

**Ưu điểm/hạn chế/phản biện.** Hữu ích chuyển/sync dữ liệu; cần quyền, có trễ và conflict. Hỏi “Cùng ID hai phía khác nhau sẽ thế nào?”: có thể tạo nhánh revision xung đột, không tự suy nội dung đúng nghiệp vụ. Hỏi “Replica là backup?”: cần phân biệt bản sao đang nhận cả xóa với mốc khôi phục độc lập.

## F4 Authentication Authorization và security

**Khái niệm/mục đích.** Xác định người gọi và quyền database/server, phân biệt với role website. [C4/C5]

**Nguyên lý.** Basic/session thiết lập danh tính; `_security` áp dụng quyền DB; `_admin` là phạm vi server. Empty members có thể mở rộng truy cập; không tự suy cấu hình từ mặc định. `_security` không có `_rev` như ordinary document.

**Thao tác.** Cho xem request ẩn danh chỉ đọc GET /_session và /qlbanhang (không cookie); sau khi credential đã thay, tài khoản được cấp quyền đọc session/security, chỉ lưu role/count đã che tên. Chủ máy trình bày account runtime là DB member, account vận hành index/security có quyền phù hợp, server admin riêng; không nâng quyền trong demo. Trên website đăng nhập Administrator/OrderManager/Shipper do nhóm tự provision, thử GET và POST chức năng vượt quyền; Shipper thử phiếu của người khác. Không chụp ticket/token/hash.

**Ví dụ qlbanhang.** Anonymous P6 thực tế nhận 401 với DB/security/membership, session anonymous không `_admin`. Với DB member/server admin sau đổi credential, kết quả còn **BLOCKED**; website roles có bằng chứng tích hợp P5, chưa diễn tập GUI hôm nay. HTTPS publish **NOT RUN**, không gọi HTTP loopback là đã mã hóa TLS.

**Ảnh.** F4-01 HTTP status anonymous; F4-02 security đã che tên; F4-03 tài khoản website và vai trò, không hash; F4-04 GET/POST bị chặn; F4-05 Shipper chỉ thấy phiếu mình; F4-06 cookie Secure/HTTPS chỉ sau khi triển khai thật và không lộ giá trị cookie.

**Ưu điểm/hạn chế/phản biện.** Phân quyền DB và website bổ sung nhau; người giữ credential DB có thể vượt giới hạn nghiệp vụ. Hỏi “403 nghĩa chắc sai password?”: không, phải xét endpoint/ngữ cảnh; 401/403 có thể liên quan xác thực/quyền/cơ chế bảo vệ. Không bật quyền anonymous để tránh lỗi.

## F5 Import Export backup logic và restore

**Khái niệm/mục đích.** Tạo bản sao nghiệp vụ để di chuyển/khôi phục có kiểm chứng. Fauxton giúp quan sát trước/sau; công cụ của repo thực hiện export/import qua API, không nói GUI cung cấp backup hệ thống chỉ bằng JSON editor. [C8]

**Nguyên lý.** Snapshot có schema/checksum, bốn loại nghiệp vụ và JSON index; bỏ `_rev`, syncToken và metadata được quy định. Còn pending sync thì từ chối. Restore chỉ create-only vào DB thử, duplicate khác nội dung bị chặn; revision đích được cấp mới.

**Thao tác.** Sau khi cấu hình mới an toàn, export qlbanhang vào file mới trong artifacts/backups. Chạy import với --target qlbanhang_restore_p6_CODE, verify hash/count/index rồi mở DB đích trong Fauxton. Chạy import lại và xác nhận skip identical/revision không đổi. Thử file sai checksum hoặc trùng ID khác nội dung trong DB thử riêng, không sửa file/DB chính để dựng lỗi. Không export app_user/quyền/config.

**Ví dụ lệnh (chỉ chạy khi đã gỡ blocker):**

```powershell
python scripts/Business-Backup.py export artifacts/backups/p6-CODE.local.json
python scripts/Business-Backup.py import artifacts/backups/p6-CODE.local.json --target qlbanhang_restore_p6_CODE
python scripts/Business-Backup.py verify artifacts/backups/p6-CODE.local.json --target qlbanhang_restore_p6_CODE
```

**Mong đợi/kết quả.** P5 từng PASS 104 nghiệp vụ/9 JSON index, hash/count/định nghĩa index khớp, nhập lại skip 104. Đây là bằng chứng lịch sử; nguồn đã thay đổi có thể cho count khác, P6 chưa đọc lại được DB. Restore mới hiện **BLOCKED**; backup đầy đủ hệ thống **NOT RUN**.

**Ảnh.** F5-01 file/log số lượng an toàn; F5-02 verify; F5-03 documents/index DB đích; F5-04 duplicate bị từ chối; F5-05 source giữ nguyên và không xóa database.

**Ưu điểm/hạn chế/phản biện.** Kiểm chứng nội dung tốt hơn chỉ thông báo thành công; không khôi phục user/security/config/revision history/attachments. Hỏi “Có thể phục hồi server hoàn chỉnh từ file này không?”: không, phải có kế hoạch sao lưu hệ thống riêng. Hỏi “Checksum bảo đảm người gửi đáng tin?”: không, đây không phải chữ ký số.
