# Đối chiếu hai Word với source P0–P5 và bằng chứng hiện có

## Phạm vi và cách định vị

A = `docs/review_input/Nhóm 7_BuiVanQuanTri.docx`, SHA256 `40a8edef26ddfcee6d0c3491b215e6684ccf1ef59d18f4617a7738d4eeb0a990`.

B = `docs/review_input/Tài liệu.docx`, SHA256 `e69b6ccccc84c60b7a336330f20347bfaf4b905b45a277d50e9505e3f2bf0658`.

A: 569 paragraph ở body, 48 bảng, 18 media resource/19 vị trí đặt ảnh, không tracked insertion/deletion. B: 56 paragraph, 4 JSON mẫu, không bảng/ảnh. B không phải rubric hay bản nhận xét giảng viên. Hai file gốc được đọc, không sửa, không đưa vào commit. Bản trích che thông tin liên hệ/bí mật chỉ ở artifacts/p6-input.

`A:P0068` là paragraph thứ 68 trong body; `A:T8R1C1` là bảng 8, hàng 1, ô 1. Các mã do scripts/Inspect-ReviewInputs.py tạo, không phải số trang hoặc bookmark có sẵn trong Word. Dùng kèm tiêu đề mục và cụm từ trích ngắn để tìm chính xác. Không ghi số trang khi chưa render/phân trang lại. Metadata A lưu Pages=50, Words=9739 chỉ là cache Word, không chứng minh bản sau biên tập vẫn 50 trang.

Source đối chiếu: mốc 1d88092 (P5 703b70c là ancestor), Models/Controllers/Services, Seed-Demo.py, docs/mango-queries.json và TEST_RESULTS.md. Audit P6 mới xác nhận server 3.5.2 qua GET / không auth; database/_security/membership đều 401 cho anonymous. Credential vẫn trùng lịch sử nên không xác thực lại. Vì thế không khẳng định KH001/SP001/DH001/GH001 tồn tại hoặc khác nội dung trong DB đang chạy. Các so sánh DB bên dưới tách rõ **nguồn seed**, **bằng chứng P5 trước đây** và **DB hiện tại BLOCKED**.

## Bảng sai lệch và đề xuất xử lý

