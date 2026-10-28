# Nhật ký tiến độ Codex – QuanLyBanHang_Nosql

Ngày khởi tạo hồ sơ: 28/10/2026. **Trạng thái mới nhất: P2/P3 kỹ thuật và P4 tích hợp thật PASS; P5 đang triển khai. GUI/ảnh, single-node và bảo mật lịch sử vẫn chưa xác nhận.**

## Baseline – mã nguồn hiện tại (`master`)
- [x] Có project ASP.NET MVC5 /.NET Framework 4.7.2 và các controller CRUD cơ bản.
- [x] Đã đọc mã nguồn tĩnh các Model, Controller, Views, service CouchDB.
- [ ] Chưa xác thực build/test trong phiên kiểm tra này (người dùng báo Rebuild OK trên máy).
- [ ] Chưa có database `qlbanhang` trên Fauxton máy người dùng; Home/Index đang lỗi.
- [ ] Credential CouchDB đã bị hardcode và repo đã từng Public: chủ repo cần đổi mật khẩu và bảo vệ repo.

## Tiến độ theo giai đoạn
| Giai đoạn | Trạng thái | PR/Commit | Bằng chứng kiểm thử | Ghi chú |
|---|---|---|---|---|
| P0 – Môi trường/bảo mật | Kết nối/xác thực/khởi tạo thật PASS | Xem bản ghi tích hợp cuối tệp | Session đúng danh tính; DB/index HTTP 200; seed chạy lại giữ nguyên revision | Đổi mật khẩu cũ/Private/lịch sử: chưa xác nhận |
| P1 – Lỗi nền tảng/CRUD | Kiểm thử kỹ thuật PASS | Xem bản ghi tích hợp cuối tệp | CRUD 4 loại qua IIS; revision, validation, Shipping/Dashboard, lỗi DB/credential PASS | Chưa có ảnh/kiểm tra bố cục; các chức năng P4 chưa thuộc nghiệm thu này |
| P2 – DB/seed/schema | PASS kỹ thuật | Commit P2 sau bản ghi | 100 seed thật, idempotency, tham chiếu/tổng, Mango/_explain | Ảnh Fauxton thuộc P3 |
| P3 – Mango/Fauxton query | PASS API/tài liệu; ảnh NOT RUN | Commit P3 | 5+5 query/_explain, bookmark PASS | Chưa thao tác/chụp GUI |
| P4 – Nghiệp vụ/roles | PASS kỹ thuật/tích hợp | Commit P4 | 49 smoke + 19 unit; full IIS workflow/roles/outbox PASS | Tạo admin thật bằng mật khẩu riêng; GUI NOT RUN |
| P5 – Backup/tests | Chưa làm | — | — | — |
| P6 – Báo cáo/demo | Chưa làm | — | — | — |

## Mẫu bản ghi sau MỖI LẦN CẬP NHẬT
### YYYY-MM-DD – Giai đoạn PX – [Tên]
- **Đã làm:**
- **Files thay đổi:**
- **Liên quan rubric:**
- **Build/test:** Lệnh chạy; môi trường; thành công/thất bại/chưa chạy; số lượng test.
- **Bằng chứng:** liên kết PR/commit, log được che thông tin nhạy cảm, ảnh demo.
- **Còn thiếu / lỗi đã biết:**
- **Bước tiếp theo:**
- **Comment tiếng Việt sau push:** [link GitHub comment hoặc ghi chưa có quyền comment].

