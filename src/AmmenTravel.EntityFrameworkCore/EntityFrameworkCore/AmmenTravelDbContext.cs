using AmmenTravel.Destinos;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace AmmenTravel.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class AmmenTravelDbContext :
    AbpDbContext<AmmenTravelDbContext>,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<DestinoTuristico> Destinos { get; set; }
    public DbSet<Opinion> Opiniones { get; set; }

    #region Entities from the modules

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion

    public AmmenTravelDbContext(DbContextOptions<AmmenTravelDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();
        
        /* Configure your own tables/entities inside here */

        builder.Entity<DestinoTuristico>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "Destinos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            b.Property(x => x.Pais).IsRequired().HasMaxLength(100);
            b.Property(x => x.Poblacion);
            b.Property(x => x.FotoURL).HasMaxLength(1000);
        }); 

        builder.Entity<Opinion>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "Opiniones", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Puntuacion).IsRequired();
            b.Property(x => x.Comentario).IsRequired().HasMaxLength(2000);
            b.Property(x => x.DestinoTuristicoId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
        });
    }
}
