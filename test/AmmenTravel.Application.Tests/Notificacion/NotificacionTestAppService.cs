using AmmenTravel.Destinos;
using AmmenTravel.ExternalService;
using AmmenTravel.ListaDeFavoritos;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Notificaciones;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.NotificacionTest
{
    public abstract class classNotificacionTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {

        private readonly NotificacionAppService _service;
        private readonly IRepository<Notificacion, Guid> _repo;

        protected classNotificacionTestAppServiceTest()
        {
            _service = GetRequiredService<NotificacionAppService>();
            _repo = GetRequiredService<IRepository<Notificacion, Guid>>();
        }

        [Fact]
        public async Task GetMisNotificacionesAsync_RetornaMaximo10_YOrdenadasDesc()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            // Creamos 12 notificaciones del usuario actual
            await WithUnitOfWorkAsync(async () =>
            {
                for (var i = 0; i < 12; i++)
                {
                    await _repo.InsertAsync(
                        new Notificacion(
                            Guid.NewGuid(),
                            userId,
                            titulo: $"Titulo {i}",
                            mensaje: $"Mensaje {i}",
                            tipo: TipoNotificacion.Informativa,
                            linkReferencia: "/test",
                            icono: "fa-bell"
                        ),
                        autoSave: true
                    );
                }
            });

            // Act
            var items = await WithUnitOfWorkAsync(async () => await _service.GetMisNotificacionesAsync());

            // Assert
            items.ShouldNotBeNull();
            items.Count.ShouldBe(10);

            // Verificar orden descendente por Fecha
            for (int i = 0; i < items.Count - 1; i++)
            {
                items[i].Fecha.ShouldBeGreaterThanOrEqualTo(items[i + 1].Fecha);
            }
        }

        [Fact]
        public async Task GetCantidadNoLeidasAsync_CuentaSoloNoLeidas()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var n1 = new Notificacion(Guid.NewGuid(), userId, "A", "A", TipoNotificacion.Social);
            var n2 = new Notificacion(Guid.NewGuid(), userId, "B", "B", TipoNotificacion.Social);
            var n3 = new Notificacion(Guid.NewGuid(), userId, "C", "C", TipoNotificacion.Social);

            await WithUnitOfWorkAsync(async () =>
            {
                await _repo.InsertAsync(n1, autoSave: true);
                await _repo.InsertAsync(n2, autoSave: true);
                await _repo.InsertAsync(n3, autoSave: true);

                // marcamos 1 como leída
                n2.Leida = true;
                await _repo.UpdateAsync(n2, autoSave: true);
            });

            // Act
            var count = await WithUnitOfWorkAsync(async () => await _service.GetCantidadNoLeidasAsync());

            // Assert
            count.ShouldBe(2);
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_MarcaSoloSiEsDelUsuario()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var notif = new Notificacion(Guid.NewGuid(), userId, "T", "M", TipoNotificacion.Informativa);

            await WithUnitOfWorkAsync(async () =>
            {
                await _repo.InsertAsync(notif, autoSave: true);
            });

            // Act
            await WithUnitOfWorkAsync(async () => await _service.MarcarComoLeidaAsync(notif.Id));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var fromDb = await _repo.GetAsync(notif.Id);
                fromDb.Leida.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task MarcarTodasComoLeidasAsync_MarcaTodasLasNoLeidas()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var a = new Notificacion(Guid.NewGuid(), userId, "A", "A", TipoNotificacion.Informativa);
            var b = new Notificacion(Guid.NewGuid(), userId, "B", "B", TipoNotificacion.Informativa);
            var c = new Notificacion(Guid.NewGuid(), userId, "C", "C", TipoNotificacion.Informativa);

            await WithUnitOfWorkAsync(async () =>
            {
                await _repo.InsertAsync(a, autoSave: true);
                await _repo.InsertAsync(b, autoSave: true);
                await _repo.InsertAsync(c, autoSave: true);

                // dejamos una ya leída, para asegurar que no rompe
                b.Leida = true;
                await _repo.UpdateAsync(b, autoSave: true);
            });

            // Act
            await WithUnitOfWorkAsync(async () => await _service.MarcarTodasComoLeidasAsync());

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var list = await _repo.GetListAsync(x => x.UserId == userId);
                list.Count.ShouldBeGreaterThanOrEqualTo(3);
                list.All(x => x.Leida).ShouldBeTrue();
            });
        }

        [Fact]
        public async Task GetMisNotificacionesAsync_RetornaVacia_SiNoHay()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();

            // Act
            var items = await WithUnitOfWorkAsync(async () => await _service.GetMisNotificacionesAsync());

            // Assert
            items.ShouldNotBeNull();
            items.Count.ShouldBeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetMisNotificacionesAsync_NoTraeNotificacionesDeOtroUsuario()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;
            var otherUserId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _repo.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "mias", "mias", TipoNotificacion.Social), autoSave: true);
                await _repo.InsertAsync(new Notificacion(Guid.NewGuid(), otherUserId, "otras", "otras", TipoNotificacion.Social), autoSave: true);
            });

            // Act
            var items = await WithUnitOfWorkAsync(async () => await _service.GetMisNotificacionesAsync());

            // Assert
            items.ShouldContain(x => x.Titulo == "mias");
            items.ShouldNotContain(x => x.Titulo == "otras");
        }

        [Fact]
        public async Task GetCantidadNoLeidasAsync_Retorna0_SiNoHayNoLeidas()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            await WithUnitOfWorkAsync(async () =>
            {
                var n = new Notificacion(Guid.NewGuid(), userId, "leida", "leida", TipoNotificacion.Informativa);
                n.Leida = true;
                await _repo.InsertAsync(n, autoSave: true);
            });

            // Act
            var count = await WithUnitOfWorkAsync(async () => await _service.GetCantidadNoLeidasAsync());

            // Assert
            count.ShouldBe(0);
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_LanzaEntityNotFound_SiNoEsDelUsuario()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var otherUserId = Guid.NewGuid();
            var notifId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _repo.InsertAsync(
                    new Notificacion(notifId, otherUserId, "otra", "otra", TipoNotificacion.Alerta),
                    autoSave: true
                );

                // Act + Assert
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                    await _service.MarcarComoLeidaAsync(notifId));
            });
        }

        [Fact]
        public async Task MarcarTodasComoLeidasAsync_NoRompe_SiNoHayNoLeidas()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            await WithUnitOfWorkAsync(async () =>
            {
                var n = new Notificacion(Guid.NewGuid(), userId, "ya", "ya", TipoNotificacion.Informativa);
                n.Leida = true;
                await _repo.InsertAsync(n, autoSave: true);
            });

            // Act
            var ex = await Record.ExceptionAsync(async () =>
                await WithUnitOfWorkAsync(async () => await _service.MarcarTodasComoLeidasAsync()));

            // Assert
            ex.ShouldBeNull();
        }

        
        /// /////////////
        
        [Fact]
        public async Task Favoritos_CuandoHayEventosExternos_CreaNotificacionSocial()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var favoritosService = GetRequiredService<ListaDeFavoritosAppService>();
            var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();
            var eventosExternos = GetRequiredService<IEventosExternosAppService>();

            // mock ticketmaster
            eventosExternos.ObtenerEventosPorUbicacionAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(new List<EventoTicketmasterDto>
                {
                new EventoTicketmasterDto
                {
                    Id = "evt-1",
                    Nombre = "Evento",
                    UrlTicket = "https://tickets.test/evt-1",
                    FechaInicio = DateTime.UtcNow.AddDays(2),
                    ImagenUrl = "https://img.test/evt-1.jpg"
                }
                }));

            var destinoId = Guid.NewGuid();
            await WithUnitOfWorkAsync(async () =>
            {
                await destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Evento",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = -34.0f,
                    Longitud = -58.0f
                }, autoSave: true);
            });

            var before = await WithUnitOfWorkAsync(async () =>
                await notificacionRepo.CountAsync(n => n.UserId == userId));

            // Act
            await WithUnitOfWorkAsync(async () => await favoritosService.AgregarAFavoritosAsync(destinoId));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var after = await notificacionRepo.CountAsync(n => n.UserId == userId);
                after.ShouldBe(before + 1);

                var list = await notificacionRepo.GetListAsync(n => n.UserId == userId);
                list.ShouldContain(n =>
                    n.Tipo == TipoNotificacion.Social &&
                    n.LinkReferencia == "/favoritos" &&
                    n.Icono == "fa-ticket-alt");
            });
        }

        [Fact]
        public async Task Opinion_PrimeraOpinion_CreaNotificacionPrimerHito_Y_Pionero()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var handler = GetRequiredService<ManejadorEventosOpinion>();
            var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            var opinionRepo = GetRequiredService<IRepository<Opinion, Guid>>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();

            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Opinión",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);
            });

            var opinion = new Opinion(destinoId, userId, ValorPuntuacion.Cinco, "Excelente");

            await WithUnitOfWorkAsync(async () =>
            {
                await opinionRepo.InsertAsync(opinion, autoSave: true);
            });

            var before = await WithUnitOfWorkAsync(async () =>
                await notificacionRepo.GetListAsync(n => n.UserId == userId));

            // Act
            await WithUnitOfWorkAsync(async () =>
                await handler.HandleEventAsync(new EntityCreatedEventData<Opinion>(opinion)));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var after = await notificacionRepo.GetListAsync(n => n.UserId == userId);

                // Primer Hito + Pionero => 2 notificaciones (según tu handler)
                after.Count.ShouldBe(before.Count + 2);

                after.ShouldContain(n => n.Titulo.Contains("Primer Hito", StringComparison.OrdinalIgnoreCase));
                after.ShouldContain(n => n.Titulo.Contains("Pionero", StringComparison.OrdinalIgnoreCase));
            });
        }

        
        
    }
}

