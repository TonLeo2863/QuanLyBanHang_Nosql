# Rà soát trước push – sau P5

**Kết luận: BLOCKED PUSH.** Có thể commit cục bộ các tài liệu an toàn; chưa được phát hành nhánh. Không push, force push, amend/rebase, xóa commit hoặc merge master trong lần rà soát này.

## Bằng chứng kiểm tra

Audit mốc `703b70c`, 10 commit và 244 blob từ toàn bộ refs/reflog local, gồm cả nội dung commit message. Công cụ chỉ xuất đường dẫn, mã quy tắc và SHA object; không xuất giá trị, dòng mã chứa bí mật hoặc hash của credential. Không thử xác thực bằng credential cũ.

| Hạng mục | Kết quả |
|---|---|
| Nhánh/commit P0–P5 | PASS: codex/nosql-completion; giữ nguyên các SHA đã có |
| origin hiệu lực fetch/push | PASS: cùng trỏ TonLeo2863/QuanLyBanHang_Nosql trên github.com, không userinfo/token nhúng |
| File hiện hành chuẩn bị commit | Quét mẫu + đối chiếu kín; không có bí mật thực sau phân loại fixture kiểm thử, phải quét staged lần cuối |
| Credential trong lịch sử | FAIL: blob dbf395c10ab0 ở Services/CouchDbService.cs có password hardcode; được thêm tại c717491 và loại khỏi mã tại fa57030 |
| Mật khẩu đang cấu hình so với literal lịch sử | FAIL: trùng toàn bộ, không chỉ trùng một phần. Coi là đã lộ; không dùng thêm để kiểm thử tích hợp |
| Đã thu hồi credential cũ trên máy chủ | BLOCKED: chủ repo phải thao tác và xác nhận; agent không tự đổi hoặc thử đăng nhập bằng giá trị cũ |
| Repository Public/Private và quyền GitHub | BLOCKED: connector và API GitHub đều HTTP 404; không suy ra Private hoặc đủ quyền push |
| Build solution + Razor | PASS: VS 2022 Community, Debug/MvcBuildViews=true, exit 0 |
| Kiểm thử nghiêm trọng không dùng DB thật | PASS: 49 smoke + 21 unit workflow/security |
| Công cụ audit | PASS: 13 assertion bằng dữ liệu sinh trong bộ nhớ, không dùng credential thật |
| Workflow PR | PASS mô phỏng 10 assertion; chạy thật trên Actions NOT RUN |

Một fixture âm có trường nhạy cảm giả trong tests/Test-Backup.py được rà soát riêng để thử chức năng từ chối import; không phải thông tin xác thực. Ngoại lệ chỉ áp dụng đúng fixture/đường dẫn, không bỏ qua so khớp credential cấu hình và không áp dụng cho service thật. Test của scanner tự sinh dữ liệu trong bộ nhớ, không ghi credential cố định.

## Việc chủ repo cần làm trước push

1. Thay/thu hồi mật khẩu CouchDB đã lộ bằng giao diện quản trị riêng; không chỉ chuyển cùng mật khẩu sang biến môi trường. Cập nhật User variables, mở lại VS/IIS/terminal và xác minh cấu hình mới không còn trùng lịch sử. Không gửi giá trị mới trong chat/Git/log.
2. Xác nhận đúng repository bằng tài khoản chính chủ, chuyển/xác minh Private và cấp quyền kết nối/push cần thiết. HTTP 404 hiện chưa phân biệt được thiếu quyền, repo đổi tên hoặc repo không khả dụng.
3. Thống nhất phương án xử lý dấu vết đã chia sẻ và lịch sử với thành viên nhóm. Giữ nguyên P0–P5 theo yêu cầu hiện tại; bất kỳ rewrite/force push/làm sạch lịch sử đều cần phê duyệt riêng, không thực hiện tự động. Xóa bí mật ở HEAD không xóa nó khỏi lịch sử.
4. Chạy lại audit với cấu hình đã thay, kiểm tra staged/diff và chẩn đoán/integration bằng credential mới an toàn. Quét phải ghi rõ lịch sử vẫn có giá trị đã thu hồi nếu chưa có phương án xử lý; không báo lịch sử sạch chỉ vì mật khẩu hiện tại đã khác.
5. Khi được phép push: báo bằng tiếng Việt nội dung/file/test/rủi ro, chỉ push nhánh tính năng được chủ repo cho phép (P6 hiện ở codex/p6-report-audit), đích PR master; không merge tự động. Workflow chưa được coi hoạt động thật cho tới khi có run được quan sát.

## File được giữ và chỉnh lý

- AGENTS.md: quy tắc bảo mật/Git/kiểm thử, bổ sung điểm tiếp tục sau P5.
- ROADMAP_NOSQL.md: giữ baseline/lộ trình, thêm trạng thái hiện hành và bộ seed 20/30/30/20.
- HUONG_DAN_BAT_CODEX.md và PROMPT_KHOI_DONG_CODEX.txt: giữ hướng dẫn cần thiết, bỏ yêu cầu làm lại P0/P1 và các bước tạo lại nhánh/push mặc định. Phần CLI dẫn tới OpenAI Docs chính thức.
- .github/workflows/ghi-chu-pr-tieng-viet.yml: pin actions/github-script v7 theo SHA đã đối chiếu API repository chính thức; quyền tối thiểu, không checkout/chạy source PR, không chạy job trên fork, sửa newline và vô hiệu mention trong tiêu đề. Token chỉ tham chiếu secrets.GITHUB_TOKEN, không có giá trị hardcode.

Giới hạn: đây là quét mẫu và so khớp trong refs/reflog đã có local, không phải chứng minh forensic không còn mọi loại bí mật. Chưa fetch refs khác, kiểm tra fork/cache/bản clone của người khác, cấu hình máy chủ hoặc thu hồi credential bên ngoài. Không lưu bản sao source lịch sử chứa bí mật vào file báo cáo. Các blocker này không phủ định kiểm thử chức năng P0–P5 đã có, nhưng ngăn kết luận an toàn để push.
