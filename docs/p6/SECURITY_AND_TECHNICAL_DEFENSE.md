# Bảo mật và các luận điểm kỹ thuật để bảo vệ đồ án

## Ba tầng danh tính và quyền

Authentication trả lời danh tính của bên gọi; authorization quyết định danh tính đó được làm gì trên tài nguyên cụ thể. Trong đề tài cần phân biệt người dùng website, tài khoản dịch vụ kết nối CouchDB và người vận hành Fauxton. Việc website có vai trò Administrator không tự cấp `_admin` trên CouchDB; ngược lại, DB member không tương đương Shipper của website. [C4/C5 trong SOURCES.md; AppSecurity.cs]

| Chủ thể | Phạm vi | Ví dụ được phép | Không được suy ra |
|---|---|---|---|
| CouchDB server admin | Cài đặt/máy chủ và DB theo API | Tạo/xóa DB, cấu hình máy chủ khi được cấp quyền | Không nên dùng credential này trong browser hoặc mặc định làm login website |
| CouchDB database admin | Một database theo `_security` | Quản lý design docs/index/quyền DB, ngoài quyền member | Không tự có quyền tạo/xóa database hay cấu hình cả server |
| CouchDB database member | Dữ liệu thường trong database | Đọc/ghi ordinary documents | Không được quản lý design docs, không phải quyền đọc theo từng phiếu của website |
| Administrator website | Chức năng MVC | Tạo/khóa app_user, quản lý nghiệp vụ, retry sync | Không tự là CouchDB server admin |
| OrderManager website | Nghiệp vụ cửa hàng | Customer/Product/Order, phân công Delivery | Không quản trị app_user |
| Shipper website | Phiếu của chính username được giao | assigned→shipping→delivered/failed | Không được quản lý khách/đơn hoặc phiếu của người khác |

`_security` chứa admins/members, mỗi nhóm có names/roles. Đây không phải document thường có `_id/_rev`, không áp dụng MVCC như Customer. Theo tài liệu, database admin có quyền quản lý design docs và quyền DB; server admin là phạm vi khác. Nếu cấu hình thành viên rỗng có thể cho phép truy cập rộng hơn dự định; CouchDB 3.x đặt `_admin` mặc định cho DB mới để giảm truy cập ngoài ý muốn. Phải đọc cấu hình thật, không suy từ mặc định hoặc ảnh cũ. [C4]

Mẫu thiết kế quyền dùng để giải thích, **không phải cấu hình đã áp dụng**:

```json
{"admins":{"names":["DB_ADMIN_PLACEHOLDER"],"roles":[]},"members":{"names":["APP_DB_MEMBER_PLACEHOLDER"],"roles":[]}}
```

Tên trên là ký hiệu, không tài khoản mặc định. Không chạy PUT `_security`, tạo `_users` hoặc cấp `_admin` chỉ để demo PASS. Chủ máy cấu hình bằng tài khoản đủ quyền; nhóm dùng tài khoản ứng dụng mới có quyền tối thiểu cần thiết. Khi tách account tạo DB/index khỏi account runtime, phải kiểm thử lại quyền từng API thay vì thêm admin cho tất cả.

## Xác thực CouchDB và mã hóa đường truyền

Basic authentication gửi thông tin được encode, không phải mã hóa bí mật bằng mật mã. Vì vậy HTTPS là yêu cầu cho kết nối từ xa; không đưa credential vào URL vì có thể đi vào history/log. CouchDB cũng có cookie authentication qua session; cookie là bí mật cần bảo vệ, không phải bằng chứng được chia sẻ lên báo cáo. Repo dùng Basic từ HttpClient phía server, lấy cấu hình từ biến môi trường; không gửi nó sang JavaScript/HTML. [C5; CouchDbService.cs]

TLS bảo vệ dữ liệu trên kênh truyền, không giải quyết mật khẩu đã nằm trong Git, quyền quá rộng hoặc dữ liệu nhạy cảm ở bản export. HTTP loopback được dùng cho demo local, không phải kết luận đã kiểm thử HTTPS triển khai thật. Web.Release.config bật secure cookie khi publish, nhưng phải kiểm tra chứng chỉ/binding/redirect/cookie thực tế trước khi đánh dấu HTTPS PASS.