| Mã | Vị trí gốc | Phát hiện có căn cứ | Thay/thêm/bỏ đề nghị |
|---|---|---|---|
| W01 | Mở đầu §3, P0068 | Nói không có phân quyền nhiều vai trò; AppSecurity/AccountController đã có 3 vai trò | Thay bằng phạm vi app nội bộ và giới hạn chưa tích hợp thanh toán/vận chuyển |
| W02 | P0069; §2.8 P0259; T18; §3.7 P0336/T25; T33R6 | 20 tài liệu/5 mỗi loại khác nguồn seed 20/30/30/20; P5 snapshot từng có 104 nghiệp vụ | Thay số liệu theo nguồn canonical, ghi ngày/scope; không dùng doc_count gồm design docs làm số nghiệp vụ |
| W03 | §1.7 P0166 | Nói chưa có dashboard; HomeController/Views/Home đã có 5 số đếm | Thay bằng Dashboard đã kiểm tra trước P6, mô tả vẫn chưa có BI/benchmark lớn |
| W04 | §1.6.1 T5; P0143 | Nhân viên kho/khách hàng là tác nhân khái niệm, không phải role website đã triển khai | Ghi role thật Administrator/OrderManager/Shipper; nhân viên tạo đơn cho Customer, chưa có customer self-service |
| W05 | P0145; T13; T19R4; P0374; T29 | `shipped` không thuộc enum Order hiện tại | Thống nhất `shipping`; không tự update document thật chỉ để đổi thuật ngữ |
| W06 | T15/T24/T40; T16; T46 | Delivery `preparing`/`in_transit` không thuộc enum hiện tại | Dùng pending/assigned/shipping/delivered/failed/cancelled, preparing thuộc Order |
| W07 | §2.1.2 P0177; T6; §2.3 P0194 và Phụ lục A | Prefix KH/SP/DH/GH không phải canonical seed P2 | Dùng p2-customer/product/order/delivery-xxx; giữ legacy sample chỉ nếu gắn nhãn ví dụ riêng |
| W08 | T7R7 | Address được đánh “Có/bắt buộc” nhưng Customer model không Required cho address | Mô tả đúng validator hiện có; đưa yêu cầu địa chỉ giao thật vào phần cải tiến |
| W09 | T9R5/R8 | Category/status được mô tả bắt buộc tuyệt đối; Product model không Required ở hai trường này | Phân biệt schema mong muốn với việc backend thực sự reject; không tự nhận có constraint chưa có |
| W10 | T11 | Thiếu history, deliveryId, pendingDelivery và cơ chế tổng server | Thêm trường P4 và MoneyAttribute; total do server tính, không tin tổng client |
| W11 | T14R9/R10/R11 | Mô tả ngày/địa chỉ luôn bắt buộc; code cho phép ngày rỗng, chỉ validate ngày nếu có | Nêu giới hạn hiện hành; shipperId phải là username Shipper enabled khi assigned/shipping/delivered/failed |
| W12 | P0236/P0452 | Tổng 1.150.000 trong ví dụ tính đúng, nhưng nói chương trình “nên tính” như việc tương lai | Giữ phép tính minh họa; thêm WorkflowService đã tính/check tổng, miền tiền tối đa 999999999999.99 và 2 số lẻ |
| W13 | P0240/P0253/T17 | State machine và kiểm tra tham chiếu chỉ được mô tả là đề xuất | Bổ sung rule đã triển khai, lỗi 409, guard xóa tham chiếu và giới hạn cạnh tranh ngoài app |
| W14 | Chương 3 tiêu đề P0278; T2; §3.1 | Thiếu ASP.NET MVC5, .NET Framework 4.7.2, Newtonsoft, IIS Express, NuGet | Giữ số chương, bổ sung triển khai website bên cạnh Fauxton |
| W15 | §3.3–3.7, T21–T25 | Hướng dẫn nhập lại ID cố định có thể xung đột/ghi đè ý định demo | Dùng seed create-only; CRUD trực tiếp chỉ với ID mới của buổi demo hoặc DB thử riêng |
| W16 | §3.9 P0350 | Dễ hiểu thành sửa Order/Delivery riêng là đủ đồng bộ | Thêm outbox, Order là aggregate authority; không sửa trực tiếp trạng thái production trong Fauxton |
| W17 | §3.10 P0359 | Tombstone được giải thích hợp lý nhưng chưa phân biệt _rev với backup lịch sử | Giữ nội dung đúng, thêm compaction/replication không bảo đảm lưu mọi nội dung revision cũ |
| W18 | §3.11; Phụ lục B | Bộ query cũ không phải bộ 5 cơ bản + 5 nâng cao đã test; B6 dùng sai trạng thái | Thay bằng mango-queries.json, giữ selector/sort/bookmark ổn định và expectedSeedCount |
| W19 | §3.12 T30 | idx_type_status trên type/status khác index thật type-status-id và ddoc idx-type-status-id có _id | Thay tên/trường theo Seed-Demo.py; bổ sung _explain thay vì chỉ nói query nhanh |
| W20 | §3.13 P0398/T31 | Nói nên viết kiểm tra backend khi lớn hơn; app đã kiểm tra tham chiếu/tổng | Thay bằng implementation hiện có và test, giữ cảnh báo CouchDB không có FK |
| W21 | §3.14 T32R3 | Gợi ý 401 phải dùng admin; có thể dẫn cấp quyền quá mức | Xác thực đúng tài khoản được cấp quyền, phân biệt 403, không mặc định dùng server admin |
| W22 | §3.14 T32 | Thiếu 403, timeout/mất kết nối và lỗi sync pending | Thêm ca tương ứng theo P0/P4 và ma trận quyền DB/website riêng |
| W23 | T33R3 | Ô CouchDB ghi “Fauxton v.3.5.2”, nhầm tên thành phần | Ghi CouchDB server 3.5.2 đã quan sát; version/build Fauxton lấy UI riêng nếu cần |
| W24 | T33R7 | Ngày 23/10/2026 khác test P0–P5 ngày 28/10/2026 | Không sửa ngày ảnh cũ thành ngày mới; tách phiên test/ảnh và ghi lại ngày demo thật |
| W25 | T34R2–R16 và T35R2–R5 | Đánh “Đạt” hàng loạt nhưng không gắn từng run/ID/ảnh; P0422/P0475 lại nói kết quả cần bổ sung | Thay bằng ma trận bằng chứng ở dưới, không chuyển API PASS thành GUI PASS |
| W26 | T35R6/R7 | “Đáp ứng yêu cầu cơ bản/nhanh” là quan sát chưa có số đo, dữ liệu cũ nhỏ | Bỏ hàm ý benchmark; chỉ ghi latency nếu có phương pháp/dữ liệu/log thật |
| W27 | §4.8 P0464/P0465; §5.3 P0490 | Nói chưa có giao diện/backend kiểm tra/quyền | Đã có MVC controllers và views; vẫn chưa có REST API public riêng, không gọi MVC là ASP.NET Web API |
| W28 | §5.4.1–5.4.2 P0493/P0495 | Đề xuất xây backend/UI như chưa có, kể cả đổi sang Java/Node | Thay bằng củng cố MVC5 hiện có, kiểm thử triển khai và hoàn thiện chức năng còn thiếu |
| W29 | §5.4.4 P0499; §5.4.5 P0501 | History/quyền/backup có phần đã làm, phần khác chưa làm | Tách status history/outbox và backup logic đã có khỏi notifications, stock reservation, full backup chưa có |
| W30 | P0507/P0509/P0510 | Nguồn chỉ dẫn tới trang chủ docs, khó kiểm chứng luận điểm | Thay link trang cụ thể theo SOURCES.md và gắn số ngay sau luận điểm |
| W31 | P0512 | Tài liệu “NoSQL Databases: An Overview” ghi cần đối chiếu, chưa định danh ấn phẩm | Không giữ như trích dẫn đã xác minh; bỏ hoặc bổ sung bản gốc/chỉ mục xuất bản thật |
| W32 | P0511/P0513/P0514 | Chưa có bản sách/bài giảng để xác minh trang; bài giảng lặp/mơ hồ | Không dựng số trang; nhóm cung cấp tài liệu giảng viên và chuẩn hóa bibliography |
| W33 | T1R2–R4 | 100% mỗi người không rõ là hoàn thành phần việc hay tỷ trọng đóng góp | Chú giải cách tính; nếu tỷ trọng toàn nhóm phải tổng 100%, không tự điều chỉnh đóng góp của sinh viên |
| W34 | A body/style | Normal TNR 14pt; nhiều run override 13pt; 299 run dùng Consolas; line=276 phổ biến hơn 360 | Chuẩn hóa TNR13/1.5 ở style và direct-format, giữ bảng dễ đọc; chưa kết luận layout đạt |
| W35 | Phần trống P0219–P0232, P0420–P0442, P0519–P0527 | Nhiều paragraph trắng có thể tạo khoảng trống/dồn trang | Dọn có kiểm soát thay vì ép thêm nội dung vượt 60 trang; kiểm tra sau cập nhật TOC |
| W36 | Toàn bộ ảnh A:P0282–P0381 | Có ảnh nhúng thật nhưng chưa có provenance/ngày/cross-reference từng test rõ ràng | Không gọi là ảnh giả; kiểm tra lại pixel/nguồn trước khi coi là bằng chứng phiên hiện tại |
| W37 | P2 seed nguồn so với P4 | 5 cặp confirmed/assigned khác mapping P4 chuẩn bị; 10 assigned có shipperId rỗng | Giữ dữ liệu cũ, ghi sai lệch rõ; chuẩn bị demo riêng theo luồng website sau khi có credential mới |

