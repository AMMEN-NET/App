using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;
using AmmenTravel.Opiniones;

namespace AmmenTravel.Notificaciones
{
    public class ReactivacionUsuarioJob : AsyncBackgroundJob<ReactivacionArgs>, ITransientDependency
    {
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;
        private readonly IRepository<PreferenciasNotificacion, Guid> _preferenciasRepo;
        private readonly IRepository<ColaResumenSemanalEmail, Guid> _colaEmailRepo;
        private readonly IdentityUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<ReactivacionUsuarioJob> _logger;

        public ReactivacionUsuarioJob(
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<Notificacion, Guid> notificacionRepository,
            IRepository<PreferenciasNotificacion, Guid> preferenciasRepo,
            IRepository<ColaResumenSemanalEmail, Guid> colaEmailRepo,
            IdentityUserManager userManager,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator,
            ILogger<ReactivacionUsuarioJob> logger)
        {
            _userRepository = userRepository;
            _opinionRepository = opinionRepository;
            _notificacionRepository = notificacionRepository;
            _preferenciasRepo = preferenciasRepo;
            _colaEmailRepo = colaEmailRepo;
            _userManager = userManager;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        public override async Task ExecuteAsync(ReactivacionArgs args)
        {
            // Parámetros de control
            const int pageSize = 500;                 // usuarios por página
            const int maxNotificationsPerRun = 200;   // límite de notificaciones a crear por ejecución
            var cutoffOpinion = DateTime.UtcNow.AddDays(-30);
            var cutoffNewUser = DateTime.UtcNow.AddDays(-15);
            var recentNotifWindow = DateTime.UtcNow.AddDays(-7);

            // Obtenemos queryables (no materializan nada)
            var usersQuery = await _userRepository.GetQueryableAsync();
            var opinionsQuery = await _opinionRepository.GetQueryableAsync();
            var notifsQuery = await _notificacionRepository.GetQueryableAsync();

            int pageIndex = 0;
            int createdCount = 0;

            while (true)
            {
                try
                {
                    // Consulta: por cada usuario obtenemos la fecha máxima de su opinión (si la tiene)
                    var candidatesQuery = from u in usersQuery
                                          join o in opinionsQuery on u.Id equals o.UserId into g
                                          let lastOpinion = g.Max(x => (DateTime?)x.CreationTime)
                                          where (lastOpinion == null && u.CreationTime < cutoffNewUser)
                                                || (lastOpinion != null && lastOpinion < cutoffOpinion)
                                          orderby u.CreationTime
                                          select new { u.Id };

                    var page = await candidatesQuery
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .Select(x => x.Id)
                        .ToListAsync();

                    if (page == null || page.Count == 0)
                        break;

                    // Filtrar los que ya recibieron recordatorio recientemente en una sola query
                    var alreadyNotified = await notifsQuery
                        .Where(n => page.Contains(n.UserId) && n.Tipo == TipoNotificacion.Recordatorio && n.CreationTime > recentNotifWindow)
                        .Select(n => n.UserId)
                        .Distinct()
                        .ToListAsync();

                    var toNotify = page.Except(alreadyNotified).ToList();

                    // Aplicar límite global por ejecución
                    var remainingQuota = Math.Max(0, maxNotificationsPerRun - createdCount);
                    if (remainingQuota <= 0)
                        break;

                    var toNotifyLimited = toNotify.Take(remainingQuota).ToList();

                    // Crear notificaciones
                    foreach (var userId in toNotifyLimited)
                    {
                        try
                        {
                            var titulo = "Te extrañamos en AmmenTravel";
                            var mensaje = "Hace tiempo que no compartes una reseña. ¿Querés contar tu última experiencia?";

                            // Obtener preferencias del usuario
                            var preferencias = await _preferenciasRepo.FirstOrDefaultAsync(p => p.UserId == userId);
                            bool enPantalla = preferencias?.EnPantalla ?? true;
                            bool porEmail = preferencias?.PorEmail ?? true;
                            FrecuenciaNotificacion frecuencia = preferencias?.Frecuencia ?? FrecuenciaNotificacion.Inmediata;

                            if (frecuencia == FrecuenciaNotificacion.Inmediata)
                            {
                                // Campanita
                                if (enPantalla)
                                {
                                    var notificacion = new Notificacion(
                                        _guidGenerator.Create(),
                                        userId,
                                        titulo,
                                        mensaje,
                                        TipoNotificacion.Recordatorio,
                                        linkReferencia: "/mi-perfil/mis-calificaciones",
                                        icono: "fa-clock"
                                    );
                                    await _notificacionRepository.InsertAsync(notificacion, autoSave: true);
                                }

                                // Email inmediato (HTML)
                                if (porEmail)
                                {
                                    var user = await _userManager.FindByIdAsync(userId.ToString());
                                    var email = user?.Email;
                                    if (!string.IsNullOrEmpty(email))
                                    {
                                        var htmlBody = EmailTemplateHelper.GenerarEmailReactivacion(titulo, mensaje);
                                        await _emailSender.SendAsync(email, titulo, htmlBody, isBodyHtml: true);
                                    }
                                }
                            }
                            else
                            {
                                // Modo semanal: encolar
                                var user = await _userManager.FindByIdAsync(userId.ToString());
                                var email = user?.Email ?? "";

                                var colaEmail = new ColaResumenSemanalEmail(
                                    _guidGenerator.Create(),
                                    userId,
                                    email,
                                    titulo,
                                    mensaje
                                );
                                await _colaEmailRepo.InsertAsync(colaEmail, autoSave: true);
                            }

                            createdCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al crear notificación para user {UserId}", userId);
                        }
                    }

                    // Si llenamos la cuota, salimos
                    if (createdCount >= maxNotificationsPerRun)
                        break;

                    // Si la página tenía menos elementos que pageSize y ya procesamos, podemos salir
                    if (page.Count < pageSize)
                        break;

                    pageIndex++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en ReactivacionUsuarioJob, página {PageIndex}", pageIndex);
                    // evitar bucle infinito en caso de error persistente
                    break;
                }
            }

            _logger.LogInformation("ReactivacionUsuarioJob finalizó. Notificaciones creadas: {Count}", createdCount);
        }
    }

    public class ReactivacionArgs { }
}