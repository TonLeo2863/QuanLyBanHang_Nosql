# Kết quả kiểm thử – 28/10/2026

PASS = đã chạy đạt; FAIL = đã chạy không đạt; BLOCKED = thiếu điều kiện/quyền; NOT RUN = chưa chạy. Đây là bằng chứng kỹ thuật, không phải tự chấm điểm. Môi trường: Windows, VS 2022 Community, .NET Framework 4.7.2, IIS Express loopback, CouchDB thật qua User variables. Không ghi credential/cookie vào báo cáo.

| Nhóm | Kết quả đã có | Bằng chứng |
|---|---|---|
| P2 seed/NoSQL | PASS | 100 tài liệu P2: 20/30/30/20; lần 2 thêm 0; nội dung/revision nguyên vẹn; tham chiếu/tổng đúng |
| P3 Mango | PASS | B1..B5 counts 20/10/2/5/5; A1..A5 counts 11/11/4/1/6; 10 explain đúng index; bookmark limit 7 đủ/không trùng |
| Build/Razor cuối P5 | PASS | MSBuild Debug/MvcBuildViews=true, Run-AllTests.ps1 exit 0 |
| P4 smoke | PASS | 49 assertion; 401/403/404/409/500, timeout/mất kết nối/JSON lỗi, encoding/revision/validation và CSRF |
| Unit workflow/security cuối P5 | PASS | 21 assertion (19 ở P4); miền tiền/hash, inject lỗi projection/ack, phục hồi/retry, xung đột giữ revision |
| P4 tích hợp IIS thật | PASS | DB qlbanhang_test_7cfbcf0b10f840d8a7e4cb3bc201b023, Run-LiveIntegration.ps1 exit 0 |
| Phân quyền | PASS | anonymous redirect; Manager không quản trị user; Shipper chỉ phiếu được giao; khóa user có hiệu lực request sau |
| CRUD/luồng | PASS | tổng server, dữ liệu sai/mã trùng/revision/trường phụ/xóa tham chiếu; confirmed/preparing/assigned/shipping/failed/retry/delivered |
| Đồng bộ | PASS | crash giả lập sau intent trên DB thật, retry không trùng, xung đột giữ intent và revision ngoài; unit inject HTTP failure |
| Lọc/phân trang/tiếng Việt | PASS | trang đầu 20 dòng, trang sau không lặp; giữ assertion tiếng Việt sau sửa BOM/encoding |
| P5 snapshot/import/verify cuối | PASS | 104 doc + 9 index; restore qlbanhang_restore_c8f5a7bbff463336, SHA256/định nghĩa index khớp; nhập lại skip 104/giữ revision |
| P5 import lỗi | PASS | ID khác nội dung/checksum sai/JSON khóa trùng/NaN/app_user/trường nhạy cảm/rev/tổng âm/tham chiếu thiếu bị từ chối |
| P5 miền tiền/codec | PASS tool và IIS | số quá lớn/nhiều số lẻ bị từ chối trước ghi, tổng vượt miền bị chặn; số tiền hỗ trợ lưu/phục hồi đúng và thông báo tiếng Việt đúng encoding |
| Bộ nghiệm thu tổng hợp cuối P5 | PASS, exit 0 | Run-AllTests.ps1 chạy từ build đến lỗi backend; lần đầu FAIL startup IIS, đã sửa readiness có giới hạn và chạy lại toàn bộ |
| Tích hợp IIS cuối P5 | PASS | DB qlbanhang_test_35ecc98ba01b4bc088619331176aa96e; bổ sung Dashboard/Shipping trước–sau, POST vượt quyền và miền tiền |
| Import có index xung đột / export pending | PASS | cùng tên index khác trường bị chặn trước ghi; app_user bị loại; pending sync không xuất snapshot gây hiểu nhầm |
| Lỗi backend/IIS public routes | PASS | DB thiếu, máy chủ loopback không kết nối, mật khẩu sai; 3 lượt lỗi Login có kiểm soát + 7 GET/4 POST anonymous/validation/CSRF mỗi lượt |
| GUI Fauxton/ảnh/bố cục | NOT RUN | Có kịch bản/danh sách ảnh; không tạo ảnh giả |
| Single-node qua API quản trị | BLOCKED | /_membership HTTP 403; sinh viên xác minh Setup bằng quyền quản trị |
| Credential cũ/Private/lịch sử Git | FAIL lịch sử, BLOCKED push | Audit sau P5 xác nhận mật khẩu cấu hình trùng literal lịch sử; GitHub 404 chưa xác minh Private/quyền. Không thử credential cũ hoặc rewrite lịch sử |
| Publish HTTPS/multi-node/backup toàn máy chủ | NOT RUN | Không suy từ IIS Express/backup logic |
| Clone sạch và demo trên máy thành viên khác | NOT RUN | Có hướng dẫn NuGet/config/provision/chạy test, cần sinh viên diễn tập thực tế |