Nội dung đúng cần giữ: phân biệt embed/reference P0101–P0102/P0173, không có FK tự động P0192, giá lịch sử P0217, phép tính tổng P0236, tombstone P0359, cảnh báo không suy benchmark P0096. Không viết lại toàn bộ những phần này.

## JSON trùng ID giữa A và B

scripts/Compare-ReviewJson.py đọc raw JSON trong bộ nhớ, xuất ID/vị trí/tên field đổi, không xuất dữ liệu liên hệ. Có 12 lần xuất hiện JSON document ở A, 4 ở B. A có 5 ID riêng; B có 4. Bốn ID chung có hai biến thể khác nhau; việc cùng ID xuất hiện nhiều lần trong A là lặp ví dụ, không tự chứng minh DB có duplicate.

| ID | Các vị trí A | Vị trí B | Trường khác |
|---|---|---|---|
| KH001 | T8/T21/T37, R1C1 | P0001–P0008 | address, email, phone |
| SP001 | T38R1C1 | P0010–P0018 | name; giá/quantity cùng nhau không làm hai bản thành cùng nội dung |
| DH001 | T12/T23/T39, R1C1 | P0021–P0042 | orderDate, address |
| GH001 | T15/T24/T40, R1C1 | P0044–P0054 | shipperId, shipperName, shipperPhone, status, pickupDate, expectedDate, address |

