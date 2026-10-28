# Audit kỹ thuật P6 – 28/10/2026

Mốc đối chiếu 1d88092, trên nhánh riêng codex/p6-report-audit; P5 703b70c là ancestor. Không sửa code ứng dụng hoặc dữ liệu, không rewrite/merge/push. PASS = đã chạy đạt đúng phạm vi; FAIL = đã chạy phát hiện sai; BLOCKED = không đủ điều kiện an toàn/quyền; NOT RUN = chưa chạy. Bằng chứng P5 là lịch sử trong TEST_RESULTS.md, không phải chạy lại P6.

| Hạng mục | P6 hiện tại | Bằng chứng và giới hạn |
|---|---|---|
| Solution + Razor | PASS | MSBuild VS2022 Community Debug, MvcBuildViews=true, exit 0 |
| Smoke | PASS | Run-SmokeTests.ps1: 49 assertion |
| Workflow/security unit | PASS | Run-WorkflowUnit.ps1: 21 assertion; không thay thế phân quyền HTTP thật |
| Khôi phục thư viện/build nguồn sạch | PASS phạm vi snapshot | git archive 1d88092 vào thư mục mới không packages/bin/obj, RestorePackagesConfig=true từ NuGet chính thức, build/Razor exit 0; lần đầu restore bị giới hạn mạng, lần chạy có quyền thành công |
| Remote clone và máy sinh viên khác | NOT RUN | Snapshot không chứa lịch sử; không xác nhận quyền clone GitHub hoặc cấu hình runtime trên máy khác |
| CouchDB đang hoạt động | PASS chỉ đọc anonymous | P6-ReadOnlyProbe.py: root/health 200, server 3.5.2, session anonymous không server admin |
| Từ chối đọc DB khi anonymous | PASS | qlbanhang, _security, _membership, hai ID mẫu đều 401; không trả nội dung document |
| Giá trị _security hiện hành | BLOCKED | Không dùng credential lộ; 401 không chứng minh cấu hình member/admin cụ thể |
| Server admin, membership, single-node | BLOCKED | P5 membership 403; P6 anonymous 401; không suy ra single-node từ UI hoặc số máy đang dùng |
| Credential và lịch sử Git | FAIL | 11 commit/258 blob local refs+reflogs; một blob lịch sử có literal trùng toàn bộ mật khẩu cấu hình. Không in giá trị, không xác thực lại bằng bí mật đó |
| File chuẩn bị commit | PASS | Review-Staged.py: 0 phát hiện sau sửa fixture URL giả; toàn worktree scan không phát hiện. Không coi staged sạch là lịch sử an toàn; Word gốc/artifacts bị ignore |
| Origin | PASS | Fetch/push đúng github.com/TonLeo2863/QuanLyBanHang_Nosql, không credential nhúng |
| Private/quyền GitHub | BLOCKED | Connector/API trước đó 404, không chứng minh repo Public hay Private; không push |
| Website login/3 role/CSRF/validation | BLOCKED chạy lại live | Source kiểm tra server; P5 có IIS PASS các ca vượt quyền/khóa user/token thiếu. Unit/smoke P6 PASS không thay thế lượt live mới |
| 401/403/404/409/500, mất kết nối | PASS smoke; live BLOCKED | P5 có backend thật DB thiếu/mật khẩu ngẫu nhiên sai/URL loopback lỗi; không lặp authenticated test P6 |
| Revision/outbox/luồng CRUD thật | BLOCKED chạy lại | P5 PASS trên DB thử; unit P6 PASS; chưa kết luận DB chính nhất quán hiện tại |
| Seed tham chiếu/tổng | Bằng chứng P5 PASS | Bảo toàn ID/revision/tham chiếu/tổng từng kiểm tra; không bao phủ mọi projection/state rule |
| Seed trạng thái so quy tắc P4 | FAIL | P6-SeedConsistency.py exit 1: 5 Order confirmed với Delivery assigned cần preparing; 10 assigned thiếu shipperId. Chỉ source seed, chưa đọc DB hiện tại |
| Mango/index/sort/bookmark/_explain | BLOCKED chạy lại live | P3/P5 có 10 query/_explain và bookmark PASS; nội dung truy vấn/index nằm trong docs/mango-queries.json, không dùng lọc toàn bộ C# thay thế |
| Import/export, logic backup/restore | BLOCKED chạy lại live | P5 có 104 doc/9 index, đối chiếu/hash và import lại PASS; không phải sao lưu đầy đủ máy chủ |
| Replication source/target | NOT RUN/BLOCKED credential | Đã soạn kịch bản 3 doc giả, hai DB thử riêng; chưa tạo DB hay gọi replication |
| GUI Fauxton, ảnh và HTTPS | NOT RUN | Có ảnh Word cũ, chưa xác minh pixel/nguồn/môi trường mới; không tạo ảnh giả hoặc xác nhận thao tác GUI chưa chạy |
| Hai Word | PASS đọc cấu trúc | OOXML 569/56 paragraph, JSON A12/B4, đối chiếu 4 ID khác nội dung; không sửa bản gốc |
| Word 50–60 trang/TNR13/1,5 | NOT RUN nghiệm thu layout | Normal A14pt, nhiều paragraph khoảng1,15; cached Pages50 không phải render mới; chỉ giao đề xuất Markdown |

## Cách chạy lại an toàn

Offline: `python scripts/Inspect-ReviewInputs.py`, `python scripts/Compare-ReviewJson.py`, `python scripts/P6-SeedConsistency.py`, `python tests/Test-P6Review.py`. Extractor đọc biến môi trường chỉ để che giá trị trong output; không gửi authentication. Seed audit phải trả exit 1 khi còn các mismatch đã công bố.

Probe anonymous: `python scripts/P6-ReadOnlyProbe.py` cần COUCHDB_URL hợp lệ; không Authorization/Cookie, không ghi DB. Không truyền credential trên command line. Các artifacts cục bộ không commit.

Sau khi chủ repo thay/thu hồi credential, xác minh Private/quyền và cấu hình riêng: kiểm tra kết nối bằng script hiện hữu, chạy lại suite tests/README.md trên DB thử, làm F1–F5 theo FAUXTON_MODULES.md và ghi kết quả/ảnh mới. Không tự sửa _security/credential quản trị để làm PASS. Seed cũ cần kế hoạch migration có chủ sở hữu; demo mới nên tạo qua website trên DB thử, không overwrite p2-*.

## Kết luận nghiệm thu

P6 đã có nội dung học thuật, đối chiếu Word, kịch bản và audit; **chưa hoàn tất nghiệm thu demo/báo cáo Word**. Blocker phát hành là credential lịch sử; thiếu minh chứng đáng kể là replication GUI, ảnh thực tế, cấu hình quyền/single-node và HTTPS. Sai lệch seed phải được công bố, không dùng kết quả tham chiếu/tổng PASS cũ để phủ nhận FAIL mới.
