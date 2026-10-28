# Nội dung bổ sung P6 dành cho người biên tập Word

Đây là bản đề xuất chèn/thay, không phải bản Word đã dàn trang. Giữ nguyên hai file gốc. Mã P/T bên dưới theo WORD_RECONCILIATION.md; tìm theo tiêu đề và cụm từ trong Word, không coi là số trang. Nguồn [C…/M…/R…/P…] tra SOURCES.md. Không ghi toàn bộ nội dung P6 vào phụ lục rồi giữ những kết luận sai trong thân bài.

## Mở đầu – thay phạm vi tại A:P0068–P0069

Đề tài nghiên cứu Apache CouchDB và công cụ quản trị Fauxton thông qua ứng dụng quản lý đơn hàng và giao hàng của cửa hàng bán lẻ trực tuyến. Ứng dụng minh họa được xây dựng bằng ASP.NET MVC 5, .NET Framework 4.7.2 và C#, giao tiếp CouchDB bằng HTTP API. Phạm vi hiện thực là hệ thống nội bộ cho Administrator, OrderManager và Shipper; Customer là đối tượng nghiệp vụ, chưa có cổng tự đặt hàng cho khách. Hệ thống đã có đăng nhập, kiểm tra quyền phía máy chủ, quản lý dữ liệu, luồng đơn hàng–giao hàng, Dashboard và truy vấn có phân trang. Thanh toán trực tuyến, tích hợp hãng vận chuyển và kiểm thử tải lớn nằm ngoài phạm vi đã nghiệm thu.

Bộ seed chuẩn gồm 20 khách hàng, 30 sản phẩm, 30 đơn hàng và 20 phiếu giao hàng. Đây là số lượng từ mã nguồn tạo dữ liệu, không phải số document hiện tại của máy chủ. Bằng chứng P5 ghi nhận một snapshot có 104 document nghiệp vụ. Tại P6, việc đọc dữ liệu có xác thực bị chặn do thông tin đăng nhập cần được thay; vì vậy báo cáo không khẳng định số lượng hiện tại bằng số lượng seed hoặc snapshot cũ.

## Chương 1 – bổ sung sau phần giới thiệu CouchDB/Fauxton, sửa §1.6–1.7

### 1.x Cơ sở lựa chọn hệ quản trị

Chèn bảng 16 đặc trưng trong DB_COMPARISON.md thành hai bảng liên tiếp, giữ cùng bốn cột hệ quản trị. Mỗi nhóm có dẫn nguồn chính thức. Không thu nhỏ font để ép một bảng rộng vào một trang.

CouchDB được chọn vì trọng tâm đề tài là quản trị CSDL tài liệu bằng Fauxton. HTTP API cho phép quan sát cùng document từ công cụ quản trị và ứng dụng MVC, thuận lợi giải thích JSON, Mango, revision và replication. Nhúng các dòng hàng trong Order giúp đọc một đơn hàng và tổng tiền trong một document; đây là ranh giới cập nhật nguyên tử phù hợp cho ví dụ học tập. [C1/C2/C3/C9]

Lựa chọn này có đánh đổi: CouchDB không cung cấp foreign key hay transaction nguyên tử tùy ý trên Order và Delivery. Ứng dụng phải kiểm tra tham chiếu và điều phối cập nhật nhiều document. Vì vậy không thể suy luận hệ thống có mức bảo đảm như một transaction quan hệ chỉ vì từng lần PUT thành công. MongoDB có transaction nhiều document trong cấu hình phù hợp; RavenDB có session và cơ chế transaction theo chế độ; PostgreSQL có transaction, ràng buộc và JSONB, là phương án đáng cân nhắc nếu ưu tiên nhất quán nghiệp vụ nhiều thực thể. Các phương án này không được chọn vì phạm vi nghiên cứu và thành quả CouchDB hiện có, không phải vì chúng thiếu khả năng xử lý đơn hàng. [M3/R1/R2/P1/P2/P3]

### 1.x Công cụ quản trị