Phiên P6: GET / và /_up ẩn danh 200, server version 3.5.2. GET /_session cho thấy anonymous, không `_admin`; /qlbanhang, /qlbanhang/_security, /_membership và hai ID mẫu đều 401. Không gửi Authorization/cookie và không ghi dữ liệu. Kết quả này chứng minh các request cụ thể không được đọc ẩn danh; **không** cho biết nội dung `_security`, quyền member đã đăng nhập hoặc single-node đã cấu hình xong. 403 lịch sử của request có xác thực cũng không tự chứng minh loại quyền: có thể thiếu quyền hoặc bị bảo vệ sau nhiều lần sai. [C4/C7 + P6-ReadOnlyProbe.py]

## Bảo mật website hiện có và phần còn thiếu

Mật khẩu app_user được băm bằng PBKDF2-HMAC-SHA256, salt ngẫu nhiên 16 byte, 600000 vòng, output 32 byte. Salt/hash được lưu, không lưu plaintext; kiểm tra mật khẩu so sánh byte không dừng tại byte sai đầu tiên. Đây là mô tả code thực tế, không phải tuyên bố đã audit độc lập mọi kênh timing. `FormsAuthentication` bảo vệ ticket; mỗi request kiểm tra lại enabled/role từ DB, nên khóa tài khoản có hiệu lực request sau. Cookie HttpOnly/SameSite, phiên 30 phút; bộ đếm login thất bại trong bộ nhớ không thay rate limit phân tán khi mở dịch vụ công khai. [AppSecurity.cs, AccountController.cs, Web.config]

CSRF được kiểm tra bằng anti-forgery token ở POST thay đổi trạng thái; việc có role đúng không thay thế token. Ngược lại, có token không cấp quyền sửa tài liệu người khác. Validation kiểm tra dữ liệu đầu vào và nghiệp vụ ở server; kiểm tra frontend chỉ hỗ trợ trải nghiệm. Tổng đơn được tính từ items, chặn quantity không dương, giá âm/quá miền, tham chiếu sai và chuyển trạng thái sai. Mọi giá trị HTML từ dữ liệu vẫn cần encoding khi render. [Controllers, WorkflowService, 49 smoke và tích hợp P5]

`_security` là quyền mức database; không tự lọc dữ liệu theo Shipper. Vì app_user đang cùng database nghiệp vụ, người có credential DB đủ đọc có thể đọc hash và dữ liệu rộng hơn quyền website. Do đó không giao credential DB cho nhân viên; bảo vệ cổng/mạng/Fauxton, cân nhắc tách kho tài khoản/credential dịch vụ cho triển khai thật. Tài khoản có quyền ghi DB trực tiếp có thể vượt state machine MVC; không gọi đây là ràng buộc máy chủ không thể vượt qua. [C4 + cấu trúc service]

## Bí mật và lịch sử Git

Biến môi trường tách cấu hình khỏi source, nhưng không tự thu hồi giá trị từng bị commit. Audit tại 1d88092 quét 11 commit/258 blob local và xác nhận mật khẩu đang cấu hình trùng literal lịch sử. Không thử lại credential lấy từ commit cũ. Đây là blocker phát hành, dù build/unit và các tích hợp trước đó từng đạt. Chủ repo phải đổi/thu hồi credential, cập nhật môi trường và xác minh Private/quyền GitHub; không gửi bí mật mới qua chat hoặc ảnh. Nhánh P6 không rewrite commit P0–P5. [RELEASE_READINESS.md]

Word gốc chứa thông tin liên hệ/MSSV hoặc ảnh có thể nhạy cảm nên chỉ giữ local trong review_input, không commit. Bản trích che dữ liệu; ảnh chưa được kiểm tra pixel không được phát hành. Export nghiệp vụ loại trừ app_user, quyền, cấu hình và secret-bearing fields; checksum phát hiện thay đổi nội dung chứ không xác thực nguồn. Chỉ import tệp đáng tin.

## MVCC revision và 409

