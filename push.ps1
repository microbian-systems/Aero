#!/usr/bin/env pwsh

Write-Host "=== Step 1: Pack ===" -ForegroundColor Cyan
& "$PSScriptRoot/build/nuget-pack.ps1"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n=== Step 2: Publish ===" -ForegroundColor Cyan
& "$PSScriptRoot/build/nuget-publish.ps1"
