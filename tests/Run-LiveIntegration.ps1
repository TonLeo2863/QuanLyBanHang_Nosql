# Run under the Windows account that owns the CouchDB User variables.
# No persisted environment changes, credentials in output, or existing IIS process termination.
$ErrorActionPreference = 'Stop'
foreach ($name in @('COUCHDB_URL','COUCHDB_DATABASE','COUCHDB_USERNAME','COUCHDB_PASSWORD')) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        [Environment]::SetEnvironmentVariable($name, [Environment]::GetEnvironmentVariable($name, 'User'), 'Process')
    }
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) { throw "Thiếu biến $name" }
}
$root = Split-Path $PSScriptRoot -Parent
$appPath = Join-Path $root 'QuanLyBanHang'
$env:TEST_WEB_URL = 'http://localhost:52516'
$iis = Start-Process -FilePath 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList @("/path:$appPath", '/port:52516') -WindowStyle Hidden -PassThru
$testExit = 1
Push-Location $root
try {
    python -u tests/Test-LiveIntegration.py
    $testExit = $LASTEXITCODE
    if ($testExit -eq 0) {
        Stop-Process -Id $iis.Id
        $iis.WaitForExit()
        $savedDatabase = $env:COUCHDB_DATABASE
        $savedPassword = $env:COUCHDB_PASSWORD
        try {
            foreach ($mode in @('missing-database', 'wrong-password')) {
                $env:COUCHDB_DATABASE = $savedDatabase
                $env:COUCHDB_PASSWORD = $savedPassword
                if ($mode -eq 'missing-database') { $env:COUCHDB_DATABASE = 'test_missing_' + [guid]::NewGuid().ToString('N') }
                else { $env:COUCHDB_PASSWORD = [guid]::NewGuid().ToString('N') }
                $iis = Start-Process -FilePath 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList @("/path:$appPath", '/port:52516') -WindowStyle Hidden -PassThru
                Write-Output "Kiểm thử IIS: $mode (cấu hình chỉ đổi trong tiến trình)"
                python -u tests/Test-WebSmoke.py
                if ($LASTEXITCODE -ne 0) { $testExit = $LASTEXITCODE; break }
                Stop-Process -Id $iis.Id
                $iis.WaitForExit()
            }
        } finally {
            $env:COUCHDB_DATABASE = $savedDatabase
            $env:COUCHDB_PASSWORD = $savedPassword
        }
    }
} finally {
    Pop-Location
    if (!$iis.HasExited) { Stop-Process -Id $iis.Id }
}
exit $testExit