CouchDB dùng cơ chế versioning/concurrency theo document. Client sửa hoặc xóa phải gửi revision phù hợp; revision cũ có thể gây 409. `_rev` là token điều phối cập nhật/nhánh revision, không phải timestamp, số thứ tự nghiệp vụ toàn hệ thống hoặc bản backup vĩnh viễn. Không parse hash suffix để suy ra ai sửa/lúc nào. History nghiệp vụ của app được lưu riêng. [C1/C2]

Hai tab đọc cùng revision: tab một ghi thành công, tab hai không được tự retry với revision mới để ghi đè. Cần tải lại, đối chiếu thay đổi và gửi quyết định của người có quyền. Với replication, các nhánh xung đột có thể cùng tồn tại; xác định winning revision không có nghĩa nội dung thắng luôn đúng nghiệp vụ. Cần quy trình phân giải miền, không gọi tự chọn winner là merge thông minh. [C1/C6]

## Embedding tham chiếu và transaction nhiều document

Order nhúng items vì cần tính tổng và đọc cùng nhau. Giá item là snapshot giao dịch; không join trực tiếp giá hiện tại để sửa lịch sử. Customer/Product/Delivery lưu riêng để tránh nhân bản toàn bộ hồ sơ và có vòng đời riêng. `customerId/productId/orderId` là mã tham chiếu, không phải FK do CouchDB áp dụng. Kiểm tra trước khi xóa có thể gặp race khi nhiều writer hoặc thao tác ngoài app; chưa có foreign key serializable. [DATABASE_DESIGN.md, C1]

Order và Delivery là hai document nên không có transaction ACID tổng quát cho cả hai PUT. App ghi Order kèm pendingDelivery trước, dùng CAS để tuần tự hóa transition của cùng Order; sau đó ghi projection Delivery với syncToken và xác nhận hết pending. Nếu chết giữa chừng, intent còn để admin retry; nếu revision đích khác expectedRev thì dừng, giữ intent, không overwrite. Đọc Delivery có thể tạm thấy trạng thái cũ trong lúc chờ. Đây là nhất quán cuối cùng **của projection ứng dụng**; không đồng nhất nó với replication giữa các node CouchDB. [WorkflowService.cs, C1/C2]

`_bulk_docs` không biến nhiều document thành transaction toàn bộ thành công/thất bại. Outbox cũng không tự xử lý stock reservation, thanh toán hoặc mọi race tham chiếu. Những việc này vẫn là giới hạn để nghiên cứu tiếp. MongoDB và RavenDB có các lựa chọn multi-document/cluster transaction riêng; không dùng giới hạn CouchDB để kết luận mọi NoSQL giống nhau. [M3, R1/R2]

## Index query và phục hồi

Mango selector biểu diễn điều kiện; sort cần chỉ mục phù hợp. `use_index` là gợi ý, phải đọc `_explain`/warning để biết kế hoạch thực tế. Một index được chọn không chứng minh mọi biểu thức `$regex` hoặc `$elemMatch` đều được index tối ưu. Bookmark là token phân trang; giữ selector/sort và tập dữ liệu ổn định, không coi việc có bookmark là chắc còn trang sau. [C3]

Replication là quá trình đồng bộ có hướng, dùng changes/revision và checkpoint. Muốn hai hướng cần thiết kế hai hướng tương ứng; replication không mặc nhiên là giao dịch đồng bộ giữa mọi node. Tombstone có thể truyền sang đích, vì thế bản replica liên tục không thay mọi yêu cầu backup chống xóa nhầm. [C6]

Snapshot logic của repo chứa bốn loại nghiệp vụ và JSON indexes, bỏ revision/metadata đồng bộ nhất định; restore tạo revision mới. Nó không giữ `_users`, `_security`, app_user, config/log/attachments hay toàn bộ lịch sử. Verify phải so sánh count, nội dung chuẩn hóa và index, không chỉ HTTP 200. Dừng ghi hoặc kiểm tra update_seq để tránh nhận một ảnh chụp bị thay đổi trong lúc export. Backup toàn hệ thống có phạm vi/quyền khác và chưa được nghiệm thu. [C8 + BACKUP_RESTORE.md]
