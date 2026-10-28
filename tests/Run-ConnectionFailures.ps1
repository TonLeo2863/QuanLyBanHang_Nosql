$ErrorActionPreference='Stop'
foreach($name in @('COUCHDB_URL','COUCHDB_DATABASE','COUCHDB_USERNAME','COUCHDB_PASSWORD')) {
    if([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        [Environment]::SetEnvironmentVariable($name,[Environment]::GetEnvironmentVariable($name,'User'),'Process')
    }
}
$originalUrl=$env:COUCHDB_URL
$originalDatabase=$env:COUCHDB_DATABASE
$originalPassword=$env:COUCHDB_PASSWORD
$root=Split-Path $PSScriptRoot -Parent
$appPath=Join-Path $root 'QuanLyBanHang'
$env:TEST_WEB_URL='http://localhost:52518'
$result=0
Push-Location $root
try {
    foreach($mode in @('missing-database','unreachable-server','wrong-password')) {
        $env:COUCHDB_URL=$originalUrl; $env:COUCHDB_DATABASE=$originalDatabase; $env:COUCHDB_PASSWORD=$originalPassword
        if($mode -eq 'missing-database') { $env:COUCHDB_DATABASE='qlbanhang_missing_'+[guid]::NewGuid().ToString('N') }
        elseif($mode -eq 'unreachable-server') {
            $listener=[System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback,0)
            $listener.Start(); $port=$listener.LocalEndpoint.Port; $listener.Stop()
            $env:COUCHDB_URL='http://127.0.0.1:'+$port
        } else { $env:COUCHDB_PASSWORD=[guid]::NewGuid().ToString('N') }
        $iis=Start-Process 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList @(('/path:"{0}"' -f $appPath),'/port:52518') -WindowStyle Hidden -PassThru
        try {
            Write-Output "Test backend failure: $mode"
            python -u tests/Test-LoginFailure.py
            if($LASTEXITCODE -ne 0) { $result=$LASTEXITCODE; break }
            python -u tests/Test-WebSmoke.py
            if($LASTEXITCODE -ne 0) { $result=$LASTEXITCODE; break }
        } finally { if(!$iis.HasExited) { Stop-Process -Id $iis.Id; $iis.WaitForExit() } }
    }
} finally {
    $env:COUCHDB_URL=$originalUrl; $env:COUCHDB_DATABASE=$originalDatabase; $env:COUCHDB_PASSWORD=$originalPassword
    Pop-Location
}
exit $result
