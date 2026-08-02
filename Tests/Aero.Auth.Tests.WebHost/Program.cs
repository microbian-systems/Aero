using Aero.Auth.Controllers;
using Aero.Models;
using Aero.Models.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
    //.AddApplicationPart(typeof(AuthController).Assembly); // AuthController is obsolete
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();


var env = builder.Environment;
var config = builder.Configuration;
//builder.Services.AddElectraAuthentication(env, config);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

var user = new AeroUser()
{
    FirstName = "Test",
    LastName = "User",
    UserName = "testuser",
    Email = "testW@user.com",
    CreatedBy = "system",
    ModifiedBy = "system",
    CreatedOn = DateTimeOffset.UtcNow,
    ModifiedOn = DateTimeOffset.UtcNow,
    ProfilePictureDataUrl = "",
    MiddleName = "",
    UserHandle = [],
    RefreshToken = "",
    // Profile = new AeroUserProfile
    // {
    //     Username = "testuser",
    //     Headline = "💩 My Headline 💩",
    //     Location = "Los Angeles, CA",
    //     Bio = "This is my bio",
    //     Website = "https://example.com",
    //     CreatedBy = "system",
    //     ModifiedBy = "system",
    //     CreatedOn = DateTimeOffset.UtcNow,
    //     ModifiedOn = DateTimeOffset.UtcNow,
    // },
    // UserSettings = new UserSettingsModel()
    // {
    //     Stuff = "{}",
    //     CreatedBy = "system",
    //     ModifiedBy = "system",
    //     CreatedOn = DateTimeOffset.UtcNow,
    //     ModifiedOn = DateTimeOffset.UtcNow,
    // }
};

// ... existing setup logic ...


        /// <summary>
    /// Represents a class for Program.
    /// </summary>
public partial class Program;
