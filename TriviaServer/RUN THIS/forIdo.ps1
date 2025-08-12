# One-click runner: validates the Firestore key, sets env var for THIS process,
# (optionally) clears user/machine env vars, restores packages, runs the API, opens Swagger.

param(
  [switch]$ClearUserEnv = $true,
  [switch]$ClearMachineEnv = $false,
  [switch]$SyncClock = $false
)

$ErrorActionPreference = "Stop"

function Write-Info($msg) { Write-Host $msg -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host $msg -ForegroundColor Yellow }
function Write-Err ($msg) { Write-Host $msg -ForegroundColor Red }

# Paths (script is inside: TriviaServer\RUN THIS\)
$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$projDir    = Split-Path -Parent $scriptDir                 # -> ...\TriviaServer
$projPath   = Join-Path $projDir "TriviaServer.csproj"
$keyPath    = Join-Path $projDir "secrets\firebase-analytics.json"
$launch     = Join-Path $projDir "Properties\launchSettings.json"

# --- Validate key exists ---
if (-not (Test-Path $keyPath)) {
  Write-Err "Missing Firebase service-account key:"
  Write-Warn "  $keyPath"
  Pause; exit 1
}

# --- Parse key & sanity checks (NO secrets printed) ---
try {
  $raw = Get-Content -Raw -Path $keyPath -ErrorAction Stop
  $j = $raw | ConvertFrom-Json
} catch {
  Write-Err "Key file is not valid JSON: $keyPath"
  Pause; exit 1
}

$projectId    = $j.project_id
$saType       = $j.type
$clientEmail  = $j.client_email
$keyId        = $j.private_key_id
$privateKey   = $j.private_key

$ok = $true
if (-not $projectId)   { Write-Err "Missing 'project_id' in key JSON."; $ok = $false }
if ($saType -ne "service_account") { Write-Err "'type' must be 'service_account'."; $ok = $false }
if (-not $clientEmail) { Write-Err "Missing 'client_email' in key JSON."; $ok = $false }
if (-not $keyId)       { Write-Err "Missing 'private_key_id' in key JSON."; $ok = $false }
if (-not $privateKey -or ($privateKey -notmatch "BEGIN PRIVATE KEY")) {
  Write-Err "Invalid 'private_key' content (PEM header not found)."; $ok = $false
}

if (-not $ok) { Pause; exit 1 }

Write-Info "Firestore project_id: $projectId"
Write-Info "Service Account:     $clientEmail"
Write-Info "Key ID:              $keyId"
Write-Host  "" 

# --- Optional: clear any stale env vars that could hijack runs ---
if ($ClearUserEnv) {
  try {
    [Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $null, "User")
    Write-Host "Cleared User env var GOOGLE_APPLICATION_CREDENTIALS" -ForegroundColor DarkGray
  } catch {}
}
if ($ClearMachineEnv) {
  try {
    # Requires admin
    [Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $null, "Machine")
    Write-Host "Cleared Machine env var GOOGLE_APPLICATION_CREDENTIALS" -ForegroundColor DarkGray
  } catch {}
}

# --- Set env var for THIS process only ---
$env:GOOGLE_APPLICATION_CREDENTIALS = $keyPath
[Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $keyPath, "Process")
Write-Info "Using key file: $keyPath"
Write-Host ""

# --- Optional: resync system clock (helps avoid token 'invalid_grant' when skewed) ---
if ($SyncClock) {
  try {
    Write-Host "Syncing system clock..." -ForegroundColor Gray
    w32tm /resync | Out-Null
  } catch {}
}

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
