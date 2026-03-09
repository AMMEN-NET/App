using AmmenTravel.Notificaciones;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Emailing;
using Xunit;

namespace AmmenTravel.NotificacionTest
{
    public abstract class PreferenciasNotificacionTestAppService<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        //protected IEmailSender EmailSender => GetRequiredService<IEmailSender>();
        protected IPreferenciasNotificacionAppService Service => GetRequiredService<IPreferenciasNotificacionAppService>();

        protected PreferenciasNotificacionTestAppService()
        {
            // constructor vacío:  para que el runner herede sin problemas
        }

        [Fact]
        public async Task GetMiPreferenciaAsync_SiNoExisteRegistro_RetornaDefaults()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();

            var service = GetRequiredService<IPreferenciasNotificacionAppService>();

            // Act
            var pref = await WithUnitOfWorkAsync(async () =>
                await service.GetMiPreferenciaAsync());

            // Assert
            pref.ShouldNotBeNull();
            pref.Id.ShouldBeNull();
            pref.EnPantalla.ShouldBeTrue();
            pref.PorEmail.ShouldBeTrue();
            pref.Frecuencia.ShouldBe(FrecuenciaNotificacion.Inmediata);
        }
        [Fact]
        public async Task UpdateMiPreferenciaAsync_CreaRegistroYPersiste()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();

            var service = GetRequiredService<IPreferenciasNotificacionAppService>();

            var input = new UpdatePreferenciasDto
            {
                EnPantalla = false,
                PorEmail = false,
                Frecuencia = FrecuenciaNotificacion.ResumenSemanal
            };

            // Act
            var updated = await WithUnitOfWorkAsync(async () =>
                await service.UpdateMiPreferenciaAsync(input));

            var fetched = await WithUnitOfWorkAsync(async () =>
                await service.GetMiPreferenciaAsync());

            // Assert
            updated.ShouldNotBeNull();
            updated.Id.ShouldNotBeNull();
            updated.EnPantalla.ShouldBeFalse();
            updated.PorEmail.ShouldBeFalse();
            updated.Frecuencia.ShouldBe(FrecuenciaNotificacion.ResumenSemanal);

            fetched.Id.ShouldBe(updated.Id);
            fetched.EnPantalla.ShouldBeFalse();
            fetched.PorEmail.ShouldBeFalse();
            fetched.Frecuencia.ShouldBe(FrecuenciaNotificacion.ResumenSemanal);
        }
    
     [Fact]
        public async Task UpdateMiPreferenciaAsync_DeSemanalAInmediata_DespachaColaPendiente_CreaNotificaciones_YMarcaProcesado()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var service = GetRequiredService<IPreferenciasNotificacionAppService>();

            var colaRepo = GetRequiredService<IRepository<ColaResumenSemanalEmail, Guid>>();
            var notificacionRepo = GetRequiredService<IRepository<Notificacion, Guid>>();

            // 1) dejar preferencia en semanal
            await WithUnitOfWorkAsync(async () =>
            {
                await service.UpdateMiPreferenciaAsync(new UpdatePreferenciasDto
                {
                    EnPantalla = true,
                    PorEmail = false, //  evitamos dependencia de EmailTemplateHelper
                    Frecuencia = FrecuenciaNotificacion.ResumenSemanal
                });
            });

            // 2) insertar items pendientes en la cola
            var colaId1 = Guid.NewGuid();
            var colaId2 = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await colaRepo.InsertAsync(
                    new ColaResumenSemanalEmail(
                        colaId1,
                        userId,
                        emailDestino: "test@local", // aunque PorEmail=false, lo ponemos válido
                        titulo: "T1",
                        mensaje: "M1"
                    ),
                    autoSave: true
                );

                await colaRepo.InsertAsync(
                    new ColaResumenSemanalEmail(
                        colaId2,
                        userId,
                        emailDestino: "test@local",
                        titulo: "T2",
                        mensaje: "M2"
                    ),
                    autoSave: true
                );
            });

            var beforePendientes = await WithUnitOfWorkAsync(() =>
                colaRepo.CountAsync(x => x.UserId == userId && !x.Procesado));
            beforePendientes.ShouldBe(2);

            var beforeNotifs = await WithUnitOfWorkAsync(() =>
                notificacionRepo.CountAsync(n => n.UserId == userId));

            // Act: pasar a inmediata => debe despachar cola
            await WithUnitOfWorkAsync(async () =>
            {
                await service.UpdateMiPreferenciaAsync(new UpdatePreferenciasDto
                {
                    EnPantalla = true,
                    PorEmail = false,
                    Frecuencia = FrecuenciaNotificacion.Inmediata
                });
            });

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var afterPendientes = await colaRepo.CountAsync(x => x.UserId == userId && !x.Procesado);
                afterPendientes.ShouldBe(0);

                var afterNotifs = await notificacionRepo.CountAsync(n => n.UserId == userId);
                afterNotifs.ShouldBe(beforeNotifs + 2);

                // validar que los 2 ítems quedaron procesados (consulta directa)
                var procesados = await colaRepo.GetListAsync(x => x.UserId == userId && x.Procesado);
                procesados.Count.ShouldBeGreaterThanOrEqualTo(2);
            });
        }


    }
}
