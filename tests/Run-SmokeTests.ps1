$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$bin = Join-Path $root 'QuanLyBanHang/bin'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$output = Join-Path $bin 'SmokeTests.exe'
& $compiler /nologo "/out:$output" "/r:$bin/QuanLyBanHang.dll" "/r:$bin/Newtonsoft.Json.dll" "/r:$bin/System.Web.Mvc.dll" /r:System.Net.Http.dll /r:System.Web.dll /r:System.ComponentModel.DataAnnotations.dll (Join-Path $PSScriptRoot 'SmokeTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Biên dịch kiểm thử thất bại.' }
Copy-Item (Join-Path $PSScriptRoot 'SmokeTests.config') ($output + '.config')
& $output
if ($LASTEXITCODE -ne 0) { throw 'Kiểm thử thất bại.' }
