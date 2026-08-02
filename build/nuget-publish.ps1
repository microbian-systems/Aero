#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Pushes Aero Framework packages and matching symbols to nuget.org.
.DESCRIPTION
    Pushes each primary .nupkg once. NuGet automatically publishes the matching
    .snupkg when it is present beside the primary package.
    Uses -ApiKey, $env:NUGET_API_KEY_Aero2, $env:NUGET_API_KEY_AERO, or $env:NUGET_API_KEY.
.PARAMETER ApiKey
    NuGet API key. Falls back to env vars if not provided.
#>

param(
    [string]$ApiKey
)

$RepoRoot = Resolve-Path "$PSScriptRoot/.."
$packageDirectory = "$RepoRoot/build/nupkgs"
$nupkgs = Get-ChildItem -LiteralPath $packageDirectory -File -Filter '*.nupkg' -ErrorAction SilentlyContinue |
    Where-Object { $_.Extension -eq '.nupkg' }
if (-not $nupkgs) {
    Write-Host "No .nupkg files found in build/nupkgs/. Run ./build/nuget-pack.ps1 first." -ForegroundColor Yellow
    exit 1
}

if ($ApiKey) {
    Write-Host "Using provided -ApiKey parameter." -ForegroundColor Gray
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY_Aero2)) {
    $ApiKey = $env:NUGET_API_KEY_Aero2
    Write-Host "Using NUGET_API_KEY_Aero2 environment variable." -ForegroundColor Gray
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY_AERO)) {
    $ApiKey = $env:NUGET_API_KEY_AERO
    Write-Host "Using NUGET_API_KEY_AERO environment variable." -ForegroundColor Gray
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {
    $ApiKey = $env:NUGET_API_KEY
    Write-Host "Using NUGET_API_KEY environment variable." -ForegroundColor Gray
} else {
    Write-Host "No API key found. Set NUGET_API_KEY_AERO or NUGET_API_KEY." -ForegroundColor Red
    exit 1
}

Write-Host "Pushing $($nupkgs.Count) packages to nuget.org..." -ForegroundColor Cyan
$failed = 0
foreach ($nupkg in $nupkgs) {
    Write-Host "  $($nupkg.Name)..." -ForegroundColor Gray
    dotnet nuget push $nupkg.FullName --source https://api.nuget.org/v3/index.json --api-key "$ApiKey" --skip-duplicate
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAILED: $($nupkg.Name)" -ForegroundColor Red
        $failed++
    }
}

if ($failed -eq 0) {
    Write-Host "All packages published successfully." -ForegroundColor Green
} else {
    Write-Host "$failed package(s) failed." -ForegroundColor Red
    exit 1
}
