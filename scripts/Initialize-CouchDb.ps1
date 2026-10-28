$ErrorActionPreference = 'Stop'
foreach ($name in @('COUCHDB_URL','COUCHDB_DATABASE','COUCHDB_USERNAME','COUCHDB_PASSWORD')) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) { throw "Thiếu biến $name" }
}
$server = [uri]$env:COUCHDB_URL
if ($server.Scheme -notin @('http','https') -or $server.UserInfo -or $server.Query -or $server.Fragment -or $server.AbsolutePath -ne '/' -or ($server.Scheme -eq 'http' -and !$server.IsLoopback)) { throw 'URL CouchDB không hợp lệ; máy chủ từ xa phải dùng HTTPS.' }
if ($env:COUCHDB_DATABASE -notmatch '^[a-z][a-z0-9_$()+/-]*$') { throw 'Tên database không hợp lệ.' }
$base = $server.AbsoluteUri.TrimEnd('/') + '/' + [uri]::EscapeDataString($env:COUCHDB_DATABASE)
$headers = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($env:COUCHDB_USERNAME + ':' + $env:COUCHDB_PASSWORD)) }
function Send-Json($method, $path, $body, $allowed = @()) {
    try {
        $args = @{ Method=$method; Uri=($base + $path); Headers=$headers; ContentType='application/json'; TimeoutSec=15; UseBasicParsing=$true }
        if ($null -ne $body) { $args.Body = [Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Depth 20)) }
        Invoke-WebRequest @args | Out-Null
    } catch {
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        if ($allowed -contains $status) { return }
        throw "Khởi tạo thất bại (HTTP $status). Kiểm tra dịch vụ, cấu hình và quyền CouchDB; không có dữ liệu bị xóa."
    }
}
Send-Json PUT '' $null @(412)
Send-Json POST '/_index' @{ index=@{fields=@('type','_id')}; ddoc='idx-type-id'; name='type-id'; type='json' }
$documents = @(
    @{_id='demo-customer-001';type='customer';name='Khách mẫu 01';phone='';email='demo@example.invalid';address='Địa chỉ giả lập'},
    @{_id='demo-product-001';type='product';name='Sản phẩm mẫu';category='Demo';price=10000;quantity=20;status='active'},
    @{_id='demo-order-001';type='order';customerId='demo-customer-001';orderDate='2026-10-28';status='shipping';items=@(@{productId='demo-product-001';quantity=2;price=10000});total=20000;paymentMethod='COD';address='Địa chỉ giả lập'},
    @{_id='demo-delivery-001';type='delivery';orderId='demo-order-001';shipperId='demo-shipper';shipperName='Người giao mẫu';status='shipping';pickupDate='2026-10-28';expectedDate='2026-10-29';address='Địa chỉ giả lập'}
)
foreach ($document in $documents) {
    # PUT không _rev: tài liệu có sẵn sẽ trả 409, được giữ nguyên.
    Send-Json PUT ('/' + [uri]::EscapeDataString($document._id)) $document @(409)
}
Write-Output 'Đã bảo đảm database, chỉ mục và bốn ID mẫu tồn tại; tài liệu có sẵn được giữ nguyên.'
