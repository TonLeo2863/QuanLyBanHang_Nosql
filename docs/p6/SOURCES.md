# Nguồn chính thức dùng cho phần bổ sung P6

Tra cứu ngày 28/10/2026. Đây là danh mục nguồn đã mở để đối chiếu, không phải bằng chứng cài đặt các hệ khác hoặc đo hiệu năng. CouchDB dùng tài liệu nhánh stable 3.5; RavenDB cố định tài liệu 7.1; PostgreSQL cố định tài liệu 18; MongoDB dùng Manual hiện hành tại ngày tra cứu. Máy demo chỉ được quan sát CouchDB server 3.5.2 qua GET / ẩn danh.

| Mã | Trang chính thức | Nội dung sử dụng |
|---|---|---|
| C1 | [CouchDB Document API](https://docs.couchdb.org/en/stable/api/document/common.html) | GET/PUT/DELETE, revision và 409 |
| C2 | [CouchDB Eventual Consistency](https://docs.couchdb.org/en/stable/intro/consistency.html) | MVCC, ranh giới nhất quán và phân tán |
| C3 | [Mango find/index/explain](https://docs.couchdb.org/en/stable/api/database/find.html) | selector, sort, bookmark, use_index, kế hoạch và thống kê |
| C4 | [Database security API](https://docs.couchdb.org/en/stable/api/database/security.html) | admins/members, _security không phải document MVCC |
| C5 | [CouchDB Security](https://docs.couchdb.org/en/stable/intro/security.html) | authentication và quyền quản trị |
| C6 | [Replication introduction](https://docs.couchdb.org/en/stable/replication/intro.html) | hướng đồng bộ, changes feed/checkpoint, tombstone |
| C7 | [Server API / replicate](https://docs.couchdb.org/en/stable/api/server/common.html#post--_replicate) | phiên bản máy chủ và API replication |
| C8 | [Backing up CouchDB](https://docs.couchdb.org/en/stable/maintenance/backups.html) | phân biệt dữ liệu, cấu hình, log và phương án sao lưu |
| C9 | [Fauxton](https://docs.couchdb.org/en/stable/fauxton/index.html) | giao diện quản trị thuộc CouchDB |
| M1 | [MongoDB data modeling](https://www.mongodb.com/docs/manual/data-modeling/) | embedding/references và thiết kế theo truy cập |
| M2 | [MongoDB BSON types](https://www.mongodb.com/docs/manual/reference/bson-types/) | BSON, ObjectId, Decimal128 |
| M3 | [MongoDB transactions](https://www.mongodb.com/docs/manual/core/transactions/) | atomic document và distributed transaction/concerns |
| M4 | [MongoDB index types](https://www.mongodb.com/docs/manual/core/indexes/index-types/) | compound, multikey và index khác |
| M5 | [MongoDB replication](https://www.mongodb.com/docs/manual/replication/) | replica set primary/secondary |
| M6 | [MongoDB Compass](https://www.mongodb.com/docs/compass/) | GUI, query, aggregation, import/export |
| M7 | [MongoDB schema validation](https://www.mongodb.com/docs/manual/core/schema-validation/) | Quy tắc kiểu/miền dữ liệu, reject hoặc warn theo cấu hình |
| R1 | [RavenDB session](https://docs.ravendb.net/7.1/client-api/session/what-is-a-session-and-how-does-it-work) | unit of work, batching/transaction, concurrency |
| R2 | [RavenDB cluster transaction](https://docs.ravendb.net/7.1/client-api/session/cluster-transaction/overview) | chế độ cluster-wide, phạm vi bảo đảm |
| R3 | [RavenDB indexing basics](https://docs.ravendb.net/7.1/indexes/indexing-basics) | dynamic/auto và static indexes |
| R4 | [RavenDB security](https://docs.ravendb.net/7.1/server/security/overview/) | X.509/TLS và authorization |
| R5 | [RavenDB Studio](https://docs.ravendb.net/7.1/studio/overview/) | phạm vi GUI quản trị RavenDB, không phải yêu cầu demo bổ sung |
| P1 | [PostgreSQL JSON types](https://www.postgresql.org/docs/18/datatype-json.html) | json/jsonb/jsonpath, GIN |
| P2 | [PostgreSQL constraints](https://www.postgresql.org/docs/18/ddl-constraints.html) | PK/FK/CHECK/NOT NULL |
| P3 | [PostgreSQL isolation](https://www.postgresql.org/docs/18/transaction-iso.html) | Read Committed/Repeatable Read/Serializable |
| P4 | [PostgreSQL standby](https://www.postgresql.org/docs/18/warm-standby.html) | WAL/streaming, đồng bộ hoặc bất đồng bộ theo cấu hình |
| P5 | [PostgreSQL EXPLAIN](https://www.postgresql.org/docs/18/using-explain.html) | planner, EXPLAIN/ANALYZE |
| P6 | [PostgreSQL backup](https://www.postgresql.org/docs/18/backup.html) | SQL dump, filesystem, continuous archive/PITR |
| P7 | [PostgreSQL numeric types](https://www.postgresql.org/docs/18/datatype-numeric.html) | numeric/decimal, precision/scale và miền tiền |
| N1 | [NuGet restore](https://learn.microsoft.com/en-us/nuget/consume-packages/package-restore) | khôi phục packages.config từ manifest/cache/source |
| N2 | [MSBuild NuGet targets](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#restorepackagesconfig) | RestorePackagesConfig=true |

Không dùng mục [7] mơ hồ trong bản A làm nguồn học thuật khi chưa xác định được ấn phẩm thực. Không tự xác nhận đã đọc sách [6] hoặc bài giảng [8]/[9] của bản A; nhóm phải cung cấp bản/edition/trang cụ thể. Đính chính “ít nhất 3 công cụ = chức năng Fauxton” lấy từ yêu cầu hiện tại của chủ dự án, không gán thành trích dẫn nguyên văn từ PDF giảng viên chưa được cung cấp trong review_input.

Khi biên tập Word, thay mã C/M/R/P bằng đánh số tài liệu tham khảo thống nhất và gắn số ngay sau luận điểm. Không lấy ảnh trên tài liệu hãng làm ảnh máy nhóm chạy; nếu dùng hình lý thuyết của hãng phải ghi rõ nguồn, phân biệt với ảnh minh chứng tự chụp.