### 2026-10-28 – Giai đoạn P0 – Cấu hình kết nối an toàn
- **Đã làm:** bỏ cấu hình/xác thực cố định; dùng bốn biến môi trường; kiểm tra HTTP status trước khi trả JSON, mã hóa ID/rev, timeout 15 giây; thông báo lỗi không chứa phản hồi máy chủ. Thêm script khởi tạo DB/index/bốn tài liệu giả không ghi đè dữ liệu.
- **Files thay đổi:** Services/CouchDbService.cs, .gitignore, .env.example, README.md, scripts/Initialize-CouchDb.ps1.
- **Liên quan rubric:** 5a, nền tảng 5b, 8. Chưa đủ bộ dữ liệu P2.
- **Build/test:** đã đọc toàn bộ bốn tài liệu, kiểm tra nhánh và trạng thái Git; chưa chạy build/CouchDB ở bước P0. Controller sẽ được bổ sung xử lý exception trong P1.
- **Còn thiếu:** chủ repo đổi mật khẩu, Private repo và quyết định xử lý lịch sử; chưa xác minh single-node/quyền trên máy chủ; chưa chạy script lên CouchDB.
- **Bước tiếp theo:** P1: xử lý lỗi ở controller, validation, anti-forgery và kiểm thử.
- **Comment tiếng Việt sau push:** chưa push; sẽ chuẩn bị bình luận với SHA sau commit.

### 2026-10-28 – Giai đoạn P1 – Ổn định kết nối, Razor và CRUD
- **Khôi phục sau gián đoạn:** kiểm tra git status, nhánh, log, diff và đọc lại toàn bộ AGENTS/ROADMAP/RUBRIC/PROGRESS. Giữ nguyên thay đổi chưa commit, không đụng các tài liệu/thư mục .github chưa tracked có sẵn. Nhánh vẫn codex/nosql-completion; P0 đã commit fa57030.
- **Đã làm:** sửa orderDate ở Create/Delete và cú pháp if trong Order/Edit; bỏ parse dynamic rows, dùng kết quả typed Mango _find theo type/status với sort type/_id và bookmark 200/trang. Sáu controller hiển thị lỗi đã kiểm soát; CRUD có ModelState validation, anti-forgery ở toàn bộ 12 POST và 12 form, kiểm tra loại document, revision và lỗi trùng mã. Đơn hàng kiểm tra tham chiếu khách/sản phẩm, ngày, số lượng/giá/tổng; giao hàng kiểm tra tham chiếu đơn. Cập nhật giữ trường phụ, thay mảng đúng quy tắc và từ chối revision cũ. Chặn ID dành riêng/dot path; bổ sung cấu hình URI giữ %2F trên .NET Framework. Seed đồng nhất trạng thái đơn/giao hàng; script chạy lại không ghi đè.
- **Files thay đổi:** Controllers/{Home,Customer,Product,Order,Delivery,Shipping}Controller.cs; Models/{Customer,Product,Order,Delivery}.cs; Services/CouchDbService.cs; 12 Views CRUD và Shared/_Layout.cshtml; Web.config; scripts/Initialize-CouchDb.ps1; README.md; tests/{SmokeTests.cs,SmokeTests.config,Run-SmokeTests.ps1,Test-Initializer.py,Test-WebSmoke.py,README.md}; CODEX_PROGRESS.md.
- **Liên quan rubric:** nền tảng 5a/5b, 8, 9b/9c/9d. Không tự đánh dấu đạt rubric; thiếu demo CouchDB/Fauxton thật và các phase sau.
- **Build/test thực tế (Windows, VS 2022 Community, .NET Framework 4.7.2):**
  - MSBuild QuanLyBanHang.sln /t:Build /p:Configuration=Debug /p:MvcBuildViews=true /verbosity:minimal /nologo: exit 0, gồm biên dịch Razor. Build ban đầu phát hiện lỗi Order/Edit và Order/Delete; đã sửa và build lại đạt.
  - powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-SmokeTests.ps1: exit 0, PASS 44 assertion: 401/403/404/409/500, timeout, mất kết nối, JSON sai/thiếu docs, DB rỗng, phân trang, encoding ID/rev, loại tài liệu sai, revision cũ, bảo toàn trường phụ, validation, 12 POST anti-forgery, sáu controller thiếu cấu hình. HTTP giả lập, không phải CRUD CouchDB thật. Test ban đầu phát hiện URI giải mã %2F; đã bổ sung schemeSettings và test lại đạt.
  - python tests/Test-Initializer.py: exit 0; script khởi tạo chạy hai lần trên HTTP stub, bốn tài liệu liên kết hợp lệ/tổng đúng, index tồn tại, giữ nguyên dữ liệu đã có, 401 dừng với thông báo không lộ giá trị xác thực thử.
  - python tests/Test-WebSmoke.py qua IIS Express localhost:52515: exit 0; Home/Customer/Product/Order/Delivery/Shipping/Order-Create đều HTTP 200; bốn POST Create dữ liệu invalid hiển thị validation; POST thiếu CSRF bị từ chối HTTP 500 theo MVC5 mặc định. Không tạo/sửa/xóa dữ liệu thật. Lần thăm Home đầu tiên timeout 15 giây lúc app khởi động; chạy lại bộ web sau khởi động đạt cả bảy GET.
  - Script khởi tạo qua PowerShell parser: 0 lỗi cú pháp (P0). Lệnh chạy .ps1 ban đầu bị ExecutionPolicy chặn; chạy lại chỉ đặt Bypass cho tiến trình test, không sửa policy máy.
