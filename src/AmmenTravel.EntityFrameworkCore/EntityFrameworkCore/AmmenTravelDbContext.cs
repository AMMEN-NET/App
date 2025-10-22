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
using Volo.Abp.Users;
using System;
using System.Linq;
using System.Reflection;
using AmmenTravel.Common;

namespace AmmenTravel.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class AmmenTravelDbContext :
    AbpDbContext<AmmenTravelDbContext>,
    IIdentityDbContext
{
    /* DbSets para tus entidades */
    public DbSet<DestinoTuristico> Destinos { get; set; }
    public DbSet<Opinion> Opiniones { get; set; }

    #region Entities from the modules

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion

    // Hacemos nullable para poder tener un constructor solo con DbContextOptions (diseño/migraciones)
    private readonly ICurrentUser? _currentUser;

    // Propiedad de instancia usada por el HasQueryFilter (permite cambiar por instancia de DbContext)
    private Guid? CurrentUserId { get; set; }

    // Constructor principal usado en runtime (inyección de ICurrentUser)
    public AmmenTravelDbContext(DbContextOptions<AmmenTravelDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
        CurrentUserId = _currentUser?.Id;
    }

    // Constructor adicional para tiempo de diseño / migraciones (IDesignTimeDbContextFactory)
    // Deja _currentUser nulo y CurrentUserId a null para evitar dependencias en el factory.
    public AmmenTravelDbContext(DbContextOptions<AmmenTravelDbContext> options)
        : base(options)
    {
        _currentUser = null;
        CurrentUserId = null;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Configuración de módulos ABP */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configuración de tus entidades */
        builder.Entity<DestinoTuristico>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "Destinos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();
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

        /* Filtro global para entidades que implementen IUserOwned */
        var userOwnedTypes = builder.Model.GetEntityTypes()
            .Where(t => typeof(IUserOwned).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var et in userOwnedTypes)
        {
            var method = typeof(AmmenTravelDbContext)
                .GetMethod(nameof(ApplyUserFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(et.ClrType);

            method.Invoke(this, new object[] { builder });
        }
    }

    private void ApplyUserFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, IUserOwned
    {
        // Usamos la propiedad de instancia CurrentUserId (EF parametriza por instancia).
        builder.Entity<TEntity>().HasQueryFilter(e => e.UserId == CurrentUserId);
    }
}
