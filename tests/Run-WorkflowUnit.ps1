$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$bin=Join-Path $root 'QuanLyBanHang/bin'
$output=Join-Path $bin 'WorkflowTests.exe'
& "$bin/roslyn/csc.exe" /nologo "/out:$output" "/r:$bin/QuanLyBanHang.dll" "/r:$bin/Newtonsoft.Json.dll" "/r:$bin/System.Web.Mvc.dll" /r:System.Net.Http.dll /r:System.Web.dll /r:System.ComponentModel.DataAnnotations.dll (Join-Path $PSScriptRoot 'WorkflowTests.cs')
if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item (Join-Path $PSScriptRoot 'SmokeTests.config') ($output+'.config')
& $output
exit $LASTEXITCODE