- **CouchDB thật đã kiểm tra chỉ đọc:** GET localhost:5984/ và /_up HTTP 200; /_session HTTP 200, phiên anonymous không có quyền _admin; /qlbanhang HTTP 404. Không có bốn biến COUCHDB_* trong Process/User/Machine. Không dùng lại credential cũ, không tự đổi mật khẩu/quyền, không khởi tạo DB thật khi chưa có cấu hình mới. Chưa chạy seed/CRUD/index trên CouchDB thật.
- **Còn thiếu:** chủ repo cần đổi mật khẩu cũ, Private repo và phối hợp làm sạch lịch sử trước khi chia sẻ. Chưa xác minh single-node/quyền bằng tài khoản mới; cần chạy checklist tests/README.md với DB thử riêng. Quy tắc trạng thái/role, ràng buộc tham chiếu khi xóa, đủ bộ seed và backup thuộc P2–P5, chưa hoàn thành. Kết quả Mango thật/chỉ mục và hiệu năng chưa kiểm chứng.
- **Bước tiếp theo:** thiết lập biến môi trường với credential mới trên máy; khởi động lại VS/IIS Express; chạy script hai lần trên DB demo riêng, kiểm thử CRUD/revision/lỗi theo tests/README.md; bổ sung ảnh Fauxton trước nghiệm thu P0/P1.
- **Kiểm tra trước commit:** git diff --check đạt; quét mẫu bí mật trên 35 file ứng viên không có phát hiện. Không in credential, không đưa bin/obj/.vs hoặc dữ liệu thật vào commit. Đây là quét mã hiện tại, không phải kiểm toán/làm sạch lịch sử Git.
- **Git/PR:** không push, không merge và không gửi bình luận PR theo yêu cầu mới nhất. Commit tiếng Việt sau diff/secret scan; hồ sơ AGENTS/ROADMAP/RUBRIC và .github có sẵn vẫn được giữ nguyên, chưa đưa vào commit này.

### 2026-10-28 – Kiểm tra điều kiện tích hợp P0/P1 và hướng dẫn thiết lập Windows
- **Nhánh/đầu phiên:** codex/nosql-completion, hai commit fa57030 và 27d427f; không có thay đổi tracked chưa commit. Giữ nguyên .github/ và tài liệu có sẵn chưa tracked. RUBRIC_CHECKLIST.md được cập nhật/đưa vào commit theo yêu cầu cụ thể của chủ repo lần này.
- **Đã làm:** đọc lại AGENTS.md, ROADMAP_NOSQL.md, CODEX_PROGRESS.md, RUBRIC_CHECKLIST.md, tests/README.md và script khởi tạo. Thêm chẩn đoán chỉ đọc scripts/Test-CouchDbConnection.ps1: cờ có/không của biến ở ba scope, health local, DB/index có xác thực khi đủ cấu hình, mã thoát 0/1/2; không in giá trị biến, raw response/error hoặc Authorization. Bổ sung hướng dẫn Environment Variables trên Windows, cách nhận biến mới, tạo DB/index/seed không ghi đè và bảng kết quả PASS/FAIL/NOT RUN.
- **Files thay đổi:** scripts/Test-CouchDbConnection.ps1, tests/Test-ConnectionDiagnostic.py, tests/README.md, CODEX_PROGRESS.md, RUBRIC_CHECKLIST.md. Không sửa code ứng dụng trong bước này vì chưa phát hiện lỗi ứng dụng mới qua các ca đã chạy.
- **Liên quan rubric:** 5a/5b/5c/5d, 8, 9b/9c/9d: chỉ ghi bằng chứng từng phần, chưa nghiệm thu tích hợp đầy đủ.

