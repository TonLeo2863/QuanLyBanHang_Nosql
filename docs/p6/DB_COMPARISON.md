# So sánh hệ quản trị và lập luận lựa chọn CouchDB

## Bảng 16 đặc trưng kỹ thuật

Các ô là mô tả khả năng sản phẩm theo nguồn chính thức tại SOURCES.md, không phải benchmark và không khẳng định cấu hình mặc định đáp ứng mọi bảo đảm. Không cài MongoDB/RavenDB/PostgreSQL trong phiên P6.

| Đặc trưng | CouchDB | MongoDB | RavenDB | PostgreSQL | Nguồn |
|---|---|---|---|---|---|
| 1. Mô hình logic | Document JSON trong database | Document BSON trong collection | Document JSON, session/client | Quan hệ, bổ sung json/jsonb | C1, M1/M2, R1, P7 |
| 2. Cấu trúc lồng | Object/mảng trong một document | Embedded document/mảng | Đối tượng con trong document | Bảng con hoặc jsonb tùy thiết kế | C1, M1, R1, P1 |
| 3. Định danh | _id duy nhất trong DB | _id trong collection; thường ObjectId | ID document do client/server quy ước | PK khai báo trên bảng | C1, M2, R1, P2 |
| 4. Tổ chức quan hệ | Mã tham chiếu logic, app đối chiếu | Embed hoặc references theo truy cập | Load/include document liên quan | JOIN và FK cho quan hệ được khai báo | C1, M1/M7, R1, P2 |
| 5. Giao diện ứng dụng | HTTP/JSON thuận tiện cho HttpClient | Driver và giao thức MongoDB | Client session, HTTP API | SQL qua driver | C1, M1/M3, R1, P3 |
| 6. Truy vấn | Mango selector và views | Query/aggregation | Query gắn hệ thống indexes | SQL, JSON operators/jsonpath | C3, M4/M6, R3, P1/P5 |
| 7. Ranh giới atomic | Ghi một document; không transaction tổng quát nhiều doc | Một document atomic; hỗ trợ transaction nhiều doc | SaveChanges/batch có phạm vi transaction; có chế độ cluster-wide | Transaction nhiều hàng/bảng | C1/C2, M3, R1/R2, P3 |
| 8. Cạnh tranh cập nhật | Gửi _rev hiện tại, xử lý 409 | Điều kiện update/version miền hoặc transaction | Change vector/optimistic concurrency theo cấu hình | Khóa hàng và isolation/MVCC | C1/C2, M3, R1, P3 |
| 9. Nhất quán | Phân biệt single-doc/quorum và replication giữa bản sao | Read/write concerns quyết định bảo đảm quan sát | Phân biệt document transaction, cluster transaction, index cập nhật | Isolation lựa chọn; replica có thể trễ | C2, M3/M5, R1/R2/R3, P3/P4 |
| 10. Replication | Theo changes/revision, có checkpoint; mỗi job có hướng | Replica set primary/secondary | Replication database và cluster-wide transaction là hai khái niệm khác nhau | WAL/streaming; lựa chọn sync/async | C6, M5, R1/R2, P4 |
| 11. Chỉ mục | JSON Mango/design doc, chọn theo selector/sort | Single/compound/multikey và nhiều loại khác | Auto/dynamic và static indexes | B-tree và GIN cho nhiều kiểu truy vấn JSON | C3, M4, R3, P1/P5 |
| 12. Kiểm tra kế hoạch | _explain, execution_stats | Công cụ explain/query trong hệ sinh thái MongoDB | Quan sát query/index trong Studio | EXPLAIN; ANALYZE thực sự chạy query | C3, M6, R3/R5, P5 |
| 13. Ràng buộc dữ liệu | Schema linh hoạt; app/validation phải chịu trách nhiệm miền | Schema linh hoạt, có thể bổ sung validation | Miền ứng dụng + API/concurrency, không giả định FK SQL | NOT NULL/CHECK/PK/FK được khai báo | C1, M1/M7, R1, P2 |
| 14. Số tiền | JSON number; demo tự giới hạn 2 số lẻ, không có kiểu Decimal128 riêng | BSON có Decimal128 | Kiểu serialize/client cần được thiết kế/kiểm thử | numeric có precision/scale | C1 + MoneyAttribute repo, M2, R1, P7 |
| 15. Sao lưu và phục hồi | Snapshot/replication/file/config là các phạm vi khác nhau | Phương án theo deployment, không coi Compass export là backup toàn hệ thống | Studio có import/export; phạm vi backup phải xác định riêng | SQL dump, file và WAL/PITR | C8, M6, R5, P6 |
| 16. Công cụ quản trị | Fauxton web đi cùng CouchDB | Compass desktop cho MongoDB | Studio web đi cùng RavenDB | Hệ sinh thái SQL/client; không dùng Fauxton | C9, M6, R5, P5 |

Không gọi mọi thao tác NoSQL là eventual consistency, hoặc khẳng định NoSQL không có transaction. Ngược lại, SQL không đồng nghĩa chỉ có bảng phẳng: PostgreSQL hỗ trợ JSONB và index. Không suy từ một bản local nhỏ rằng CouchDB nhanh hơn các hệ còn lại.

