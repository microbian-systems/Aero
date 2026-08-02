#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packs all Aero Framework library projects into NuGet packages.
.DESCRIPTION
    Builds all library projects in Release mode and produces .nupkg files
    in the build/nupkgs/ directory. Package version and symbol settings come
    from src/Directory.Build.props. Skips Aero.Cloudflare (EXE) and test projects.
.PARAMETER VersionSuffix
    Optional SemVer prerelease suffix that overrides VersionSuffix from
    src/Directory.Build.props. An explicitly empty value produces a stable package.
.PARAMETER OutputDir
    Output directory for nupkg files. Default: build/nupkgs.
.PARAMETER Configuration
    Build configuration. Default: Release.
.EXAMPLE
    ./build/nuget-pack.ps1
    Packs all libraries with the version from src/Directory.Build.props.
.NOTES
    The develop CI workflow invokes this script for every push and packs all
    libraries, including when a commit changes only documentation or other
    non-package files. This remains intentional until path filtering is added.
#>

param(
    [AllowEmptyString()]
    [string]$VersionSuffix,
    [string]$OutputDir = "",
    [string]$Configuration = "Release"
)

$RepoRoot = Resolve-Path "$PSScriptRoot/.."
$OutputDir = $(if ($OutputDir) { $OutputDir } else { "$RepoRoot/build/nupkgs" })

Write-Host "=== Aero NuGet Pack Script ===" -ForegroundColor Cyan
Write-Host "Repo:     $RepoRoot" -ForegroundColor Gray
Write-Host "Output:   $OutputDir" -ForegroundColor Gray
Write-Host "Config:   $Configuration" -ForegroundColor Gray

$versionArgs = @()
if ($PSBoundParameters.ContainsKey('VersionSuffix')) {
    $versionArgs += "-p:VersionSuffix=$VersionSuffix"
}

# Ensure output directory exists and contains no artifacts from earlier versions.
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$staleArtifacts = Get-ChildItem -LiteralPath $OutputDir -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name.StartsWith('Aero.') -and $_.Extension -in '.nupkg', '.snupkg' }
foreach ($artifact in $staleArtifacts) {
    Remove-Item -LiteralPath $artifact.FullName -Force
}

# Libraries to pack (excluding Aero.Cloudflare - EXE, and test projects)
$libProjects = @(
    "$RepoRoot/src/Aero.Core"
    "$RepoRoot/src/Aero.Actors.Abstractions"
    "$RepoRoot/src/Aero.Actors"
    "$RepoRoot/src/Aero.Models"
    "$RepoRoot/src/Aero.Events"
    "$RepoRoot/src/Aero.Modular"
    "$RepoRoot/src/Aero.Secrets"
    "$RepoRoot/src/Aero.SignalR"
    "$RepoRoot/src/Aero.Social"
    "$RepoRoot/src/Aero.Social/Twitter.Client"
    "$RepoRoot/src/Aero.Validators"
    "$RepoRoot/src/Aero.Caching"
    "$RepoRoot/src/Aero.Core.Ai"
    "$RepoRoot/src/Aero.EfCore"
    "$RepoRoot/src/Aero.Marten"
    "$RepoRoot/src/Aero.Services"
    "$RepoRoot/src/Aero.Web"
    "$RepoRoot/src/Aero.Auth"
)

$failed = @()

foreach ($proj in $libProjects) {
    $csproj = Get-ChildItem "$proj/*.csproj" | Select-Object -First 1 -ExpandProperty FullName
    if (-not $csproj) {
        Write-Host "WARN: Project not found, skipping: $proj" -ForegroundColor Yellow
        continue
    }

    $projName = (Get-Item $csproj).BaseName
    Write-Host "  Packing: $projName..." -ForegroundColor Cyan
    $output = dotnet pack $csproj -c $Configuration -o $OutputDir @versionArgs 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAILED: $(Split-Path $proj -Leaf)" -ForegroundColor Red
        $failed += $proj
        $output | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkRed }
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
$count = (Get-ChildItem -LiteralPath $OutputDir -File -Filter '*.nupkg' -ErrorAction SilentlyContinue |
    Where-Object { $_.Extension -eq '.nupkg' }).Count
Write-Host "Packages created: $count" -ForegroundColor Green
Write-Host "Location: $OutputDir" -ForegroundColor Green

if ($failed.Count -gt 0) {
    Write-Host "Failed: $($failed.Count)" -ForegroundColor Red
    exit 1
}