| Nhóm | Ca kiểm tra | Kết quả thực tế |
|---|---|---|
| Build thật | MSBuild VS 2022 Community, Debug, MvcBuildViews=true | PASS, exit 0, gồm Razor |
| Giả lập | Run-SmokeTests.ps1 | PASS, exit 0, 44 assertion; không phải CouchDB thật |
| Giả lập | Test-Initializer.py | PASS, exit 0; hai lần seed, index/bốn doc liên kết, giữ dữ liệu cũ, lỗi 401 |
| Giả lập | Test-ConnectionDiagnostic.py | PASS, exit 0; thiếu cấu hình exit 2; HTTP 200/index đúng exit 0; 401 exit 1; không in giá trị xác thực giả |
| IIS Express thật | Test-WebSmoke.py tại localhost:52515 | PASS, exit 0; bảy GET HTTP 200, bốn POST invalid có validation, POST thiếu token bị chặn HTTP 500 theo MVC5 |
| CouchDB thật, chỉ đọc | GET /, /_up, /_session tại 127.0.0.1:5984 | PASS, HTTP 200; phiên anonymous không có _admin |
| Điều kiện DB sẵn sàng | GET /qlbanhang chưa xác thực | FAIL, HTTP 404; chưa có bằng chứng DB dùng được bằng tài khoản ứng dụng |
| Cấu hình Windows | COUCHDB_URL/DATABASE/USERNAME/PASSWORD tại Process/User/Machine | FAIL điều kiện sẵn sàng: cả 12 cờ configured=False; không in giá trị |
| Chẩn đoán môi trường thật | Test-CouchDbConnection.ps1 | PASS health; NOT RUN kiểm tra DB/index có xác thực, exit 2 vì thiếu biến |
| Tích hợp CouchDB thật | Tạo DB/index/seed, chạy seed lần hai, Mango query | NOT RUN: thiếu thông tin xác thực mới; không thử ghi bằng anonymous |
| Tích hợp CouchDB thật | CRUD Customer, Product, Order, Delivery; revision/xung đột/tham chiếu | NOT RUN: thiếu thông tin xác thực mới và DB chưa sẵn sàng |
| Tích hợp Shipping | Lọc Delivery shipping và đối chiếu dữ liệu CouchDB | NOT RUN; Shipping không có CRUD riêng |
| Giao diện trực quan | Kiểm tra bằng thao tác trình duyệt/ảnh màn hình | NOT RUN; đã kiểm thử HTML/form HTTP qua IIS, chưa đánh giá bố cục trực quan |

- **Còn thiếu / lý do chưa hoàn thành P0/P1:** chưa có cấu hình mới ở Process/User/Machine kể cả kiểm tra lại cuối bước. Đã yêu cầu chủ repo thiết lập riêng trên Windows, không gửi tài khoản/mật khẩu vào chat. Chưa xác minh quyền tạo DB/index, quyền đọc/ghi ứng dụng và single-node bằng tài khoản mới. Không suy đoán hay tái sử dụng credential cũ; không đổi mật khẩu/quyền, không xóa DB hoặc dữ liệu có sẵn.
- **Bước chủ repo cần làm:** theo tests/README.md, tạo bốn User variables bằng giao diện Windows, đóng/mở lại VS/IIS/terminal/Codex; chạy chẩn đoán, khởi tạo hai lần bằng tài khoản đủ quyền nếu DB chưa tồn tại, sau đó chuyển sang tài khoản ứng dụng phù hợp. Chỉ thông báo đã thiết lập, không gửi giá trị. Chạy checklist CRUD trên dữ liệu giả, ghi bằng chứng thực tế trước nghiệm thu.
- **An toàn:** vẫn cần chủ repo đổi mật khẩu đã lộ, Private repo và phối hợp xử lý lịch sử trước khi chia sẻ. Không bắt đầu P2; kết luận hiện tại là thiếu điều kiện kiểm thử tích hợp, không phải PASS CRUD thật.
- **Git/PR:** tạo commit tiếng Việt trên nhánh hiện tại sau diff/secret scan. Không push/merge hoặc gửi bình luận PR theo yêu cầu phiên này.
- **Kiểm tra trước commit:** git diff --check PASS; quét mẫu bí mật trên cả 5 file thay đổi không có phát hiện; PowerShell parser báo 0 lỗi cho script chẩn đoán. Không có bin/obj/.vs hoặc dữ liệu thật trong danh sách commit; không kiểm toán hay viết lại lịch sử Git.

