using AmmenTravel.Notificaciones;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.NotificacionTest
{
    public abstract class classNotificacionTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly NotificacionAppService _service;
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;

        protected classNotificacionTestAppServiceTest()
        {
            _service = GetRequiredService<NotificacionAppService>();
            _notificacionRepository = GetRequiredService<IRepository<Notificacion, Guid>>();
        }

        [Fact]
        public async Task GetMisNotificacionesAsync_ObtieneSoloNotificacionesDelUsuarioActual()
        {
            // Asegurarse de tener un usuario actual en el contexto de pruebas
            (CurrentUser.Id != null).ShouldBeTrue();

            var userId = CurrentUser.Id.Value;
            var otherUser = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "TituloUser", "MensajeUser", TipoNotificacion.Informativa), autoSave: true);
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), otherUser, "TituloOtro", "MensajeOtro", TipoNotificacion.Informativa), autoSave: true);
            });

            var result = await WithUnitOfWorkAsync(async () => await _service.GetMisNotificacionesAsync());

            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result[0].Titulo.ShouldBe("TituloUser");
        }

        [Fact]
        public async Task GetCantidadNoLeidasAsync_RetornaCantidadCorrecta()
        {
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            await WithUnitOfWorkAsync(async () =>
            {
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "N1", "M1", TipoNotificacion.Informativa) { Leida = false }, autoSave: true);
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "N2", "M2", TipoNotificacion.Informativa) { Leida = false }, autoSave: true);
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "N3", "M3", TipoNotificacion.Informativa) { Leida = true }, autoSave: true);
            });

            var count = await WithUnitOfWorkAsync(async () => await _service.GetCantidadNoLeidasAsync());
            count.ShouldBe(2);
        }
        
        [Fact]
        public async Task MarcarComoLeidaAsync_MarcaSoloSiEsDelUsuarioActual()
        {
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;
            var otherUser = Guid.NewGuid();

            Guid idUserNotificacion = Guid.Empty;
            Guid idOtherNotificacion = Guid.Empty;

            await WithUnitOfWorkAsync(async () =>
            {
                var n1 = new Notificacion(Guid.NewGuid(), userId, "ParaMarcar", "M", TipoNotificacion.Informativa) { Leida = false };
                var n2 = new Notificacion(Guid.NewGuid(), otherUser, "NoTuya", "M", TipoNotificacion.Informativa) { Leida = false };
                await _notificacionRepository.InsertAsync(n1, autoSave: true);
                await _notificacionRepository.InsertAsync(n2, autoSave: true);
                idUserNotificacion = n1.Id;
                idOtherNotificacion = n2.Id;
            });

            // Act: ejecutar la mutación dentro de UoW para asegurar DbContext activo
            await WithUnitOfWorkAsync(async () => await _service.MarcarComoLeidaAsync(idUserNotificacion));

            // Assert dentro de UoW para leer desde DB activo
            await WithUnitOfWorkAsync(async () =>
            {
                var queryable = await _notificacionRepository.GetQueryableAsync();

                var updatedUserNotif = await queryable
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(n => n.Id == idUserNotificacion);

                updatedUserNotif.ShouldNotBeNull();
                updatedUserNotif.Leida.ShouldBeTrue();

                var otherNotif = await queryable
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(n => n.Id == idOtherNotificacion);

                otherNotif.ShouldNotBeNull();
                otherNotif.Leida.ShouldBeFalse();

                /*
                        var updatedUserNotif = await _notificacionRepository.GetAsync(idUserNotificacion);
                        updatedUserNotif.Leida.ShouldBeTrue();

                        var otherNotif = await _notificacionRepository.GetAsync(idOtherNotificacion);
                        otherNotif.Leida.ShouldBeFalse();
                */
            });
        }
        

        // Por ahora funcionan los tres de arriba

        
        [Fact]
        public async Task MarcarTodasComoLeidasAsync_MarcaTodasLasNoLeidasDelUsuarioActual()
        {
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;
            var otherUser = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "A1", "M", TipoNotificacion.Informativa) { Leida = false }, autoSave: true);
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), userId, "A2", "M", TipoNotificacion.Informativa) { Leida = false }, autoSave: true);
                await _notificacionRepository.InsertAsync(new Notificacion(Guid.NewGuid(), otherUser, "B1", "M", TipoNotificacion.Informativa) { Leida = false }, autoSave: true);
            });

            // Ejecutar la acción del servicio dentro de UoW
            await WithUnitOfWorkAsync(async () => await _service.MarcarTodasComoLeidasAsync());

            // Leer y comprobar dentro de UoW
            await WithUnitOfWorkAsync(async () =>
            {
                var queryable = await _notificacionRepository.GetQueryableAsync();

                var mine = (await queryable
                    .IgnoreQueryFilters()
                    .Where(n => n.UserId == userId)
                    .ToListAsync());

                mine.ShouldNotBeEmpty();
                mine.ShouldAllBe(n => n.Leida);

                var others = (await queryable
                    .IgnoreQueryFilters()
                    .Where(n => n.UserId == otherUser)
                    .ToListAsync());

                others.Any().ShouldBeTrue();
                // Las de otros usuarios no deben haber sido marcadas por el servicio
                others.All(n => n.Leida == false).ShouldBeTrue();


                /*
                        var mine = (await _notificacionRepository.GetListAsync(n => n.UserId == userId)).ToList();
                        mine.ShouldAllBe(n => n.Leida);

                        var others = (await _notificacionRepository.GetListAsync(n => n.UserId == otherUser)).ToList();
                        others.Any().ShouldBeTrue();
                        // Las de otros usuarios no deben haber sido marcadas por el servicio
                        others.All(n => n.Leida == false).ShouldBeTrue();
                */
            });
        }
        
    }
}