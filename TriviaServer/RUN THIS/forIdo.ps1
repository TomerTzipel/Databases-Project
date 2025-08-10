# One-click runner: sets Firebase creds, restores packages, runs the API, opens Swagger (after it starts).

$ErrorActionPreference = "Stop"

# Paths (script is inside: TriviaServer\RUN THIS\)
$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$projDir    = Split-Path -Parent $scriptDir                 # -> ...\TriviaServer
$projPath   = Join-Path $projDir "TriviaServer.csproj"      # FIXED
$keyPath    = Join-Path $projDir "secrets\firebase-analytics.json"
$launch     = Join-Path $projDir "Properties\launchSettings.json"

# Check key
if (-not (Test-Path $keyPath)) {
  Write-Host "Missing Firebase key at:" -ForegroundColor Red
  Write-Host "  $keyPath" -ForegroundColor Yellow
  Pause; exit 1
}

# Env var for this process
$env:GOOGLE_APPLICATION_CREDENTIALS = $keyPath
[Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $keyPath, "Process")

# Show project id
try {
  $projId = (Get-Content -Raw -Path $keyPath | ConvertFrom-Json).project_id
  if ($projId) { Write-Host "Firestore project_id: $projId" -ForegroundColor Cyan }
} catch {}

Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore $projPath | Out-Null

# Figure out HTTP url from launchSettings.json (fallback to 5180)
$swaggerUrl = "http://localhost:5180/swagger"
try {
  if (Test-Path $launch) {
    $ls = Get-Content -Raw -Path $launch | ConvertFrom-Json
    $profiles = $ls.profiles.PSObject.Properties | ForEach-Object { $_.Value }
    $urls = @()
    foreach ($p in $profiles) {
      if ($p.applicationUrl) { $urls += ($p.applicationUrl -split '\s*,\s*') }
    }
    $http = $urls | Where-Object { $_ -match '^http://localhost:\d+' } | Select-Object -First 1
    if ($http) { $swaggerUrl = ($http.TrimEnd('/')) + '/swagger' }
  }
} catch {}

# Open Swagger after a short delay so the server is listening
Start-Job -ScriptBlock {
  param($url)
  Start-Sleep -Seconds 4
  Start-Process $url | Out-Null
} -ArgumentList $swaggerUrl | Out-Null

Write-Host "Starting API... (Swagger will open at $swaggerUrl)" -ForegroundColor Yellow
dotnet run --project $projPath
