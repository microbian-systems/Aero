

# assuming this is run from the main appication root directory
dotnet ef migrations add InitialAero --context AeroDbContext --project Aero\src\Aero.EfCore --startup-project src\Aero.Cms.Web