### 2026-10-28 – Tích hợp P0/P1 với CouchDB thật và IIS Express
- **Nhánh/đầu phiên:** `codex/nosql-completion`, HEAD `31c4f65`; không có thay đổi tracked chưa commit. Giữ nguyên `.github/`, AGENTS.md, ROADMAP_NOSQL.md và các tài liệu chưa tracked có sẵn; không đưa vào commit này.
- **Nguyên nhân cấu hình chưa được nhận:** sandbox dùng tài khoản Windows riêng, không thấy User variables của chủ máy. Kiểm tra ngoài sandbox xác nhận đủ bốn biến User, nhưng Process cũ chưa nhận biến. Đã nạp User → Process riêng cho lệnh kiểm thử/IIS, không sửa biến Windows và không hiển thị giá trị xác thực. Đây là nguyên nhân môi trường, không cần sửa service ứng dụng.
- **Đã làm:** dùng script hiện có tạo `qlbanhang`, index `type-id` và bốn tài liệu `demo-*`; chạy hai lần thành công. Bộ tích hợp tiếp tục chạy lại initializer, so sánh toàn bộ nội dung và `_rev` trước/sau, giữ nguyên bốn tài liệu đã có. Không xóa DB hoặc ghi đè dữ liệu hiện có.
- **Files thay đổi:** `tests/Test-LiveIntegration.py`, `tests/Run-LiveIntegration.ps1`, `tests/Test-WebSmoke.py` (cho phép chọn URL qua biến test), `tests/README.md`, `CODEX_PROGRESS.md`, `RUBRIC_CHECKLIST.md`. Không đổi stack hoặc code nghiệp vụ; không phát hiện lỗi ứng dụng mới qua các ca đã chạy.

