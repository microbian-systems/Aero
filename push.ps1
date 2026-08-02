#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Local pack + push to NuGet.
.DESCRIPTION
    Packs all Aero Framework library projects and pushes them to nuget.org.
    Uses $env:NUGET_API_KEY_Aero2 (preferred), $env:NUGET_API_KEY_AERO, or $env:NUGET_API_KEY.
.EXAMPLE
    ./push.ps1
#>

$apiKeySource = $null
$apiKey = $null

if (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY_Aero2)) {
    $apiKey = $env:NUGET_API_KEY_Aero2
    $apiKeySource = "NUGET_API_KEY_Aero2"
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY_AERO)) {
    $apiKey = $env:NUGET_API_KEY_AERO
    $apiKeySource = "NUGET_API_KEY_AERO"
} elseif (-not [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {
    $apiKey = $env:NUGET_API_KEY
    $apiKeySource = "NUGET_API_KEY"
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "No NuGet API key found." -ForegroundColor Red
    Write-Host "Set: `$env:NUGET_API_KEY_Aero2 = 'your-key-here'" -ForegroundColor Yellow
    Write-Host "Set: `$env:NUGET_API_KEY_AERO = 'your-key-here'" -ForegroundColor Yellow
    Write-Host "Or:  `$env:NUGET_API_KEY = 'your-key-here'" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using $apiKeySource environment variable." -ForegroundColor Gray

Write-Host "=== Step 1: Pack ===" -ForegroundColor Cyan
& "$PSScriptRoot/build/nuget-pack.ps1"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n=== Step 2: Publish ===" -ForegroundColor Cyan
& "$PSScriptRoot/build/nuget-publish.ps1" -ApiKey $apiKey
