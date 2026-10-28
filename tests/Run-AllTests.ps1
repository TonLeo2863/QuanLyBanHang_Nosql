$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Push-Location $root
try {
    $msbuild='C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
    if(!(Test-Path -LiteralPath $msbuild)) { $msbuild=(Get-Command msbuild -ErrorAction Stop).Source }
    & $msbuild QuanLyBanHang.sln /t:Build /p:Configuration=Debug /p:MvcBuildViews=true /verbosity:minimal /nologo
    if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    foreach($script in @('Run-SmokeTests.ps1','Run-WorkflowUnit.ps1')) {
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot $script)
        if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    foreach($script in @('tests/Test-Initializer.py','tests/Test-ConnectionDiagnostic.py')) {
        python -u $script
        if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    foreach($script in @('tests/Test-P2.py','tests/Test-Mango.py','tests/Test-Backup.py')) {
        & powershell -NoProfile -ExecutionPolicy Bypass -File scripts/With-CouchEnvironment.ps1 -Script $script
        if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    & powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-LiveIntegration.ps1
    if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & powershell -NoProfile -ExecutionPolicy Bypass -File tests/Run-ConnectionFailures.ps1
    if($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Output 'PASS complete automated suite; GUI/deployment checks remain separate'
} finally { Pop-Location }
