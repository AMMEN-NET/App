using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using AmmenTravel.Notificaciones;

namespace AmmenTravel.BackgroundWorkers
{
    public class ResumenSemanalEmailService : ITransientDependency
    {
        private readonly IRepository<ColaResumenSemanalEmail, Guid> _colaEmailRepo;
        private readonly IRepository<PreferenciasNotificacion, Guid> _preferenciasRepo;
        private readonly IRepository<Notificacion, Guid> _notificacionRepo;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<ResumenSemanalEmailService> _logger;

        public ResumenSemanalEmailService(
            IRepository<ColaResumenSemanalEmail, Guid> colaEmailRepo,
            IRepository<PreferenciasNotificacion, Guid> preferenciasRepo,
            IRepository<Notificacion, Guid> notificacionRepo,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator,
            ILogger<ResumenSemanalEmailService> logger)
        {
            _colaEmailRepo = colaEmailRepo;
            _preferenciasRepo = preferenciasRepo;
            _notificacionRepo = notificacionRepo;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        [UnitOfWork]
        public virtual async Task ProcesarResumenSemanalAsync()
        {
            try
            {
                // Obtenemos todos los emails pendientes (no procesados)
                var queryable = await _colaEmailRepo.GetQueryableAsync();
                var emailsPendientes = await queryable
                    .Where(e => !e.Procesado)
                    .ToListAsync();

                if (!emailsPendientes.Any())
                {
                    _logger.LogInformation("No hay emails pendientes en la cola de resumen semanal.");
                    return;
                }

                // Agrupamos por usuario/email
                var gruposPorUsuario = emailsPendientes
                    .GroupBy(e => new { e.UserId, e.EmailDestino })
                    .ToList();

                foreach (var grupo in gruposPorUsuario)
                {
                    var userId = grupo.Key.UserId;
                    var emailDestino = grupo.Key.EmailDestino;
                    var notificaciones = grupo.ToList();

                    // Leemos las preferencias actuales del usuario
                    var preferencias = await _preferenciasRepo.FirstOrDefaultAsync(p => p.UserId == userId);
                    bool enPantalla = preferencias?.EnPantalla ?? true;
                    bool porEmail = preferencias?.PorEmail ?? true;

                    try
                    {
                        // NOTIFICACIONES EN PANTALLA (campanita) - se crean ahora en el resumen
                        if (enPantalla)
                        {
                            foreach (var notif in notificaciones)
                            {
                                var notificacion = new Notificacion(
                                    _guidGenerator.Create(),
                                    userId,
                                    notif.Titulo,
                                    notif.Mensaje,
                                    TipoNotificacion.Social,
                                    "/favoritos",
                                    "fa-ticket-alt"
                                );
                                await _notificacionRepo.InsertAsync(notificacion);
                            }
                        }

                        // EMAIL - solo si tiene el canal activo y hay email
                        if (porEmail && !string.IsNullOrEmpty(emailDestino))
                        {
                            var htmlBody = GenerarHtmlResumen(notificaciones);
                            await _emailSender.SendAsync(
                                emailDestino,
                                "🌍 Tu Resumen Semanal de AmmenTravel",
                                htmlBody,
                                isBodyHtml: true
                            );
                        }

                        // Marcamos como procesados
                        foreach (var email in notificaciones)
                        {
                            email.Procesado = true;
                        }
                        await _colaEmailRepo.UpdateManyAsync(notificaciones);

                        _logger.LogInformation($"Resumen semanal procesado para usuario {userId} con {notificaciones.Count} notificaciones.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error al procesar resumen semanal para {emailDestino}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al procesar resumen semanal de emails.");
            }
        }

        private string GenerarHtmlResumen(List<ColaResumenSemanalEmail> notificaciones)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); padding: 30px; }");
            sb.AppendLine(".header { text-align: center; border-bottom: 2px solid #4CAF50; padding-bottom: 20px; margin-bottom: 20px; }");
            sb.AppendLine(".header h1 { color: #333; margin: 0; }");
            sb.AppendLine(".header p { color: #666; margin-top: 10px; }");
            sb.AppendLine(".notification { background-color: #f9f9f9; border-left: 4px solid #4CAF50; padding: 15px; margin-bottom: 15px; border-radius: 0 5px 5px 0; }");
            sb.AppendLine(".notification h3 { color: #333; margin: 0 0 8px 0; font-size: 16px; }");
            sb.AppendLine(".notification p { color: #666; margin: 0; font-size: 14px; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #999; font-size: 12px; }");
            sb.AppendLine(".cta-button { display: inline-block; background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>🌍 AmmenTravel</h1>");
            sb.AppendLine($"<p>¡Hola! Esta semana hubo <strong>{notificaciones.Count}</strong> novedades en tus destinos favoritos</p>");
            sb.AppendLine("</div>");

            // Notificaciones
            foreach (var notif in notificaciones)
            {
                sb.AppendLine("<div class='notification'>");
                sb.AppendLine($"<h3>{EscapeHtml(notif.Titulo)}</h3>");
                sb.AppendLine($"<p>{EscapeHtml(notif.Mensaje)}</p>");
                sb.AppendLine("</div>");
            }

            // CTA
            sb.AppendLine("<div style='text-align: center;'>");
            sb.AppendLine("<a href='https://ammentravel.com/favoritos' class='cta-button'>Ver mis favoritos</a>");
            sb.AppendLine("</div>");

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Recibiste este email porque activaste el resumen semanal en tu perfil.</p>");
            sb.AppendLine("<p>© 2026 AmmenTravel - Tu compañero de viajes</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return System.Net.WebUtility.HtmlEncode(text);
        }
    }
}
