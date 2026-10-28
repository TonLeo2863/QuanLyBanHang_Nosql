# Demo Fauxton – nhóm 7

Fauxton là giao diện web quản trị CouchDB, được cung cấp cùng máy chủ tại `http://127.0.0.1:5984/_utils/`. Browser gọi HTTP API CouchDB; ứng dụng MVC gọi API từ server bằng cấu hình riêng. Trình duyệt ứng dụng không được nhận credential CouchDB. Fauxton không phải một DB riêng hay ORM.

Chuẩn bị: hoàn tất single-node bằng tài khoản quản trị riêng; chạy Initialize-CouchDb.ps1, Seed-Demo.py; dùng dữ liệu giả. Phiên API trước nhận 403 ở `/_membership`, không suy ra Setup đã hoàn tất. Không chụp hộp đăng nhập, token, header Authorization, cookie hay màn hình config chứa bí mật.

1. Mở URL `_utils/`; ghi phiên bản CouchDB/Fauxton trong biên bản demo. Dùng **Log In**, nhập thông tin riêng trên máy, không đưa vào báo cáo.
2. Vào **Databases**, chọn `qlbanhang`; chụp danh sách DB. Không bấm Delete Database.
3. **All Documents**, tìm `p2-customer-001`; xem JSON và `_id`, `_rev`, `type`, email miền `.invalid`.
4. **Create Document**, tạo ID `fauxton-demo-<mã buổi demo>` chưa tồn tại, JSON `{"type":"customer","name":"Khách thử Fauxton","email":"demo@example.invalid"}`. Lưu; ghi lại revision.
5. Mở đúng document vừa tạo, sửa tên thành `Khách thử đã sửa`, lưu; quan sát `_rev` đổi. Thử tab cũ để minh họa conflict nếu cần, không ghi đè.
6. Chỉ xóa document vừa tạo bằng chức năng Delete trong editor, xác nhận đúng ID; không thao tác tài liệu có sẵn. Quan sát biến mất khỏi danh sách live (tombstone không phải xóa lịch sử vật lý).
7. Mở **Mango Indexes / Manage Indexes** theo phiên bản; tạo JSON index fields `["type","status","_id"]`, name `type-status-id`, design doc `idx-type-status-id`. Nếu đã có từ seed, xem lại; không xóa index để làm demo.
8. Mở **Mango Query / Run A Query**, dán phần `query` của B1 trong [mango-queries.json](mango-queries.json), chạy và kiểm tra seedCount=20. Kết quả có thể gồm `demo-*` hoặc dữ liệu ngoài seed, nên không đồng nhất tổng DB với số tài liệu P2.
9. Chạy B2–B5 và A1–A5, đối chiếu `expectedSeedCount` cho các ID `p2-*`. Chụp selector và kết quả thực tế. `$elemMatch` minh họa item nhúng; `$regex` có thể cần quét trong CouchDB, không tuyên bố tối ưu bằng index chuỗi thông thường.
10. Thử limit=7, dùng bookmark trả về cho trang sau, giữ selector/sort không đổi. Dùng REST API `POST /qlbanhang/_explain` với cùng query để kiểm tra tên index; không khẳng định Fauxton mọi phiên bản đều có nút Explain. Test tự động: `tests/Test-Mango.py`.

| Công việc | Fauxton trực tiếp | Công cụ bổ sung / giới hạn |
|---|---|---|
| Database/document/index/Mango | có màn hình tương ứng | quyền phụ thuộc tài khoản; API vẫn là nền tảng |
| Replication | có màn hình cấu hình tùy phiên bản/quyền | bản sao có thể truyền cả thay đổi/xóa; không thay backup có lịch sử |
| Export/import chuẩn bàn giao | không coi JSON editor là backup toàn DB | dùng công cụ backup logic P5, kiểm tra file và từng kết quả ghi |
| Sao lưu đầy đủ máy chủ | không thực hiện bằng tải JSON trên GUI | cần quản trị file `.couch`, cấu hình, log/quy trình vận hành |
| Người dùng/quyền | xem `_users` và security theo quyền | tài khoản app website tách biệt; không đưa hash người dùng vào ảnh/export demo |

Ưu điểm: đi cùng CouchDB, xem JSON/revision và thử HTTP query thuận tiện cho document có item nhúng; không cần cài GUI desktop riêng. Giới hạn: không có join hoặc transaction xuyên document, GUI không thay kiểm tra nghiệp vụ, quyền admin và backup hệ thống cần kiểm soát riêng. Thích hợp minh họa đơn hàng có cấu trúc item thay đổi và truy vấn JSON; không khẳng định là lựa chọn tối ưu cho mọi giao dịch cần ACID nhiều document.

## So sánh với MongoDB Compass

| Tiêu chí | Fauxton | MongoDB Compass |
|---|---|---|
| Hệ DB | CouchDB, HTTP JSON/revision | MongoDB; không kết nối trực tiếp thay thế Fauxton trên CouchDB |
| Quản trị dữ liệu | database/document editor | database/collection/document editor |
| Trực quan hóa | JSON, danh sách và kết quả query | khám phá schema và môi trường phân tích trực quan |
| Query | Mango, views; HTTP API | MongoDB query/aggregation pipeline builder |
| Index | Mango JSON index và design doc | index MongoDB trong collection |
| Dễ dùng | nhanh cho người đã hiểu JSON/CouchDB | nhiều chức năng phân tích, cần hiểu MongoDB |
| Triển khai | browser + máy chủ CouchDB | app desktop Windows/macOS/Linux + MongoDB |
| Import/export | dùng script/API cho quy trình được kiểm chứng | có import/export JSON/CSV; vẫn không thay backup hệ thống |
| Phù hợp | quản trị/demonstration CouchDB | khám phá/phát triển ứng dụng MongoDB |

Nguồn chính thức (tra cứu 28/10/2026): [Fauxton](https://docs.couchdb.org/en/stable/fauxton/index.html), [Mango và explain](https://docs.couchdb.org/en/stable/api/database/find.html), [Backup CouchDB](https://docs.couchdb.org/en/stable/maintenance/backups.html), [MongoDB Compass](https://www.mongodb.com/docs/compass/). Các đoạn trên là tổng hợp, không phải ảnh hoặc biên bản thao tác GUI đã chạy.
