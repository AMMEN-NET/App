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
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Experiencias;

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

    public DbSet<Experiencia> Experiencias { get; set; }

    #region Entities from the modules

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<ListaFavorito> ListasFavoritos { get; set; }
    public DbSet<LineaListaFavorito> LineasListasFavoritos { get; set; }

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
            b.Property(x => x.Latitud).IsRequired();
            b.Property(x => x.Longitud).IsRequired();
            b.Property(x => x.IdExterno).HasMaxLength(100);

        });

        builder.Entity<Opinion>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "Opiniones", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Puntuacion).IsRequired();
            b.Property(x => x.Comentario).IsRequired().HasMaxLength(2000);
            b.Property(x => x.DestinoTuristicoId).IsRequired();
            b.Property(x => x.UserId).IsRequired();

            // --- AGREGAR ESTO ---
            // Define la relación explícita: Una Opinión tiene UN Destino
            b.HasOne(x => x.DestinoTuristico)
             .WithMany() // Un destino puede tener muchas opiniones (aunque no esté en la clase Destino)
             .HasForeignKey(x => x.DestinoTuristicoId)
             .OnDelete(DeleteBehavior.Cascade); // O Restrict, según prefieras
        });

        /* Configuración de LISTA DE FAVORITOS (Contenedor) */
        builder.Entity<ListaFavorito>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "ListasFavoritos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();
            // Como es única por usuario, EF ya usa el UserId por la interfaz IUserOwned
        });

        /* Configuración de LINEAS DE FAVORITOS */
        builder.Entity<LineaListaFavorito>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "LineasListasFavoritos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();

            // RELACIÓN 1: Una línea pertenece a UNA Lista
            b.HasOne(x => x.ListaFavorito)
                .WithMany() // Una lista tiene muchas líneas (aunque no lo definamos en la clase Lista, EF lo entiende)
                .HasForeignKey(x => x.ListaFavoritoId)
                .OnDelete(DeleteBehavior.Cascade); // IMPORTANTE: Si un día borras la lista, se borran las líneas.

            // RELACIÓN 2: Una línea apunta a UN Destino
            b.HasOne(x => x.DestinoTuristico)
                .WithMany()
                .HasForeignKey(x => x.DestinoTuristicoId)
                .OnDelete(DeleteBehavior.Cascade); // Si borras el destino (ej. París), desaparece de los favoritos de todos.
        });

        builder.Entity<Experiencia>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "Experiencias", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Comentario).IsRequired().HasMaxLength(1000);
            b.Property(x => x.Valoracion).IsRequired();

            // --- AGREGAR ESTO PARA QUE FUNCIONE EL INCLUDE/JOIN ---
            b.HasOne(x => x.Destino)
             .WithMany()
             .HasForeignKey(x => x.DestinoId)
             .IsRequired();

            b.HasIndex(x => x.DestinoId);
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
