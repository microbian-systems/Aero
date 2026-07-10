using Aero.Core;
using Aero.Core.Data;
using Aero.Core.Entities;

namespace Aero.EfCore;


/// <summary>
/// Represents a class for AeroDbContext.
/// </summary>
public class AeroDbContext(DbContextOptions<AeroDbContext> options) : DbContext(options)
{
        /// <summary>
    /// Gets or sets the Ai Usage Logs.
    /// </summary>
public DbSet<AiUsageLog> AiUsageLogs { get; set; }
        /// <summary>
    /// Gets or sets the Addresses.
    /// </summary>
public DbSet<AddressModel> Addresses { get; set; }
        /// <summary>
    /// Gets or sets the Api Accounts.
    /// </summary>
public DbSet<ApiAccountModel> ApiAccounts { get; set; }
        /// <summary>
    /// Gets or sets the Api Claims.
    /// </summary>
public DbSet<ApiClaimsModel> ApiClaims { get; set; }
        /// <summary>
    /// Gets or sets the Cities.
    /// </summary>
public DbSet<CityModel> Cities { get; set; }
        /// <summary>
    /// Gets or sets the Countries.
    /// </summary>
public DbSet<CountryModel> Countries { get; set; }
        /// <summary>
    /// Gets or sets the States.
    /// </summary>
public DbSet<StateModel> States { get; set; }
    //public DbSet<UserPasskeys> UserPasskeys { get; set; }

    // Authentication token management
        /// <summary>
    /// Gets or sets the Refresh Tokens.
    /// </summary>
public DbSet<RefreshToken> RefreshTokens { get; set; }
        /// <summary>
    /// Gets or sets the Jwt Signing Keys.
    /// </summary>
public DbSet<JwtSigningKey> JwtSigningKeys { get; set; }

        /// <summary>
    /// OnModelCreating method.
    /// </summary>
protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureDecimalPrecision(builder);
        ModelApiAuth(builder);
        ConfigureAuthenticationTokens(builder);
    }


    private void ConfigureDecimalPrecision(ModelBuilder builder)
    {
        foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }


        /// <summary>
    /// ModelApiAuth method.
    /// </summary>
protected void ModelApiAuth(ModelBuilder builder)
    {

        builder.Entity<ApiAccountModel>()
            .ToTable("ApiAccounts", schema: Schemas.Aero);
        builder.Entity<ApiAccountModel>()
            .HasKey(i => i.Id);
        builder.Entity<ApiAccountModel>()
            .HasIndex(i => i.ApiKey, "ix_apikey")
            .IsUnique();
        builder.Entity<ApiAccountModel>()
            .HasIndex(i => i.Email);
        builder.Entity<ApiAccountModel>()
            .HasIndex(i => i.Enabled);
        builder.Entity<ApiAccountModel>()
            .HasIndex(i => i.CreatedOn);
        builder.Entity<ApiAccountModel>()
            .HasIndex(i => i.ModifiedOn);

        builder.Entity<ApiClaimsModel>()
            .HasIndex(i => i.ClaimKey);
        builder.Entity<ApiClaimsModel>()
            .HasIndex(i => i.ClaimValue);

        builder.Entity<ApiAccountModel>()
            .HasMany<ApiClaimsModel>()
            .WithOne();

        builder.Entity<ApiClaimsModel>()
            .ToTable("ApiClaims", schema: Schemas.Aero)
            .HasKey(pk => pk.Id);
        builder.Entity<ApiClaimsModel>()
            .HasOne<ApiAccountModel>()
            .WithMany(m => m.Claims)
            .HasForeignKey(m => m.AccountId);

        builder.Entity<ApiAccountModel>(e =>
        {
            e.Property(rt => rt.CreatedOn).ValueGeneratedOnAdd();
            e.Property(rt => rt.ModifiedOn).ValueGeneratedOnAddOrUpdate();
        });

        builder.Entity<ApiClaimsModel>(e =>
        {
            e.Property(rt => rt.CreatedOn).ValueGeneratedOnAdd();
            e.Property(rt => rt.ModifiedOn).ValueGeneratedOnAddOrUpdate();
        });
    }

    private void ConfigureAuthenticationTokens(ModelBuilder builder)
    {
        // Refresh tokens for session management
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", schema: Schemas.Auth);
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.UserId);
            entity.HasIndex(rt => rt.TokenHash).IsUnique();
            entity.HasIndex(rt => rt.ExpiresAt);
            entity.HasIndex(rt => rt.RevokedAt);
            entity.Property(rt => rt.CreatedOn).ValueGeneratedOnAdd();
            entity.Property(rt => rt.ModifiedOn).ValueGeneratedOnAddOrUpdate();

        });

        // JWT signing keys for key rotation
        builder.Entity<JwtSigningKey>(entity =>
        {
            entity.ToTable("JwtSigningKeys", schema: Schemas.Auth);
            entity.HasKey(jsk => jsk.Id);
            entity.HasIndex(jsk => jsk.KeyId).IsUnique();
            // Unique constraint: only one key can be current
            entity.HasIndex(jsk => jsk.IsCurrentSigningKey).IsUnique();
            entity.Property(jsk => jsk.CreatedOn).ValueGeneratedOnAdd();
            entity.Property(jsk => jsk.ModifiedOn).ValueGeneratedOnAddOrUpdate();
        });
    }

        /// <summary>
    /// SaveChanges method.
    /// </summary>
public override int SaveChanges()
    {
        AssignSnowflakeIds();
        return base.SaveChanges();
    }

        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AssignSnowflakeIds();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AssignSnowflakeIds()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry is { State: EntityState.Added, Entity: IEntity { Id: 0 } entity })
            {
                entity.Id = Snowflake.NewId();
            }
        }
    }
}



//private void ConfigureIdentityTables(ModelBuilder builder)
//{
// builder.Entity<AeroUser>(entity =>
// {
//     entity.ToTable("Users", schema: Schemas.Auth);
//     
//     // Auditing - use ValueGeneratedOnAdd for server-side defaults
//     entity.Property(x => x.CreatedOn).ValueGeneratedOnAdd();
//     entity.Property(x => x.ModifiedOn).ValueGeneratedOnAdd();
//     entity.HasIndex(x => x.CreatedOn);
//     entity.HasIndex(x => x.ModifiedOn);
//     entity.HasIndex(x => x.CreatedBy);
//     entity.HasIndex(x => x.ModifiedBy);
//     
//     // Profile relationship - ONLY CONFIGURE ONCE
//     // entity.HasOne(x => x.Profile)
//     //     .WithOne()
//     //     .HasForeignKey<AeroUserProfile>(x => x.Userid)
//     //     .OnDelete(DeleteBehavior.Cascade);
//     
//     entity.HasIndex(i => i.UserProfileId).IsUnique();
// });
//
// builder.Entity<AeroRole>(entity =>
// {
//     entity.ToTable("Roles", schema: Schemas.Auth);
// });
//
// builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", schema: Schemas.Auth);
// builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", schema: Schemas.Auth);
// builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", schema: Schemas.Auth);
// builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", schema: Schemas.Auth);
// builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", schema: Schemas.Auth);
//}