var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Aero_Cms_Manager_UI>("aero-cms-manager-ui");

builder.AddProject<Projects.Aero_Cms_Manager_UI_Web>("aero-cms-manager-ui-web");

builder.Build().Run();
