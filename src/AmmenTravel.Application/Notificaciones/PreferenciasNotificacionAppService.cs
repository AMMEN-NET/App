using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;

namespace AmmenTravel.Notificaciones
{
    [Authorize]
    public class PreferenciasNotificacionAppService : AmmenTravelAppService, IPreferenciasNotificacionAppService
    {
        private readonly IRepository<PreferenciasNotificacion, Guid> _preferenciasRepository;
        private readonly IRepository<ColaResumenSemanalEmail, Guid> _colaEmailRepo;
        private readonly IRepository<Notificacion, Guid> _notificacionRepo;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;

        public PreferenciasNotificacionAppService(
            IRepository<PreferenciasNotificacion, Guid> preferenciasRepository,
            IRepository<ColaResumenSemanalEmail, Guid> colaEmailRepo,
            IRepository<Notificacion, Guid> notificacionRepo,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator)
        {
            _preferenciasRepository = preferenciasRepository;
            _colaEmailRepo = colaEmailRepo;
            _notificacionRepo = notificacionRepo;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
        }

        public async Task<PreferenciasNotificacionDto> GetMiPreferenciaAsync()
        {
            var userId = CurrentUser.Id!.Value;

            var preferencia = await _preferenciasRepository
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (preferencia == null)
            {
                // Valores por defecto si no existe registro
                return new PreferenciasNotificacionDto
                {
                    Id = null,
                    EnPantalla = true,
                    PorEmail = true,
                    Frecuencia = FrecuenciaNotificacion.Inmediata
                };
            }

            return new PreferenciasNotificacionDto
            {
                Id = preferencia.Id,
                EnPantalla = preferencia.EnPantalla,
                PorEmail = preferencia.PorEmail,
                Frecuencia = preferencia.Frecuencia
            };
        }

        public async Task<PreferenciasNotificacionDto> UpdateMiPreferenciaAsync(UpdatePreferenciasDto input)
        {
            var userId = CurrentUser.Id!.Value;

            var preferencia = await _preferenciasRepository
                .FirstOrDefaultAsync(x => x.UserId == userId);

            // Detectamos si está cambiando de Semanal → Inmediata
            var frecuenciaAnterior = preferencia?.Frecuencia ?? FrecuenciaNotificacion.Inmediata;
            bool cambioAInmediata = frecuenciaAnterior == FrecuenciaNotificacion.ResumenSemanal
                                && input.Frecuencia == FrecuenciaNotificacion.Inmediata;

            if (preferencia == null)
            {
                // Crear nuevo registro (INSERT)
                preferencia = new PreferenciasNotificacion(
                    _guidGenerator.Create(),
                    userId,
                    input.EnPantalla,
                    input.PorEmail,
                    input.Frecuencia
                );

                await _preferenciasRepository.InsertAsync(preferencia);
            }
            else
            {
                // Actualizar existente (UPDATE)
                preferencia.EnPantalla = input.EnPantalla;
                preferencia.PorEmail = input.PorEmail;
                preferencia.Frecuencia = input.Frecuencia;

                await _preferenciasRepository.UpdateAsync(preferencia);
            }

            // Si cambió de Semanal → Inmediata, despachar toda la cola pendiente
            if (cambioAInmediata)
            {
                await DespacharColaPendienteAsync(userId, input.EnPantalla, input.PorEmail);
            }

            return new PreferenciasNotificacionDto
            {
                Id = preferencia.Id,
                EnPantalla = preferencia.EnPantalla,
                PorEmail = preferencia.PorEmail,
                Frecuencia = preferencia.Frecuencia
            };
        }

        /// <summary>
        /// Despacha inmediatamente todas las notificaciones encoladas del usuario.
        /// Se ejecuta cuando cambia de ResumenSemanal → Inmediata.
        /// </summary>
        private async Task DespacharColaPendienteAsync(Guid userId, bool enPantalla, bool porEmail)
        {
            var pendientes = await _colaEmailRepo.GetListAsync(
                e => e.UserId == userId && !e.Procesado
            );

            if (!pendientes.Any()) return;

            foreach (var item in pendientes)
            {
                // Campanita
                if (enPantalla)
                {
                    var notificacion = new Notificacion(
                        _guidGenerator.Create(),
                        userId,
                        item.Titulo,
                        item.Mensaje,
                        TipoNotificacion.Social,
                        "/favoritos",
                        "fa-ticket-alt"
                    );
                    await _notificacionRepo.InsertAsync(notificacion);
                }

                // Email (HTML)
                if (porEmail && !string.IsNullOrEmpty(item.EmailDestino))
                {
                    var htmlBody = EmailTemplateHelper.GenerarEmailNotificacion(item.Titulo, item.Mensaje, "/favoritos", "Ver en AmmenTravel");
                    await _emailSender.SendAsync(item.EmailDestino, item.Titulo, htmlBody, isBodyHtml: true);
                }

                // Marcar como procesado
                item.Procesado = true;
            }

            await _colaEmailRepo.UpdateManyAsync(pendientes);
        }
    }
}