Fauxton là giao diện quản trị đi kèm CouchDB, sử dụng API máy chủ để quản lý database, document và truy vấn. MongoDB Compass là GUI của MongoDB, không kết nối thay Fauxton để quản trị CouchDB. Chèn bảng so sánh ngắn trong DB_COMPARISON.md. Theo yêu cầu đã đính chính, minh họa ít nhất ba chức năng Fauxton: quản lý document/revision, Mango/index và replication; không yêu cầu triển khai ba hệ quản trị hoặc demo ba GUI khác nhau. [C9/M6]

Thay đoạn “chưa có Dashboard” tại P0166: ứng dụng hiện có Dashboard với các số đếm được kiểm tra ở P5; chưa có cơ sở tuyên bố hiệu năng ở quy mô lớn hoặc khả năng BI chuyên sâu.

## Chương 2 – sửa mô hình và bổ sung sau §2.3, §2.8

### 2.x Ranh giới document và tham chiếu

Customer, Product, Order và Delivery là bốn loại document nghiệp vụ. Trường type phục vụ selector và index; _id định danh document; _rev là token revision do CouchDB quản lý. OrderItem được nhúng trong items của Order thay vì lưu thành document độc lập. Dòng hàng giữ mã sản phẩm, tên/giá tại thời điểm đặt và số lượng, giúp diễn giải lịch sử đơn khi thông tin sản phẩm về sau thay đổi. Customer, Product và Order được liên kết bằng mã định danh logic, không phải khóa ngoại do máy chủ CouchDB cưỡng chế. [C1/C2; source Models, WorkflowService]

Ứng dụng kiểm tra sự tồn tại của khách hàng/sản phẩm, số lượng, miền tiền, tổng tính phía máy chủ và chuyển trạng thái. Các kiểm tra trước khi ghi không loại trừ mọi race khi có nhiều bên ghi đồng thời hoặc ghi trực tiếp bằng Fauxton. Không dùng mô tả “toàn vẹn tham chiếu tuyệt đối” cho thiết kế này. Đọc DATABASE_DESIGN.md để lấy ví dụ JSON chuẩn; không nhập đè các ví dụ KH001/DH001 giữa hai Word vì cùng ID có nội dung khác nhau.

### 2.x Trạng thái chuẩn và cập nhật liên document

| Đối tượng | Giá trị chuẩn trong ứng dụng | Cách dùng trong báo cáo |
|---|---|---|
| Order | processing, confirmed, preparing, shipping, delivered, cancelled | Luồng chính processing → confirmed → preparing → shipping → delivered; hủy theo điều kiện dịch vụ |
| Delivery | pending, assigned, shipping, delivered, failed, cancelled | Gán Shipper trước giao; giao thất bại có thể phân công/giao lại theo dịch vụ |

Không dùng shipped cho Order hoặc preparing/in_transit cho Delivery. Tạo Delivery phải gắn Order hợp lệ và đúng giai đoạn; không suy ra mọi chuỗi giữa các giá trị trong bảng đều được phép. Quy tắc chi tiết được thực hiện trong WorkflowService, không chỉ trong nút giao diện.

Order lưu ý định đồng bộ và phiên bản phục vụ cập nhật Delivery; nếu thao tác tiếp theo lỗi, cơ chế retry có thể hoàn thành projection. Đây là điều phối nhất quán cuối cùng, không phải transaction ACID trên hai document. Xung đột revision được giữ lại để xử lý có kiểm soát, không ghi đè mù. P5 có ca inject lỗi và phục hồi; P6 chưa chạy lại ca có xác thực. Bổ sung sơ đồ và phân tích trong SECURITY_AND_TECHNICAL_DEFENSE.md.

### 2.x Dữ liệu minh họa và sai lệch cần công bố

