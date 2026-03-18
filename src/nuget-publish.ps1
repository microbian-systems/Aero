# Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

# Create all NuGet packages
Get-ChildItem -Path . -Filter "*.nupkg" -Recurse | ForEach-Object {
    Write-Host "Processing project $($_.FullName) ..."
    # dotnet nuget push $_.FullName --api-key qz2jga8pl3dvn2akksyquwcs9ygggg4exypy3bhxy6w6x6 --source https://api.nuget.org/v3/index.json
    dotnet nuget push $_.FullName --api-key qz2jga8pl3dvn2akksyquwcs9ygggg4exypy3bhxy6w6x6 --source https://api.nuget.org/v3/index.json
}