SP002 có ở A:T10/T22 nhưng thiếu JSON tương ứng trong B; B:DH001 vẫn tham chiếu SP002. Không nhập cả hai phiên bản cùng _id như hai tài liệu độc lập: CouchDB chỉ có một ID trong DB; PUT thiếu/current _rev sai có thể 409 hoặc update có chủ đích. Không xử lý bằng ghi đè. Không ID KH/SP/DH/GH của hai Word nằm trong bộ canonical p2-*; chưa đọc được DB live nên không kết luận các ID legacy vắng mặt ở DB.

Trạng thái: A:DH001 processing đi cùng GH001 preparing (delivery không hợp enum). B:DH001 processing đi cùng GH001 pending; pending là enum hợp lệ nhưng website hiện chỉ tạo Delivery khi Order preparing. Hai ví dụ cần được chuyển thành một chuỗi thời điểm rõ ràng hoặc thay bằng snapshot nhất quán, không ghép ảnh trước/sau khác ngày thành một thời điểm.

## Năm cặp seed nguồn cần xử lý cho demo

`P6-SeedConsistency.py` trả **exit 1/FAIL** đối chiếu với WorkflowService.cs:107; không sửa seed/DB để làm xanh. Đây là phát hiện trên source, không phải kết quả đọc DB hôm nay.

| Order | Delivery | Nguồn hiện tại | Rule P4 sau cập nhật phiếu |
|---|---|---|---|
| p2-order-002 | p2-delivery-001 | confirmed / assigned | preparing / assigned |
| p2-order-008 | p2-delivery-005 | confirmed / assigned | preparing / assigned |
| p2-order-014 | p2-delivery-009 | confirmed / assigned | preparing / assigned |
| p2-order-020 | p2-delivery-013 | confirmed / assigned | preparing / assigned |
| p2-order-026 | p2-delivery-017 | confirmed / assigned | preparing / assigned |

P2 test cũ kiểm tra cấu trúc/tham chiếu/tổng nhưng không khẳng định mọi cặp seed là một workflow P4 hoàn tất. Backup validator chỉ áp dụng mapping canonical khi Order đã có deliveryId; dữ liệu legacy không có trường này nên không bị bắt bởi phép kiểm ấy. Vì thế PASS trước đây không loại trừ phát hiện mới. Demo mới dùng các record mới trong DB thử và tài khoản Shipper enabled, hoặc quy trình chuẩn hóa được chủ repo cho phép; không auto đổi p2-* trên qlbanhang.

## Rà soát bảng “Đạt” ở §4.4