Nguồn seed có 100 document nhưng audit P6 phát hiện 5 cặp Order confirmed–Delivery assigned lệch quy tắc projection hiện tại và 10 Delivery assigned chưa có shipperId. Kết quả kiểm tra này là FAIL đối với nguồn seed; không chứng minh dữ liệu máy chủ đang giống hệt. Những ca tham chiếu/tổng tiền đã PASS ở P5 không bao phủ đầy đủ điều kiện mới này. Không tự sửa document cũ để làm báo cáo PASS; chuẩn bị bộ demo mới qua ứng dụng trên database thử riêng sau khi thay credential và kiểm tra quyền.

## Chương 3 – bổ sung triển khai và năm chức năng Fauxton

### 3.x Kiến trúc ứng dụng đã triển khai

Trình duyệt truy cập ASP.NET MVC chạy trên IIS Express; Controller kiểm tra phiên đăng nhập, vai trò, anti-forgery và ModelState, sau đó gọi service CouchDB phía máy chủ. Credential CouchDB được đọc từ biến môi trường, không đưa xuống JavaScript/HTML. Đây là backend MVC trả View/form; không mô tả như một bộ REST API công khai độc lập nếu chưa triển khai API đó. Công nghệ vẫn là MVC 5/.NET Framework 4.7.2, không phải ASP.NET Core.

### 3.x Các chức năng quản trị và bảo mật

Chèn lần lượt năm mục F1–F5 từ FAUXTON_MODULES.md. Mỗi mục giữ các tiểu mục: khái niệm/mục đích, nguyên lý, thao tác, ví dụ qlbanhang, kết quả mong đợi, kết quả thực tế, ảnh cần chụp, ưu/nhược điểm và câu hỏi phản biện. F1–F3 là ba chức năng trọng tâm. _explain, export/restore và một số thao tác kiểm chứng có thể dùng REST API/script hỗ trợ; không gán tất cả thành nút GUI của Fauxton.

Thêm phần bảo mật từ SECURITY_AND_TECHNICAL_DEFENSE.md, đặc biệt phân biệt server admin/database admin/member với ba role website. _security thực tế chưa đọc được trong audit P6; anonymous bị 401 là bằng chứng từ chối truy cập, không đủ kết luận cấu hình member/admin đúng. Website kiểm tra vai trò không ngăn người có credential DB thao tác trực tiếp. HTTP Basic không tự mã hóa kênh truyền; kiểm thử HTTPS chưa thực hiện. Không đưa mật khẩu hay URL chứa thông tin đăng nhập vào hình và phụ lục.

Replication sao chép thay đổi giữa database và có thể mang cả xóa/conflict; không phải bản sao lưu bất biến. Sao lưu logic hiện có chọn document nghiệp vụ và JSON index, loại trừ app_user, cấu hình/quyền/attachments và lịch sử revision; phục hồi vào DB thử riêng, không thay thế bản sao lưu đầy đủ hệ thống. Chèn hướng dẫn F3/F5 cùng BACKUP_RESTORE.md, giữ đúng giới hạn. [C6/C7/C8]

## Chương 4 – thay kết luận tất cả Đạt tại T34/T35

Chèn bảng bằng chứng trong TECHNICAL_AUDIT.md, ghi ngày 28/10/2026 và cột “lượt chạy/phạm vi”. PASS cũ P5 giữ lịch sử; không đổi thành kết quả mới P6. Các trường hợp GUI/ảnh và câu query legacy trong Word chưa đối chiếu trực tiếp phải là NOT RUN hoặc BLOCKED.

Trong đợt P6, build Solution và Razor thành công; smoke 49 assertion, workflow/security 21 assertion thành công. Một bản trích xuất mã nguồn tracked tại 1d88092, không kèm packages/bin/obj hay lịch sử, đã khôi phục NuGet và build/Razor thành công. Điều này chứng minh khả năng build từ snapshot mã nguồn sạch, chưa chứng minh việc clone từ GitHub, cấu hình trên máy khác hay vận hành website trên môi trường mới.

