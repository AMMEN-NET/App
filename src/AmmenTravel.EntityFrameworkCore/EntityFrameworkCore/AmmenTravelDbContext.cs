using AmmenTravel.Common;
using AmmenTravel.Destinos;
using AmmenTravel.Estadisticas;
using AmmenTravel.Experiencias;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Notificaciones;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
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
    public DbSet<HistorialBusqueda> HistorialBusquedas { get; set; }
    public DbSet<RegistroApiExterna> RegistrosApiExterna { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }

    public DbSet<HistorialNotificacionEvento> HistorialNotificacionesEventos { get; set; }
    public DbSet<PreferenciasNotificacion> PreferenciasNotificaciones { get; set; }
    public DbSet<ColaResumenSemanalEmail> ColaResumenSemanalEmails { get; set; }


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

    // Hacemos nullable para poder tener un constructor solo con DbContextOptions (dise�o/migraciones)
    private readonly ICurrentUser? _currentUser;

    // Propiedad de instancia usada por el HasQueryFilter (permite cambiar por instancia de DbContext)
    private Guid? CurrentUserId { get; set; }

    // Constructor principal usado en runtime (inyecci�n de ICurrentUser)
    public AmmenTravelDbContext(DbContextOptions<AmmenTravelDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
        CurrentUserId = _currentUser?.Id;
    }

    // Constructor adicional para tiempo de dise�o / migraciones (IDesignTimeDbContextFactory)
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

        /* Configuraci�n de m�dulos ABP */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configuraci�n de tus entidades */
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
            // Define la relaci�n expl�cita: Una Opini�n tiene UN Destino
            b.HasOne(x => x.DestinoTuristico)
             .WithMany() // Un destino puede tener muchas opiniones (aunque no est� en la clase Destino)
             .HasForeignKey(x => x.DestinoTuristicoId)
             .OnDelete(DeleteBehavior.Cascade); // O Restrict, seg�n prefieras
        });

        /* Configuraci�n de LISTA DE FAVORITOS (Contenedor) */
        builder.Entity<ListaFavorito>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "ListasFavoritos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();
            // Como es �nica por usuario, EF ya usa el UserId por la interfaz IUserOwned
        });

        /* Configuraci�n de LINEAS DE FAVORITOS */
        builder.Entity<LineaListaFavorito>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "LineasListasFavoritos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();

            // RELACI�N 1: Una l�nea pertenece a UNA Lista
            b.HasOne(x => x.ListaFavorito)
                .WithMany() // Una lista tiene muchas l�neas (aunque no lo definamos en la clase Lista, EF lo entiende)
                .HasForeignKey(x => x.ListaFavoritoId)
                .OnDelete(DeleteBehavior.Cascade); // IMPORTANTE: Si un d�a borras la lista, se borran las l�neas.

            // RELACI�N 2: Una l�nea apunta a UN Destino
            b.HasOne(x => x.DestinoTuristico)
                .WithMany()
                .HasForeignKey(x => x.DestinoTuristicoId)
                .OnDelete(DeleteBehavior.Cascade); // Si borras el destino (ej. Par�s), desaparece de los favoritos de todos.
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

        builder.Entity<Notificacion>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "Notificaciones", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Titulo).IsRequired().HasMaxLength(100);
            b.Property(x => x.Mensaje).IsRequired().HasMaxLength(500);
            b.HasIndex(x => x.UserId); // Importante para rendimiento
        });

        builder.Entity<HistorialNotificacionEvento>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "HistorialNotificacionesEventos", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.EventoTicketmasterId).IsRequired().HasMaxLength(100);

            // �ndice compuesto fundamental para que el worker vuele buscando coincidencias
            b.HasIndex(x => new { x.UserId, x.DestinoTuristicoId });
        });
        builder.Entity<PreferenciasNotificacion>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "PreferenciasNotificaciones", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();

            // Índice único para relación 1:1 con usuario
            b.HasIndex(x => x.UserId).IsUnique();
        });

        builder.Entity<ColaResumenSemanalEmail>(b =>
        {
            b.ToTable(AmmenTravelConsts.DbTablePrefix + "ColaResumenSemanalEmails", AmmenTravelConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.EmailDestino).IsRequired().HasMaxLength(256);
            b.Property(x => x.Titulo).IsRequired().HasMaxLength(200);
            b.Property(x => x.Mensaje).IsRequired().HasMaxLength(2000);

            // Índice para procesar eficientemente por usuario y estado
            b.HasIndex(x => new { x.UserId, x.Procesado });
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
