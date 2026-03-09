using AmmenTravel.BackgroundWorkers;
using AmmenTravel.Destinos;
using AmmenTravel.ExternalService;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Notificaciones;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.WorkersTest
{


    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public abstract class WorkersTestAppService<TStartupModule>
       : AmmenTravelApplicationTestBase<TStartupModule>
       where TStartupModule : IAbpModule
    {
    
         
        protected WorkersTestAppService()
        {

        }

        [Fact]
        public async Task ProcesarEventosAsync_CuandoHayEventoNuevo_CreaNotificacion_Y_GuardaHistorial_Y_NoDuplica()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var notificador = GetRequiredService<NotificadorEventosService>();

            var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            var listaRepo = GetRequiredService<IRepository<ListaFavorito, Guid>>();
            var lineaRepo = GetRequiredService<IRepository<LineaListaFavorito, Guid>>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();
            var historialRepo = GetRequiredService<IRepository<HistorialNotificacionEvento, Guid>>();

            var eventosExternos = GetRequiredService<IEventosExternosAppService>();

            // mock ticketmaster
            eventosExternos.ObtenerEventosPorUbicacionAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(new List<EventoTicketmasterDto>
                {
                    new EventoTicketmasterDto
                    {
                        Id = "evt-1",
                        Nombre = "Evento 1",
                        UrlTicket = "https://tickets.test/evt-1",
                        FechaInicio = DateTime.UtcNow.AddDays(2),
                        ImagenUrl = "https://img.test/evt-1.jpg"
                    }
                }));

            var destinoId = Guid.NewGuid();
            Guid listaId = Guid.Empty;

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

                var lista = await listaRepo.InsertAsync(new ListaFavorito
                {
                    UserId = userId
                }, autoSave: true);

                listaId = lista.Id;

                await lineaRepo.InsertAsync(new LineaListaFavorito
                {
                    ListaFavoritoId = listaId,
                    DestinoTuristicoId = destinoId
                }, autoSave: true);
            });

            var beforeNotifs = await WithUnitOfWorkAsync(async () =>
                await notificacionRepo.CountAsync(n => n.UserId == userId));

            var beforeHist = await WithUnitOfWorkAsync(async () =>
                await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId));

            // Act 1
            await notificador.ProcesarEventosAsync();

            // Assert 1
            await WithUnitOfWorkAsync(async () =>
            {
                var afterNotifs = await notificacionRepo.CountAsync(n => n.UserId == userId);
                afterNotifs.ShouldBe(beforeNotifs + 1);

                var list = await notificacionRepo.GetListAsync(n => n.UserId == userId);
                list.ShouldContain(n =>
                    n.Tipo == TipoNotificacion.Social &&
                    n.LinkReferencia == "/favoritos" &&
                    n.Icono == "fa-ticket-alt");

                var afterHist = await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId);
                afterHist.ShouldBe(beforeHist + 1);
            });

            // Act 2 (mismo evento) => no duplica
            await notificador.ProcesarEventosAsync();

            // Assert 2
            await WithUnitOfWorkAsync(async () =>
            {
                var finalNotifs = await notificacionRepo.CountAsync(n => n.UserId == userId);
                finalNotifs.ShouldBe(beforeNotifs + 1);

                var finalHist = await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId);
                finalHist.ShouldBe(beforeHist + 1);
            });
        }

        [Fact]
        public async Task ProcesarEventosAsync_SinFavoritos_NoCreaNotificaciones()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var notificador = GetRequiredService<NotificadorEventosService>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();
            var historialRepo = GetRequiredService<IRepository<HistorialNotificacionEvento, Guid>>();

            var beforeNotifs = await WithUnitOfWorkAsync(async () =>
                await notificacionRepo.CountAsync(n => n.UserId == userId));

            var beforeHist = await WithUnitOfWorkAsync(async () =>
                await historialRepo.CountAsync(h => h.UserId == userId));

            // Act
            await notificador.ProcesarEventosAsync();

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var afterNotifs = await notificacionRepo.CountAsync(n => n.UserId == userId);
                afterNotifs.ShouldBe(beforeNotifs);

                var afterHist = await historialRepo.CountAsync(h => h.UserId == userId);
                afterHist.ShouldBe(beforeHist);
            });
        }

        [Fact]
        public async Task ProcesarEventosAsync_CuandoTicketmasterNoDevuelveEventos_NoCreaNotificacionNiHistorial()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var notificador = GetRequiredService<NotificadorEventosService>();

            var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            var listaRepo = GetRequiredService<IRepository<ListaFavorito, Guid>>();
            var lineaRepo = GetRequiredService<IRepository<LineaListaFavorito, Guid>>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();
            var historialRepo = GetRequiredService<IRepository<HistorialNotificacionEvento, Guid>>();
            var eventosExternos = GetRequiredService<IEventosExternosAppService>();

            // mock: sin eventos
            eventosExternos.ObtenerEventosPorUbicacionAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(new List<EventoTicketmasterDto>()));

            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino sin eventos",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);

                var lista = await listaRepo.InsertAsync(new ListaFavorito { UserId = userId }, autoSave: true);

                await lineaRepo.InsertAsync(new LineaListaFavorito
                {
                    ListaFavoritoId = lista.Id,
                    DestinoTuristicoId = destinoId
                }, autoSave: true);
            });

            var beforeNotifs = await WithUnitOfWorkAsync(async () =>
                await notificacionRepo.CountAsync(n => n.UserId == userId));

            var beforeHist = await WithUnitOfWorkAsync(async () =>
                await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId));

            // Act
            await notificador.ProcesarEventosAsync();

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var afterNotifs = await notificacionRepo.CountAsync(n => n.UserId == userId);
                afterNotifs.ShouldBe(beforeNotifs);

                var afterHist = await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId);
                afterHist.ShouldBe(beforeHist);
            });
        }

        [Fact]
        public async Task ProcesarEventosAsync_CuandoEventoYaNotificado_NoCreaNotificacion()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var notificador = GetRequiredService<NotificadorEventosService>();

            var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            var listaRepo = GetRequiredService<IRepository<ListaFavorito, Guid>>();
            var lineaRepo = GetRequiredService<IRepository<LineaListaFavorito, Guid>>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();
            var historialRepo = GetRequiredService<IRepository<HistorialNotificacionEvento, Guid>>();
            var eventosExternos = GetRequiredService<IEventosExternosAppService>();
            var guidGenerator = GetRequiredService<Volo.Abp.Guids.IGuidGenerator>();

            // Ticketmaster devuelve SIEMPRE el mismo evento
            eventosExternos.ObtenerEventosPorUbicacionAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(new List<EventoTicketmasterDto>
                {
            new EventoTicketmasterDto { Id = "evt-1", Nombre="Evento 1", UrlTicket="x", FechaInicio=DateTime.UtcNow, ImagenUrl="y" }
                }));

            var destinoId = Guid.NewGuid();
            Guid listaId;

            await WithUnitOfWorkAsync(async () =>
            {
                await destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);

                var lista = await listaRepo.InsertAsync(new ListaFavorito { UserId = userId }, autoSave: true);
                listaId = lista.Id;

                await lineaRepo.InsertAsync(new LineaListaFavorito
                {
                    ListaFavoritoId = listaId,
                    DestinoTuristicoId = destinoId
                }, autoSave: true);

                // Insertamos historial previo -> “ya notificado”
                await historialRepo.InsertAsync(new HistorialNotificacionEvento(
                    guidGenerator.Create(),
                    userId,
                    destinoId,
                    "evt-1"
                ), autoSave: true);
            });

            var beforeNotifs = await WithUnitOfWorkAsync(async () =>
                await notificacionRepo.CountAsync(n => n.UserId == userId));

            var beforeHist = await WithUnitOfWorkAsync(async () =>
                await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId));

            // Act
            await notificador.ProcesarEventosAsync();

            // Assert (no cambia nada)
            await WithUnitOfWorkAsync(async () =>
            {
                var afterNotifs = await notificacionRepo.CountAsync(n => n.UserId == userId);
                afterNotifs.ShouldBe(beforeNotifs);

                var afterHist = await historialRepo.CountAsync(h => h.UserId == userId && h.DestinoTuristicoId == destinoId);
                afterHist.ShouldBe(beforeHist);
            });
        }

    }
}
    