Probe chỉ đọc không xác thực ghi nhận CouchDB 3.5.2 đang hoạt động; truy cập database, _security và _membership bị 401. Kiểm thử có xác thực không chạy lại vì credential đang cấu hình trùng bí mật lịch sử. Audit không đánh dấu các ca này PASS bằng cách tái sử dụng credential đã lộ. Kết quả seed consistency mới là FAIL và phải ghi rõ bên cạnh những kiểm thử P5 đã thành công.

Bỏ nhận định hiệu năng “nhanh” như kết quả định lượng nếu chưa có workload, cỡ dữ liệu, thời gian, số lần lặp và môi trường đo. Tách ảnh cũ ngày 23/10 khỏi kết quả chạy ngày 28/10. Không thay nhãn thời gian để tạo cảm giác cùng một buổi nghiệm thu.

## Chương 5 – thay hạn chế lỗi thời tại P0464–P0499

Ứng dụng đã triển khai giao diện MVC, xác thực, ba vai trò, validation phía máy chủ, CSRF, quản lý đơn hàng–giao hàng và sao lưu logic. Vì vậy không tiếp tục liệt kê các chức năng này như chưa tồn tại. Kết quả triển khai có giới hạn: chưa kiểm chứng GUI Fauxton trong đợt audit, chưa kiểm thử HTTPS và single-node đầy đủ, chưa có benchmark tải, thanh toán, tích hợp hãng vận chuyển, giữ tồn kho chống cạnh tranh hoặc transaction nguyên tử nhiều document. Phân công nhân viên và quản lý quyền đã có; thông báo tự động, nhiều kiện giao và tích hợp ngoài có thể giữ ở hướng phát triển.

Trước khi chia sẻ repository hoặc báo cáo trực tiếp, chủ dự án cần thay/thu hồi credential đã lộ, xác minh quyền và trạng thái Private, thống nhất xử lý lịch sử với các thành viên; không tự rewrite lịch sử đã hoàn thành. Bộ demo nên dùng document giả mới trong DB thử, được kiểm tra chéo bằng website và Fauxton. Báo cáo cần hoàn tất ảnh thật, định dạng và bảng kết quả trước khi kết luận đạt các tiêu chí thực hành.

## Tài liệu tham khảo và phụ lục

Thay các dẫn trang chủ chung bằng URL cụ thể trong SOURCES.md, kèm ngày truy cập và phiên bản. Không giữ tài liệu tác giả/tên sách chưa xác minh như một nguồn đã đọc. Tài liệu giảng viên chưa được cung cấp trong hai Word này; không tự gán trọng số hoặc lời trích cho giảng viên Nguyễn Thị Định. Phụ lục JSON chỉ chọn một bộ canonical; bỏ bản trùng trái nội dung. Không đưa raw export, credential hay hash tài khoản vào phụ lục.

## Dự toán 56 trang và kiểm tra biên tập

| Phần | Trang dự kiến |
|---|---:|
| Bìa, phân công, mục lục/danh mục | 4 |
| Mở đầu | 2 |
| Chương 1 | 8 |
| Chương 2 | 11 |
| Chương 3 | 14 |
| Chương 4 | 8 |
| Chương 5 | 3 |
| Tài liệu tham khảo và phụ lục chọn lọc | 6 |
| Tổng | 56 |

Đây là dự toán, **chưa xác minh bản Word 50–60 trang**. Bản A có metadata 50 trang nhưng Normal 14pt và nhiều paragraph giãn khoảng 1,15, không đồng nhất yêu cầu. Người biên tập đặt Times New Roman 13, giãn 1,5 theo mẫu trường; xử lý cả style và định dạng trực tiếp, bảng/chú thích; xóa đoạn trống dư, rút JSON trùng trước khi chèn. Cập nhật TOC/caption/cross-reference, xuất PDF và kiểm tra từng trang bằng mắt: ngắt bảng, ảnh rõ/chú thích đúng, không lộ bí mật, không tràn dòng. Giữ nội dung kiểm thử thiếu bằng chứng ở trạng thái chưa đạt; không ép đủ trang bằng kết quả giả.
