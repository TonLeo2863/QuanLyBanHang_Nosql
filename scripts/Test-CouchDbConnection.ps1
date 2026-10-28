# Chỉ đọc cấu hình/trạng thái, không tạo hoặc xóa database/document.
$ErrorActionPreference = 'Stop'
$names = @('COUCHDB_URL','COUCHDB_DATABASE','COUCHDB_USERNAME','COUCHDB_PASSWORD')
foreach ($scope in @('Process','User','Machine')) {
    foreach ($name in $names) {
        $present = ![string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name, $scope))
        Write-Output "$scope $name configured=$present"
    }
}
function Read-Endpoint($label, $uri, $headers = @{}) {
    try {
        $response = Invoke-WebRequest -Uri $uri -Headers $headers -UseBasicParsing -TimeoutSec 5
        Write-Host "PASS $label HTTP $($response.StatusCode)"
        return $response
    } catch {
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        Write-Host "FAIL $label HTTP $status (0 = connection/timeout)"
        return $null
    }
}
$health = Read-Endpoint 'CouchDB local health' 'http://127.0.0.1:5984/_up'
if ($health) { $health = $null }
$missing = @($names | Where-Object { [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_)) })
if ($missing.Count -gt 0) {
    Write-Output 'NOT RUN authenticated database/index checks: thiếu cấu hình trong Process. Xem tests/README.md.'
    Write-Output 'NOT RUN initialization/CRUD: script này chỉ chẩn đoán, không ghi dữ liệu.'
    exit 2
}
$server = $null
if (![uri]::TryCreate($env:COUCHDB_URL, [UriKind]::Absolute, [ref]$server) -or
    $server.Scheme -notin @('http','https') -or $server.UserInfo -or $server.Query -or
    $server.Fragment -or $server.AbsolutePath -ne '/' -or ($server.Scheme -eq 'http' -and !$server.IsLoopback) -or
    $env:COUCHDB_DATABASE -notmatch '^[a-z][a-z0-9_$()+/-]*$') {
    Write-Output 'FAIL cấu hình URL/database không hợp lệ. Không hiển thị giá trị cấu hình.'
    exit 1
}
$base = $server.AbsoluteUri.TrimEnd('/') + '/' + [uri]::EscapeDataString($env:COUCHDB_DATABASE)
$headers = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($env:COUCHDB_USERNAME + ':' + $env:COUCHDB_PASSWORD)) }
$database = Read-Endpoint 'configured database (authenticated)' $base $headers
$indexes = Read-Endpoint 'Mango indexes (authenticated)' ($base + '/_index') $headers
$indexFound = $false
if ($indexes) {
    try {
        $indexData = $indexes.Content | ConvertFrom-Json
        $indexFound = @($indexData.indexes | Where-Object { $_.name -eq 'type-id' -and $_.def.fields.Count -eq 2 -and $_.def.fields[0].type -eq 'asc' -and $_.def.fields[1]._id -eq 'asc' }).Count -gt 0
        if ($indexFound) { Write-Output 'PASS index type-id (type,_id)' }
        else { Write-Output 'FAIL index type-id chưa có hoặc sai trường' }
    } catch { Write-Output 'FAIL phản hồi chỉ mục không hợp lệ' }
}
Write-Output 'NOT RUN initialization/CRUD: script này chỉ chẩn đoán, không ghi dữ liệu.'
if (!$database -or !$indexes -or !$indexFound) { exit 1 }
exit 0
