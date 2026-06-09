$ErrorActionPreference = 'Stop'
$logDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$stdout = Join-Path $logDir "api-stdout4.log"
$stderr = Join-Path $logDir "api-stderr4.log"
$apiPath = Join-Path $logDir "backend/src/KnowVault-Core.Api"
dotnet run --project $apiPath 2>&1 | Out-File -FilePath $stdout -Encoding UTF8
