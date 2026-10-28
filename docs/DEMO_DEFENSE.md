# Kịch bản demo và bảo vệ đồ án NoSQL nhóm 7

## Phạm vi và điều kiện mở đầu

Đính chính áp dụng: ít nhất 3 chức năng trong Fauxton, **không bắt buộc demo 3 phần mềm GUI**. Không cài MongoDB/RavenDB/PostgreSQL. Compass chỉ là bảng so sánh ngắn; nghiên cứu 4 DBMS nằm trong p6/DB_COMPARISON.md.

Hiện trạng: tài liệu/code/build/unit đã kiểm tra; credential còn trùng lịch sử, DB có xác thực chưa được đọc lại, GUI và replication P6 chưa thực hiện. Sinh viên phải thay/thu hồi credential, xác nhận quyền/Private, provision tài khoản website riêng và diễn tập trước bảo vệ. Chưa có điều kiện thì ghi BLOCKED, không trình bày kế hoạch như kết quả đã chạy.

## Lịch trình đề nghị khoảng 20 phút

| Bước | Nội dung trực tiếp | Kiểm tra chéo và căn cứ |
|---|---|---|
| 1, 2 phút | Giới thiệu qlbanhang, CouchDB server và Fauxton; phân biệt website MVC | GET / cho version; không gọi version Fauxton là version DB nếu chưa đối chiếu |
| 2, 3 phút | F1 đọc p2-customer/order, tạo/sửa/xóa đúng document mới trong DB thử, stale revision | So ID/revision; không xóa seed hoặc DB; giải thích 409 |
| 3, 4 phút | F2 1 query cơ bản, 2 query nâng cao, index/explain và bookmark | Đối chiếu manifest 5+5 và ID thật; không dựng benchmark |
| 4, 4 phút | Website: Manager tạo Customer/Product/Order mới ở DB thử web, confirmed→preparing; pending→assigned; Shipper giao thành công | Fauxton đọc cùng ID, total/history; Delivery/Order khớp sau hết pending; Dashboard/Shipping khớp |
| 5, 2 phút | F4 anonymous bị từ chối, Shipper/Manager vượt quyền bị chặn | Kiểm tra server GET/POST; không chỉ ẩn nút; tách role website và quyền DB |
| 6, 3 phút | F3 one-shot replication giữa 2 DB thử nhỏ không chứa app_user | Cùng ID/revision/nội dung, đếm nguồn-đích; không lấy restore logic thay replication |
| 7, 2 phút | F5 snapshot logic vào DB restore riêng | Verify count/hash/index, nhập lại không ghi đè; nêu phần không được backup |

Card đầy đủ khái niệm, nguyên lý, bước, ví dụ, mong đợi/thực tế, ảnh, ưu/nhược và phản biện của từng chức năng ở [FAUXTON_MODULES.md](p6/FAUXTON_MODULES.md). Kịch bản có thể rút gọn theo thời gian giảng viên cho phép nhưng không cắt mất việc kiểm tra nguồn/đích hoặc nói về giới hạn.

## Chuẩn bị dữ liệu để không phá bộ hiện có

Chỉ đọc qlbanhang/p2-* cho phần query. Dùng ba vùng tách biệt: `qlbanhang_test_p6_web_CODE` cho website (có tài khoản app riêng), `qlbanhang_test_p6_src_CODE`/`dst_CODE` cho replication (chỉ dữ liệu giả, không app_user), và `qlbanhang_restore_p6_CODE` cho restore logic. Tất cả DB do người có quyền tạo/bảo vệ; không tự đổi `_security` của DB chính. CODE là mã buổi demo duy nhất, không ghi đè tên/ID đã tồn tại.

Nguồn P2 có 5 cặp confirmed/assigned chưa khớp mapping P4 và 10 assigned chưa có username Shipper; không chọn tùy tiện phiếu legacy đó để diễn tập luồng. Tạo một đơn mới bằng website theo đúng các bước, dùng Shipper enabled do nhóm tự đặt mật khẩu. Không nhập lại hai Word vì bốn ID trùng khác nội dung và mẫu B thiếu SP002. Chuẩn hóa bản báo cáo theo snapshot được chụp, không đổi DB chỉ để khớp ảnh cũ.

## Nếu demo lỗi

- Credential chưa thay/quyền thiếu: dừng phần có xác thực, ghi BLOCKED, chỉ trình bày code/unit và kết quả lịch sử đã được ghi rõ.
- 409: giữ bản cũ, tải lại/đối chiếu; không dùng revision mới để overwrite tự động.
- Projection lỗi: trình bày pendingDelivery và trang Sync của Administrator; không xóa intent để làm giống thành công.
- Query rỗng: kiểm tra type/status/ID/nguồn dữ liệu, không sửa expected thành kết quả đang thấy để báo PASS.
- Replication/restore bị từ chối: ghi status và phạm vi thiếu quyền, không cấp admin hoặc tắt xác thực để cứu demo.
- Mạng NuGet: đã có bằng chứng clean-source restore/build trong môi trường hiện tại; diễn tập trước, không coi một máy khác chắc chắn có cùng package cache/SDK.

## Ảnh minh chứng sinh viên phải chụp

