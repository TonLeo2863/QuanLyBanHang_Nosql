param([string]$TestScript='tests/Test-Workflow.py')
$ErrorActionPreference='Stop'
foreach($name in @('COUCHDB_URL','COUCHDB_DATABASE','COUCHDB_USERNAME','COUCHDB_PASSWORD')) {
    if([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        [Environment]::SetEnvironmentVariable($name,[Environment]::GetEnvironmentVariable($name,'User'),'Process')
    }
}
# Keep the primary database unchanged. The test database is retained for inspection.
$env:COUCHDB_DATABASE='qlbanhang_test_' + [guid]::NewGuid().ToString('N')
$env:TEST_WEB_URL='http://localhost:52517'
$root=Split-Path $PSScriptRoot -Parent
$appPath=Join-Path $root 'QuanLyBanHang'
$iis=Start-Process 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList @("/path:$appPath",'/port:52517') -WindowStyle Hidden -PassThru
Push-Location $root
try {
    python -u $TestScript
    $result=$LASTEXITCODE
} finally {
    Pop-Location
    if(!$iis.HasExited) { Stop-Process -Id $iis.Id }
}
exit $result
