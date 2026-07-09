#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Pushes Aero Framework .nupkg and .snupkg to nuget.org.
.DESCRIPTION
    Uses $env:NUGET_API_KEY_AERO (preferred), $env:NUGET_API_KEY, or -ApiKey parameter.
.PARAMETER ApiKey
    NuGet API key. Falls back to env vars if not provided.
#>

param(
    [string]$ApiKey
)

$RepoRoot = Resolve-Path "$PSScriptRoot/.."
$nupkgs = Get-ChildItem "$RepoRoot/build/nupkgs/*.nupkg" -ErrorAction SilentlyContinue
if (-not $nupkgs) {
    Write-Host "No .nupkg files found in build/nupkgs/. Run ./build/nuget-pack.ps1 first." -ForegroundColor Yellow
    exit 1
}

if ($ApiKey) {
    Write-Host "Using provided -ApiKey parameter." -ForegroundColor Gray
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY_AERO)) {
    $ApiKey = $env:NUGET_API_KEY_AERO
    Write-Host "Using NUGET_API_KEY_AERO environment variable." -ForegroundColor Gray
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY_Aero2)) {
    $ApiKey = $env:NUGET_API_KEY_Aero2
    Write-Host "Using NUGET_API_KEY_Aero2 environment variable." -ForegroundColor Gray
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

# Push symbol packages (.snupkg)
$snupkgs = Get-ChildItem "$RepoRoot/build/nupkgs/*.snupkg" -ErrorAction SilentlyContinue
if ($snupkgs) {
    Write-Host "Pushing $($snupkgs.Count) symbol packages..." -ForegroundColor Cyan
    foreach ($snupkg in $snupkgs) {
        Write-Host "  $($snupkg.Name)..." -ForegroundColor Gray
        dotnet nuget push $snupkg.FullName --source https://api.nuget.org/v3/index.json --api-key "$apiKey" --skip-duplicate
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  FAILED: $($snupkg.Name)" -ForegroundColor Red
            $failed++
        }
    }
}

if ($failed -eq 0) {
    Write-Host "All packages published successfully." -ForegroundColor Green
} else {
    Write-Host "$failed package(s) failed." -ForegroundColor Red
    exit 1
}
