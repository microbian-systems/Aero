#!/usr/bin/env pwsh

$apiKey = $env:GITHUB_API_KEY_Aero2 ?? $env:NUGET_API_KEY
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "NUGET_API_KEY environment variable is not set." -ForegroundColor Red
    Write-Host "Set: `$env:GITHUB_API_KEY_Aero2 = 'your-Aero2-key'" -ForegroundColor Yellow
    Write-Host "Or:  `$env:NUGET_API_KEY = 'your-key-here'" -ForegroundColor Yellow
    exit 1
}

Write-Host "=== Step 1: Pack ===" -ForegroundColor Cyan
& "$PSScriptRoot/build/nuget-pack.ps1"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n=== Step 2: Publish ===" -ForegroundColor Cyan
& "$PSScriptRoot/build/nuget-publish.ps1"