| Ca kiểm thử thực tế | Kết quả | Bằng chứng/phạm vi |
|---|---|---|
| Cấu hình User; kết nối/xác thực | PASS | Đủ 4 biến User; health/DB/index HTTP 200; `_session.userCtx.name` khớp tài khoản cấu hình, không in tên/mật khẩu |
| Khởi tạo DB/index/seed | PASS | Initialize-CouchDb.ps1 chạy hai lần; bộ tích hợp chạy lại và đối chiếu 4 nội dung/revision không đổi |
| Build ứng dụng + Razor | PASS | MSBuild VS 2022 Community, Debug, MvcBuildViews=true; exit 0 |
| Smoke giả lập | PASS | Run-SmokeTests.ps1: 44 assertion; Test-Initializer.py và Test-ConnectionDiagnostic.py exit 0 |
| CRUD Customer/Product/Order/Delivery | PASS | POST form qua IIS Express localhost:52516 có cookie/token; đọc trực tiếp CouchDB đối chiếu từng create/edit/delete; tổng đơn 24000 → 36000 do server tính |
| Validation/tham chiếu/mã trùng | PASS | Email sai, giá âm, khách/đơn tham chiếu thiếu và mã trùng bị từ chối; không sinh tài liệu invalid |
| Revision/bảo toàn dữ liệu | PASS | Sửa/xóa với revision cũ bị từ chối ở cả 4 loại; revision hiện tại không đổi khi bị từ chối; cập nhật giữ trường JSON phụ |
| Shipping/Dashboard thật | PASS | So sánh 5 số đếm Dashboard với Mango; số lượng và ID Shipping khớp; Delivery shipping xuất hiện, đổi delivered bị loại khỏi Shipping; đối chiếu trước/trong/sau CRUD |
| Mango cơ bản | PASS | Selector type + ID sản phẩm trả đúng tài liệu/giá |
| Mango nâng cao | PASS | `$and`, `$in`, `$gte`, `$elemMatch` tìm đúng đơn theo khách/tổng/trạng thái/số lượng |
| Mango phân trang/sort/index | PASS | type + _id tăng dần, use_index idx-type-id, limit=1 và bookmark; kết quả khớp truy vấn đầy đủ, không trùng/thiếu |
| IIS GET với DB thật | PASS | Home, Customer, Product, Order, Delivery, Shipping, Order/Create HTTP 200, không cảnh báo lỗi DB |
| DB không tồn tại/mật khẩu sai | PASS | Hai tiến trình IIS cấu hình lỗi riêng; mỗi lượt 7 GET, 4 POST invalid, CSRF đều đạt; không tạo DB thiếu, không sửa biến Windows |
| Anti-forgery | PASS | POST thiếu token bị chặn HTTP 500 theo MVC5 mặc định, gồm cả môi trường DB thật |
| Xác minh node qua API quản trị | FAIL quyền truy cập | GET `/_membership` có xác thực trả HTTP 403; tài khoản cấu hình không truy cập được API này. Chưa xác minh single-node; không tự nâng quyền hoặc sửa cấu hình máy chủ |
| 403 với tài khoản bị hạn chế; timeout/mất dịch vụ thật | NOT RUN | 403/timeout/mất kết nối đã có test giả lập; không sửa quyền hoặc dừng CouchDB đang chạy |
| Fauxton/kiểm tra bố cục/ảnh minh chứng | NOT RUN | Phiên này kiểm tra HTTP/HTML/form IIS; chưa thực hiện GUI hoặc chụp ảnh |
| Đổi mật khẩu cũ/Private/làm sạch lịch sử | NOT RUN xác minh | Thuộc thao tác chủ repo; không suy ra đã hoàn tất từ việc thiết lập biến |

- **Lệnh tích hợp:** `powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-LiveIntegration.ps1`, exit 0. Đã chạy bộ CRUD thật hai lượt sau khi bổ sung validation và ca cấu hình lỗi; kết quả cuối PASS. IIS chỉ chạy trong kiểm thử và đã dừng tiến trình do test tạo.
- **An toàn dữ liệu:** dùng ID `test-p01-<UUID>-*`, đã xóa cả bốn tài liệu thử qua app theo thứ tự Delivery → Order → Product → Customer và xác nhận GET 404; CouchDB giữ tombstone theo cơ chế thông thường. Không xóa dữ liệu khác/DB. Seed vẫn bốn tài liệu, chưa phải bộ P2.
- **Liên quan rubric:** bằng chứng kỹ thuật từng phần cho 5a–5d, 8, 9b–9d; chưa đánh dấu hoàn thành toàn bộ rubric (còn GUI, bộ dữ liệu/truy vấn đầy đủ, quyền, backup và các phase sau).
- **Kết luận:** các ca kỹ thuật P0/P1 được yêu cầu trong phiên PASS; lỗi môi trường nhận biến đã xử lý. Không bắt đầu P2 trong phiên này. Chưa nghiệm thu phần bảo mật ngoài repo khi chưa có xác nhận từ chủ repo.
- **Bước tiếp theo:** chủ repo xác nhận thay mật khẩu đã lộ/Private/phương án lịch sử; bổ sung demo Fauxton và các ca NOT RUN thích hợp. Sau đó triển khai nhóm công việc P2 theo roadmap.
- **Git/PR:** tạo commit tiếng Việt trên nhánh hiện tại; không push, không merge, không gửi bình luận PR.
- **Kiểm tra trước commit:** diff --check PASS; PowerShell parser 0 lỗi; cú pháp Python PASS. Secret scan 6 file (mẫu token/private key/URL chứa credential và đối chiếu kín mật khẩu đang cấu hình) không có phát hiện. Không đưa bin/obj/.vs, dữ liệu thật hoặc tài liệu chưa tracked có sẵn vào commit. Không quét/làm sạch lịch sử Git.

