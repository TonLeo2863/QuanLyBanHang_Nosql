# Mở Codex và tiếp tục đúng trạng thái dự án

## Mở workspace

Dùng workspace hiện có chứa `QuanLyBanHang.sln`. Trên máy chủ dự án hiện tại, thư mục là `C:\Users\TonLeo\Documents\QuanLyBanHang`; máy khác dùng đường dẫn clone riêng. Không tạo lại repo hoặc nhánh khi đã có.

```powershell
Set-Location 'C:\Users\TonLeo\Documents\QuanLyBanHang'
git status
git branch --show-current
git log -5 --oneline
codex --version
codex
```

Nếu dùng ứng dụng Codex thay CLI, mở đúng workspace đó rồi đọc/dán PROMPT_KHOI_DONG_CODEX.txt. P0–P5 đã có commit; tiếp tục từ tiến độ, không yêu cầu làm lại P0/P1. Khi có thay đổi chưa commit, giữ nguyên và rà soát trước khi đổi nhánh.

## Nếu chưa có CLI

Theo [OpenAI Docs – Codex CLI](https://learn.chatgpt.com/docs/codex/cli), cài bằng phương thức Windows được hướng dẫn hiện hành, mở thư mục dự án rồi chạy `codex` và đăng nhập qua luồng chính thức. Không ghi API key/token vào source hoặc hướng dẫn này. Nếu PowerShell chặn shim của một bản cài npm, dùng terminal/shim phù hợp với bản cài đó; không tắt bảo mật toàn máy. Việc cài/đăng nhập CLI không được thực hiện tự động trong lần rà soát repository này.

## Git và điều kiện trước khi chia sẻ

Nhánh hiện tại phải là `codex/nosql-completion`. Không chạy lại `git switch -c` cho nhánh đã tồn tại; không tự pull/merge master vào workspace đang có thay đổi. `origin` dự kiến là `TonLeo2863/QuanLyBanHang_Nosql`; kiểm tra URL hiệu lực ở cả fetch và push nhưng không in URL nếu có userinfo/token.

1. Đọc AGENTS/ROADMAP/PROGRESS/RELEASE_READINESS và kiểm tra file chuẩn bị commit.
2. Quét file và lịch sử không in bí mật. Audit sau P5 thấy mật khẩu đang cấu hình trùng literal lịch sử: **chưa được push**. Chủ repo phải thay/thu hồi credential, xác minh cấu hình mới, bảo vệ repo và phê duyệt riêng cách xử lý lịch sử.
3. GitHub trả 404 không chứng minh repo Private; có thể thiếu quyền hoặc repo không còn khả dụng. Chủ repo cần kiểm tra visibility và quyền truy cập chính chủ.
4. Commit cục bộ bằng tiếng Việt, chọn file cụ thể. Không đưa bin/obj/.vs, snapshot hoặc cấu hình riêng vào Git. Không tự đổi/xóa commit P0–P5.
5. Chỉ khi được phép và đã xác minh đủ điều kiện mới chuẩn bị push/PR vào master; trước push báo nội dung/file/test/rủi ro bằng tiếng Việt. Không tự merge.

Workflow `.github/workflows/ghi-chu-pr-tieng-viet.yml` chỉ thêm ghi chú khi mở/cập nhật PR nội bộ từ nhánh codex/; không chạy source PR, không thay báo cáo thủ công và không phải cơ chế quét/thu hồi bí mật. Việc workflow hoạt động thật trên GitHub vẫn cần quyền Actions/PR và một lượt chạy được quan sát.

## Kiểm thử

Build cả Razor và chạy các ca phù hợp theo tests/README.md. Không chạy thêm tích hợp bằng credential đã bị xác định lộ; sau khi chủ repo cung cấp cấu hình mới an toàn, chẩn đoán và chạy lại nếu cần. Phân biệt PASS, FAIL, BLOCKED và NOT RUN; không lấy kết quả build thay cho kiểm tra lịch sử/visibility hay demo GUI.