| Ca trong Word | Kết luận hiện có | Cách ghi thay thế |
|---|---|---|
| TC01 tạo qlbanhang | API tạo DB có bằng chứng P0/P5 trước; GUI phiên mới chưa xác minh | PASS lịch sử API; NOT RUN GUI mới |
| TC02–TC05 KH001/SP002/DH001/GH001 | Không có run được nối đúng ID và nội dung Word hiện tại | NOT RUN đối với trường hợp cụ thể; DB live BLOCKED |
| TC06 có 20 tài liệu | Khác nguồn P2=100 và snapshot nghiệp vụ P5=104 | Thay expected theo scope; live chưa đếm lại |
| TC07–TC08 query processing/active | Mango tổng quát đã test nhưng không chứng minh đúng selector/ID/ảnh Word | Không sao chép PASS chung; chạy lại GUI và ghi ID thật |
| TC09 đổi trạng thái đơn | Nếu đổi thành shipped thì không phù hợp state machine | Sửa ca theo processing→confirmed→preparing; ghi lỗi khi cố nhảy trạng thái |
| TC10 xóa record thử | CRUD qua IIS có test; ảnh/ID thao tác Fauxton cần xác minh | PASS lịch sử IIS; NOT RUN GUI mới |
| TC11 tổng DH001 | JSON mẫu hai Word đều tính đúng 1.150.000 | PASS phép tính offline; live DH001 BLOCKED |
| TC12 JSON sai trong Fauxton | Test response JSON sai ở HTTP stub không phải test editor Fauxton | NOT RUN GUI; giữ test stub như bằng chứng khác |
| TC13 total > ngưỡng Word | Không đồng nhất với A3 manifest mới (khác ngưỡng/điều kiện) | Chọn một query chuẩn và đối chiếu lại, không đổi kết quả cũ để khớp |
| TC14/TC15 tham chiếu | A có KH001/DH001; B thiếu SP002; live chưa đọc | PASS phần tham chiếu có trong file, FAIL thiếu mẫu B; live BLOCKED |

## Ảnh nhúng và chú thích cần xác minh

Không có renderer/dependency bundle chuẩn Documents trong phiên Windows này; chưa render từng trang hoặc kiểm tra pixel ảnh. Vì vậy không chứng nhận nguồn ảnh, nội dung giao diện, số trang cuối hay ảnh khớp môi trường. Bảng sau là audit quan hệ OOXML và chú thích, không phải ảnh demo mới.

| Vị trí ảnh | Resource | Chú thích/công việc cần làm |
|---|---|---|
| P0005/P0030 | image1.jpeg | Logo dùng hai lần; ghi nguồn/quyền sử dụng nếu cần |
| P0191 | image2.png | Sơ đồ logic; đối chiếu quan hệ và thêm app_user/outbox bằng chú giải, không biến thành FK SQL |
| P0282 → P0283 | image3.png | Chú thích đăng nhập Fauxton; không đủ để chứng minh website auth/roles |
| P0288 → P0290 | image4.png | Danh sách DB; cần ngày và rõ doc_count có design docs |
| P0294 → P0296 | image5.png | KH001, không tự chú thích thành p2-customer-001 |
| P0300 → P0302 | image6.png | KH002; xác minh ID và bản dữ liệu |
| P0308 → P0309 | image7.png | Chú thích sản phẩm quá chung; cần ID và trường giá/quantity |
| P0325 → P0326 | image8.png | DH001/items; kiểm tra thời điểm, date/total so với hai biến thể |
| P0332 → P0334 | image9.png | GH001; phải thống nhất enum và đơn tham chiếu |
| P0339 → P0341 | image10.png | Kiểm kê; không chứng minh bộ P2=100 chỉ từ ảnh cũ |
| P0346 → P0348 | image11.png | “đơn hàng hoặc khách hàng” mơ hồ; ghi đúng loại/ID |
| P0353/P0355 → P0354/P0357 | image12/13.png | Cặp trước/sau; cần cùng ID, _rev đổi, trạng thái hợp lệ |
| P0361/P0363 → P0362/P0364 | image14/15.png | Cặp xóa; cần ID mới của buổi demo, không xác nhận đã xóa doc có sẵn |
| P0370 → P0372 | image16.png | “đang xử lý” phải kèm selector và tập ID ở đúng thời điểm |
| P0378/P0381 → P0379/P0382 | image17/18.png | Trước/sau filter active; cần cùng dataset, không suy ra tăng hiệu năng |

Mỗi ảnh giữ hay thay phải có mã hình, người chụp/nguồn, ngày, môi trường và ID/query tương ứng; không chứa credential/cookie/hash. Chưa có căn cứ kết luận ảnh gốc giả hoặc sai pixel, chỉ xác định phần chưa đủ kiểm chứng. Mọi ảnh mới phải do sinh viên thực hiện theo DEMO_DEFENSE.md.