## Vì sao chọn CouchDB cho đề tài này

Đơn hàng được đọc cùng các dòng hàng nên mảng items nằm trong document Order là một ranh giới dữ liệu tự nhiên. Giá từng dòng được lưu theo giao dịch để thay đổi giá Product không sửa ngược lịch sử. Customer/Product có vòng đời độc lập nên Order lưu mã tham chiếu và địa chỉ giao nhận. Thiết kế này phù hợp với API document, nhưng không tạo ra khóa ngoại tự động. Lập luận mô hình dựa trên mẫu truy cập, không dựa vào số lượng field hoặc trào lưu NoSQL. [C1, M1; đối chiếu Models/Order.cs và WorkflowService.cs]

Lý do quyết định trong phạm vi học phần là đề tài tập trung Fauxton: cùng một JSON có thể quan sát ở giao diện quản trị và gọi bằng HttpClient từ MVC5. Revision, selector, index và replication tạo thành các nội dung quản trị có thể giải thích, kiểm tra chéo và phản biện. Việc giữ CouchDB cũng bảo toàn ứng dụng P0–P5 đang build/test đạt; chuyển DB lúc này làm mất trọng tâm và tạo chi phí kiểm thử lại. Đây là đánh giá phù hợp mục tiêu đồ án, không phải xếp hạng ưu việt tuyệt đối. [C1/C3/C6/C9 và bằng chứng repo]

Đổi lại, hệ thống phải tự kiểm tra tham chiếu, quy tắc trạng thái, phép tính tổng và xử lý projection Delivery có thể chậm sau khi Order được ghi. Durable outbox trong Order giữ yêu cầu đang chờ; CAS theo _rev chống ghi đè và retry có syncToken tránh lặp. Cơ chế này cung cấp khả năng phục hồi và nhất quán cuối cùng ở tầng ứng dụng, không tương đương transaction ACID hai document. Quantity sản phẩm hiện không phải cơ chế đặt giữ tồn kho an toàn khi cạnh tranh. Một hệ bán lẻ sản xuất còn cần thiết kế thêm và đánh giá lại lựa chọn DB theo nghiệp vụ tài chính/kho. [C1/C2 + source WorkflowService]

## Vì sao chưa chọn các phương án khác

**MongoDB** cũng phù hợp document và item nhúng, có nhiều loại index, aggregation và transaction nhiều document. Vì vậy không được nói MongoDB thiếu transaction để biện hộ cho CouchDB. Tuy nhiên Compass không quản trị CouchDB; thay sang MongoDB không đáp ứng trọng tâm Fauxton và cần thay driver/truy vấn/cơ chế concurrency của app. Nhóm giữ nó làm phương án so sánh kỹ thuật, không triển khai một bản ứng dụng thứ hai. [M1–M6]

**RavenDB** có session/client thuận tiện cho .NET, automatic/static indexing, quản trị web và các lựa chọn transaction. Không được diễn giải mọi document DB đều có cùng giới hạn như CouchDB. Dùng RavenDB cần học API/index/security bằng certificate của nó và kiểm tra version client tương thích .NET Framework 4.7.2; việc đó không trực tiếp phục vụ demo Fauxton hiện tại. Không đưa ra kết luận về giá/license hoặc tốc độ khi chưa khảo sát những tiêu chí ấy. [R1–R5]

**PostgreSQL** là lựa chọn mạnh nếu ưu tiên JOIN, FK, transaction nhiều bảng và dữ liệu kho/tiền cần ràng buộc chặt. JSONB cũng cho phép phần mở rộng linh hoạt, nên không loại nó chỉ vì dữ liệu đơn hàng lồng nhau. Nhóm không chọn PostgreSQL vì mục tiêu là nghiên cứu công cụ quản trị CouchDB và document API; nếu phát triển hệ thống thương mại thật với giao dịch nhiều thực thể, cần cân nhắc lại thay vì mặc định tiếp tục lựa chọn học phần. [P1–P6]

## So sánh GUI ngắn theo rubric

| Tiêu chí | Fauxton | MongoDB Compass |
|---|---|---|
| DBMS được quản trị | CouchDB | MongoDB, không thay Fauxton trên CouchDB |
| Triển khai | Web cùng server | Ứng dụng desktop kết nối MongoDB |
| Document/query | JSON/revision/Mango | BSON/JSON view, query và aggregation |
| Index | Mango/design document | Các index thuộc MongoDB |
| Minh chứng trong đồ án | Phải demo trực tiếp ít nhất 3 chức năng Fauxton | Chỉ so sánh tài liệu; không bắt buộc cài/demo |
| Giới hạn | Không phải UI nghiệp vụ hoặc backup hệ thống hoàn chỉnh | Export dữ liệu không tự thay quy trình backup MongoDB |

RavenDB Studio chỉ được nhận diện để tránh nhầm thuật ngữ: là GUI web quản trị RavenDB, hỗ trợ documents, indexes, query, tasks/import/export theo quyền. Đính chính hiện tại không yêu cầu demo ba phần mềm GUI. Tối thiểu ba chức năng được chọn là CRUD/revision, Mango/index/explain và replication; security và backup/restore là hai nhóm bổ sung. [C9, M6, R5]