### 2026-10-28 – P2: mô hình và 100 tài liệu demo
- **Đã làm/files:** docs/DATABASE_DESIGN.md giải thích nhúng/tham chiếu và revision; scripts/couch.py client lỗi có kiểm soát; Seed-Demo.py tạo 20 khách/30 sản phẩm/30 đơn/20 giao hàng, 9 JSON index; With-CouchEnvironment.ps1 nạp riêng biến User khi Process thiếu; tests/Test-P2.py và công cụ Review-Staged.py kiểm tra bí mật/artifact trước commit.
- **Test thật:** Test-P2.py PASS: lần đầu thêm 100 doc, lần hai thêm 0; toàn bộ nội dung và revision giữ nguyên; tham chiếu đúng loại, mỗi đơn ít nhất 2 item, tổng đúng; query sort và _explain chọn type-date PASS. Không sửa/xóa tài liệu có sẵn. Chưa thay code ứng dụng.
- **Rubric:** 5a/5b/5c/5d/8 có thêm bằng chứng kỹ thuật; ảnh GUI NOT RUN. Giữ nguyên các file chưa tracked ban đầu. Không push/merge, chưa xác nhận an toàn lịch sử; single-node vẫn BLOCKED quyền API quản trị 403.
- **Bước tiếp:** P3 bộ 5+5 truy vấn, hướng dẫn Fauxton/so sánh; sau đó P4/P5 theo yêu cầu liên tục.

### 2026-10-28 – P3: Fauxton và truy vấn thực tế
- **Files/đã làm:** docs/FAUXTON_DEMO.md gồm 10 thao tác, phân biệt GUI/API/backup; so sánh Compass khác hệ DB, nguồn chính thức; docs/SCREENSHOT_CHECKLIST.md ghi toàn bộ ảnh NOT RUN; docs/mango-queries.json chứa 5 cơ bản + 5 nâng cao có số lượng seed mong đợi; tests/Test-Mango.py kiểm tra kết quả và index thực tế. couch.py chỉ bổ sung mã lỗi chữ cái an toàn, không in reason/body.
- **Kiểm thử:** Test-Mango.py PASS trên CouchDB thật, counts B1..B5 = 20/10/2/5/5; A1..A5 = 11/11/4/1/6; _explain chọn đúng index ở cả 10 query; bookmark limit 7 khớp tập đầy đủ, đúng thứ tự/không trùng. Hồi quy IIS P0/P1 sau P2 PASS (7 GET, CRUD/revision/validation/CSRF, lỗi DB/credential).
- **Lỗi đã xử lý:** _explain ban đầu chọn index dự phòng ở query không có sort đủ trường; bổ sung sort phù hợp và chạy lại cả 10 PASS. Một request 403 tạm thời sau bộ thử mật khẩu sai; lần chạy tiếp xác thực lại hoạt động, không thay quyền/credential hoặc vô hiệu cơ chế bảo vệ. Không khẳng định nguyên nhân máy chủ khi chưa có log quản trị.
- **Rubric:** 1–4/5c/5d/7a/7b thêm tài liệu và API test; thao tác GUI/ảnh vẫn NOT RUN, không dùng API làm bằng chứng đã chạy GUI. Không đo hiệu năng. Single-node/lịch sử Git vẫn chưa xác nhận.
- **Git:** commit P3 cục bộ tiếng Việt sau diff/secret scan; không push/merge. Chuyển P4.

