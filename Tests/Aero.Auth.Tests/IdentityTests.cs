using TUnit.Core;
using Bogus;
using Aero.Core;
using Aero.Models;
using Aero.Models.Entities;
using Shouldly;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Aero.Auth.Tests;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class IdentityTests : IDisposable
{
    private readonly HttpClient client;
    private readonly UserManager<AeroUser> userManager;
    private readonly IServiceScope scope;
    readonly Faker faker = new();
    private readonly IDocumentSession db;

    public IdentityTests(TestWebAppFactory factory)
    {
        scope = factory.Services.CreateScope();
        client = factory.CreateClient(); 
        userManager = scope.ServiceProvider.GetRequiredService<UserManager<AeroUser>>();
        db = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
    }

    [Test]
    public async Task CanCreateUser()
    {
        var user = new AeroUser()
        {
            Id = Snowflake.NewId(),
            FirstName = faker.Person.FirstName,
            LastName = faker.Person.LastName,
            UserName = faker.Person.UserName,
            Email = faker.Internet.Email(),
            CreatedBy = "system",
            ModifiedBy = "system",
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow,
            ProfilePictureDataUrl = "",
            MiddleName = "",
            UserHandle = [],
            RefreshToken = "",
        };
        var res = await userManager.CreateAsync(user);
        res.Succeeded.ShouldBeTrue();

        var saved = await userManager.FindByEmailAsync(user.Email);
        var saved2 = await userManager.FindByIdAsync(user.Id.ToString());
        var efuser = saved != null ? await db.LoadAsync<AeroUser>(saved.Id) : null;
        var efuseremail = await db.Query<AeroUser>()
            .Where(x => x.Email == user.Email).FirstOrDefaultAsync();
        
    }

    public void Dispose()
    {
        scope.Dispose();
}
}