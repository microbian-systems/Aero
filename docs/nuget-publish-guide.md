# NuGet Publishing Setup Guide

Copy these files to your new project and follow the steps below.

## Files to Copy

| File | Purpose |
|---|---|
| `build/nuget-pack.ps1` | Packs all library projects into `.nupkg` files |
| `build/nuget-publish.ps1` | Pushes `.nupkg` files to nuget.org |
| `push.ps1` | Runs pack + publish in one command |

**Optional (for CI/CD):**
| File | Purpose |
|---|---|
| `.github/workflows/nuget-preview.yml` | Auto-publish on push to `develop` |
| `.github/workflows/nuget-release.yml` | Gated release from `main` |

## Setup Steps

### 1. `Directory.Build.props` (in your `src/` directory)

```xml
<PropertyGroup Label="Package information">
    <Version>0.0.1-alpha</Version>
    <Authors>Your Name</Authors>
    <Company>Your Company</Company>
    <Copyright>Copyright (c) 2026 Your Company</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageIcon>package-icon.png</PackageIcon>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageProjectUrl>https://github.com/your-org/your-repo</PackageProjectUrl>
    <RepositoryUrl>https://github.com/your-org/your-repo</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>

<ItemGroup>
    <None Include="$(MSBuildThisFileDirectory)../assets/package-icon.png" Pack="true" PackagePath="\" />
    <None Include="$(MSBuildThisFileDirectory)../README.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

**Key:**
- `$(MSBuildThisFileDirectory)../` resolves from `src/` to repo root — use this pattern for `<None>` paths
- All projects under `src/` inherit these settings automatically

### 2. Csproj Changes

Add to every library project's `.csproj`:

```xml
<PropertyGroup>
    <IsPackable>true</IsPackable>
    <PackageId>Your.Package.Name</PackageId>
</PropertyGroup>
```

For test/console projects, set:
```xml
<IsPackable>false</IsPackable>
```

`dotnet pack` auto-converts `<ProjectReference>` to NuGet dependencies — no extra work needed.

### 3. Update `build/nuget-pack.ps1`

Replace the `$libProjects` array with your project paths:

```powershell
$libProjects = @(
    "$RepoRoot/src/Your.Project1"
    "$RepoRoot/src/Your.Project2"
    "$RepoRoot/src/Your.Project3"
)
```

### 4. NuGet API Key

1. Go to **nuget.org** → avatar → **API Keys** → **Create**
2. Scope: **Push** → Glob: `Your.Prefix.*`
3. Copy the key (shows once)

### 5. Publish

```powershell
$env:NUGET_API_KEY = 'oy2...your-key'
./push.ps1
```

## Git Flow (recommended)

| Branch | Releases | Version |
|---|---|---|
| `develop` | Preview (auto-push via CI) | `0.0.1-alpha.{build#}` |
| `main` | Stable (gated) | `0.0.1` (no suffix) |

## Requirements

- .NET SDK 10.0+
- PowerShell 7+
- NuGet.org account (free)