F1-01..06, F2 index/query/explain/bookmark, F3-01..05, F4-01..05 và F5-01..05 theo card. Mỗi ảnh cần ngày, người chụp, version/môi trường, ID/selector và chú thích đúng thao tác. Bảng W36 ở WORD_RECONCILIATION.md chỉ ra 19 vị trí ảnh cũ cần xác minh; không tự thay chú thích KH001 thành p2-customer-001. Không đưa credential, cookie, token, hash app_user hoặc thông tin cá nhân thật vào ảnh. Nếu chưa chụp, trạng thái là NOT RUN.

## Câu hỏi phản biện và trả lời có căn cứ

| Câu hỏi | Trả lời ngắn để nhóm diễn giải |
|---|---|
| “Ba công cụ” là ba GUI? | Theo đính chính của chủ đề, trình bày ít nhất ba chức năng Fauxton: CRUD/revision, Mango/index, replication; security và backup bổ sung. Không cài ba hệ chỉ để minh họa GUI. |
| Fauxton có phải database? | Không; đó là GUI cho CouchDB, website MVC là giao diện nghiệp vụ khác. [C9] |
| Vì sao không dùng PostgreSQL nếu có đơn hàng? | PostgreSQL mạnh ở FK/transaction và có JSONB; nhóm chọn CouchDB vì mục tiêu học phần Fauxton/document HTTP. Không kết luận nó tốt hơn cho mọi bán lẻ. [P1–P3] |
| MongoDB có multi-document transaction không? | Có, với điều kiện deployment/read-write concerns; không được nói mọi NoSQL thiếu transaction. [M3] |
| _rev có phải số phiên bản tăng toàn cục? | Không; token/nhánh revision cho concurrency của document, không timestamp/audit hoặc backup vĩnh viễn. [C1] |
| Giải quyết 409 bằng lấy _rev mới rồi lưu đè? | Không tự động: tải lại, đối chiếu và quyết định thay đổi có chủ đích. [C1 + service] |
| Order và Delivery luôn atomic cùng nhau? | Không. Outbox giữ intent và retry để phục hồi; có khoảng projection chậm, admin xử lý pending. [WorkflowService] |
| Source seed có hoàn toàn khớp state machine? | Không; audit P6 phát hiện 5 cặp legacy và 10 assigned thiếu username. Bằng chứng tham chiếu/tổng PASS trước đó không chứng minh mọi cặp khớp luồng P4. |
| Vì sao nhúng items nhưng không nhúng toàn bộ Customer? | Item/giá lịch sử gắn đơn và thường đọc cùng; hồ sơ khách và sản phẩm dùng chung, vòng đời độc lập. Không có FK tự động. [C1, M1] |
| Xóa khách được kiểm tra tham chiếu là đủ mọi race? | Chưa: check trước xóa không phải FK serializable khi có writer khác/ghi trực tiếp DB. [source + DATABASE_DESIGN] |
| Có use_index là chắc tối ưu? | Không; đọc explain/warning/stats và điều kiện còn filter; không có benchmark thì không nói nhanh hơn bao nhiêu. [C3] |
| Bookmark có thể dùng sau đổi selector? | Không nên; giữ selector/sort. Dữ liệu thay đổi giữa trang không tạo bảo đảm snapshot như transaction. [C3] |
| DB member có sửa security/index được không? | Không đồng nhất với database admin; server admin còn có phạm vi tạo/xóa DB/cấu hình máy chủ. [C4/C5] |
| 401/403 hôm nay nói gì về _security? | Anonymous 401 cho request đã kiểm tra; chưa đọc được security object nên chưa xác minh names/roles hay single-node. |
| Đưa mật khẩu vào biến môi trường là đã xử lý lộ Git? | Không. Phải thu hồi/thay giá trị cũ, kiểm tra lại cấu hình, visibility và phương án lịch sử. [RELEASE_READINESS] |
| Replication có thay backup không? | Không bao trùm mọi yêu cầu: tombstone/xóa có thể lan truyền. Cần mốc khôi phục và config/security riêng. [C6/C8] |
| Restore có giữ _rev như replication? | Snapshot logic của repo bỏ _rev, DB đích cấp mới; replication vận chuyển revision theo cơ chế riêng. [C6 + backup tool] |
| HTTP 200 export/restore là đủ? | Chưa; phải kiểm count/nội dung/hash/index và đọc lại đích, không chứa dữ liệu tài khoản. |
| Đã đạt HTTPS, GUI và single-node? | Chưa có đầy đủ bằng chứng. Ghi NOT RUN/BLOCKED, không lấy build hoặc API local thay thế. |
| Ảnh Word có chứng minh run hiện tại? | Không tự động; cần đối chiếu ID/selector/date/version, nguồn ảnh và đúng test. Chưa kiểm pixel ảnh cũ thì không kết luận ảnh giả hay đúng. |

Nguồn ký hiệu và URL chính thức ở p6/SOURCES.md. Câu trả lời dựa trên source thực tế hoặc giới hạn được nêu; nhóm phải diễn giải bằng hiểu biết, không học thuộc một tuyên bố “đã đạt tất cả”.