### 2026-10-28 – P4: phân quyền, nghiệp vụ và phục hồi đồng bộ
- **Đã làm/files:** Services/AppSecurity.cs, AccountController + Views/Account: Forms Authentication, PBKDF2-HMAC-SHA256 600000 vòng/salt ngẫu nhiên, 3 vai trò, admin tạo/khóa tài khoản, bộ đếm login lỗi trong tiến trình. Global filter và kiểm tra quyền POST/URL; Web.config cookie HttpOnly/SameSite, timeout 30 phút, Release transform yêu cầu HTTPS. scripts/Create-AppUser.py tạo-only, nhập mật khẩu riêng, không có tài khoản/mật khẩu mặc định.
- **Nghiệp vụ:** WorkflowService.cs + Order/Delivery controllers kiểm tra transitions, tham chiếu/giá/số lượng/tổng; Order history/deliveryId/pendingDelivery durable outbox, CAS revision, retry idempotent, khóa đơn đang chờ, không overwrite projection có revision khác. SyncController/Views cho admin. Chặn xóa document đang được tham chiếu; chỉ xóa phiếu pending. Legacy chỉ nhận vào quy trình khi duy nhất một phiếu và trạng thái khớp.
- **Truy vấn/UI:** QueryService + FindPage/HasReference trong CouchDbService; bộ lọc Mango và bookmark 20/trang trên 4 danh sách; Shipper chỉ thấy/sửa phiếu được giao, Shipping 100/trang. Views trạng thái và lịch sử, menu đăng xuất; docs/USER_GUIDE.md và DATABASE_DESIGN.md giải thích nhất quán cuối cùng/giới hạn.
- **Kiểm thử thực tế PASS:** build ứng dụng + Razor VS 2022/.NET Framework 4.7.2; Run-SmokeTests.ps1 49 assertion (mở rộng CSRF Account/Sync, không bỏ ca cũ); Run-WorkflowUnit.ps1 19 assertion (hash/transition và lỗi ghi projection/ack, retry, xung đột). Run-LiveIntegration.ps1 đã chuyển sang successor có xác thực Test-Workflow.py qua Run-SystemTests.ps1; DB riêng `qlbanhang_test_7cfbcf0b10f840d8a7e4cb3bc201b023`, exit 0.
- **Tích hợp PASS:** login 3 vai trò; anonymous bị chuyển Login, Shipper/Manager bị 403 khi vượt quyền; admin tạo/khóa tài khoản, cookie tài khoản khóa bị từ chối ngay request sau. CRUD/validation/mã trùng; tổng server; chặn xóa tham chiếu; revision cũ và trường phụ; processing→confirmed→preparing→assigned→shipping→failed→assigned→shipping→delivered; Order khớp Delivery, Shipping lọc theo nhân viên. Outbox phục hồi crash giả lập trên DB thật, lock, retry không trùng, xung đột giữ nguyên revision ngoài. Phân trang/lọc, CSRF và logout PASS.
- **Lỗi đã sửa:** Razor thiếu @foreach gây build FAIL, sửa và build lại PASS. Assertion nhãn trang tiếp theo FAIL do Razor mới không BOM bị đọc UTF-8 thành ký tự sai; thêm BOM/encoding rõ ràng và giữ assertion tiếng Việt, chạy lại toàn bộ PASS. Bộ test cũ anonymous được nâng cấp theo chính sách auth mới, các ca CRUD/revision/bảo toàn dữ liệu được giữ trong successor.
- **Dữ liệu/an toàn:** chỉ ghi/test trên DB thử riêng, giữ DB để đối chiếu; không sửa credential/quyền CouchDB hoặc tài liệu qlbanhang. Mật khẩu test ngẫu nhiên chỉ ở bộ nhớ, app_user chỉ có hash và không hiển thị trong View. Chưa tạo admin cố định trong DB chính; sinh viên tự nhập bí mật riêng bằng tool. Không tự cấp quyền admin để gọi membership.
- **Giới hạn:** không có transaction hai document, Shipping có thể tạm chậm khi projection lỗi; thao tác trực tiếp DB/multiple writers có thể vượt ràng buộc tham chiếu, đã ghi trong hướng dẫn. Không coi kiểm tra trước xóa là foreign key serializable. GUI và kiểm tra HTTPS deployment NOT RUN; MFA/reset mật khẩu tự phục vụ ngoài phạm vi demo. Rubric 9a–9d có thêm test; chưa tự chấm điểm.
- **Git:** commit P4 tiếng Việt sau diff/secret scan; không push/merge. Bước tiếp P5, hoàn tất snapshot/import/restore và tài liệu bàn giao.