Lỗi từng gặp: Razor @foreach, Unicode view, index fallback, thiếu index DB thử; chỉ PASS sau sửa/chạy lại. Ca precision cực lớn không bị bỏ qua: bổ sung validation app/import từ chối trước ghi; không coi mọi decimal .NET đều được CouchDB giữ chính xác.

Snapshot generated chỉ ở artifacts; DB thử/restore giữ để đối chiếu. Admin website DB chính chưa có mật khẩu cố định, người demo tự provision bằng Create-AppUser.py. Ảnh thực tế theo SCREENSHOT_CHECKLIST.md vẫn cần sinh viên chụp. Không push/merge trong phiên này.

Lệnh nghiệm thu: `powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-AllTests.ps1`. Đã kiểm tra đủ tài liệu bàn giao tồn tại; syntax 16 Python file PASS, PowerShell parser của 4 runner 0 lỗi. Kết quả chi tiết theo giai đoạn/commit ở CODEX_PROGRESS.md.

Trước commit, thay các số mẫu vô tình khớp một phần bí mật cấu hình sau khi scan chặn (không in giá trị). Giữ nguyên miền và assertion; chạy lại WorkflowUnit 21 PASS, Test-Backup PASS (qlbanhang_restore_6425c26243f3cf9a), LiveIntegration PASS (qlbanhang_test_0bfb658e50ae4293b86528e5d082858a), exit 0. Secret scan sau thay đổi: 0 phát hiện; không đổi credential hoặc vô hiệu kiểm thử.

Audit phát hành bổ sung: build/Razor + 49 smoke + 21 unit chạy lại PASS; Test-GitSafety 13 và workflow mock 10 PASS. Quét 10 commit/244 blob ở mốc 703b70c phát hiện credential lịch sử trùng cấu hình hiện tại, nên không chạy thêm tích hợp bằng cấu hình đó. Không diễn giải staged an toàn là lịch sử sạch. Origin fetch/push đúng mục tiêu, không credential nhúng; GitHub connector/API 404, visibility chưa xác minh. Chi tiết và bước chủ repo ở RELEASE_READINESS.md.

## Đợt P6: phân biệt kết quả mới và lịch sử

Ngày 28/10/2026, nhánh codex/p6-report-audit từ 1d88092. Không thay code ứng dụng/seed, không ghi CouchDB, không rewrite commit P0–P5. Xem [ma trận audit](p6/TECHNICAL_AUDIT.md).

| Ca mới P6 | Kết quả | Giới hạn |
|---|---|---|
| Build Solution/Razor | PASS exit 0 | VS2022 Community Debug/MvcBuildViews=true |
| Smoke / unit workflow | PASS 49 / 21 | Không thay lượt integration có auth |
| Git safety unit / review Word regression | PASS 13 / PASS | Kiểm thử scanner/detection, không làm lịch sử/seed thành an toàn |
| Nguồn sạch khôi phục NuGet + build/Razor | PASS exit 0 | git archive tracked source 1d88092; không phải remote clone/runtime máy mới |
| Anonymous CouchDB read-only | PASS phạm vi | 3.5.2, root/health200, session anonymous, DB/security/membership và mẫu401 |
| Nguồn seed so quy tắc projection | FAIL exit 1 | 5 cặp confirmed/assigned lệch; 10 assigned không shipperId; chưa so DB thật |
| Toàn lịch sử local secret scan | FAIL exit 1 | Credential cấu hình trùng literal lịch sử; không in giá trị/không xác thực lại |
| Integration có auth, Mango/explain/backup mới | BLOCKED | Cần chủ repo thay credential trước; PASS P5 ở trên giữ nguyên là lịch sử |
| _security thực tế, server admin/single-node | BLOCKED | Không tự thay quyền; anonymous401 không đủ xác nhận |
| Replication mới, GUI/ảnh, HTTPS | NOT RUN | Đã có kịch bản, chưa có kết quả thực thi |
| Phân trang Word sau sửa | NOT RUN | Chỉ giao nội dung Markdown, chưa xuất/render Word/PDF |

Sai lệch Word/JSON và kết quả seed mới được công bố riêng thay vì thay assertion cho PASS. Hai Word gốc có hash không đổi; ảnh cũ chỉ kiểm kê vị trí/chú thích, chưa xác minh nội dung pixel/nguồn/môi trường. Artifact trích xuất đã che dữ liệu chỉ lưu cục bộ và bị ignore.
