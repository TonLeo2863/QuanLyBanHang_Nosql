param([Parameter(Mandatory=$true)][string]$Script, [string[]]$ScriptArguments = @())
$ErrorActionPreference = 'Stop'
foreach ($name in @('COUCHDB_URL','COUCHDB_DATABASE','COUCHDB_USERNAME','COUCHDB_PASSWORD')) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        [Environment]::SetEnvironmentVariable($name,[Environment]::GetEnvironmentVariable($name,'User'),'Process')
    }
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) { throw "Thiếu biến $name" }
}
python -u $Script @ScriptArguments
exit $LASTEXITCODE
