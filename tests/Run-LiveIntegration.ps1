# Authentication/workflow supersede the old anonymous CRUD assumptions.
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Run-SystemTests.ps1') -TestScript 'tests/Test-LiveIntegration.py'
exit $LASTEXITCODE
