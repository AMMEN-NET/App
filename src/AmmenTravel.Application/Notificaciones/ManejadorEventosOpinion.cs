using AmmenTravel.Destinos;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace AmmenTravel.Notificaciones
{
    public class ManejadorEventosOpinion :
        ILocalEventHandler<EntityCreatedEventData<Opinion>>,
        ITransientDependency
    {
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _favoritosRepository;
        private readonly IRepository<ListaFavorito, Guid> _listaFavoritoRepository;
        private readonly IRepository<PreferenciasNotificacion, Guid> _preferenciasRepo;
        private readonly IRepository<ColaResumenSemanalEmail, Guid> _colaEmailRepo;
        private readonly IdentityUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<ManejadorEventosOpinion> _logger;

        public ManejadorEventosOpinion(
            IRepository<Notificacion, Guid> notificacionRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<LineaListaFavorito, Guid> favoritosRepository,
            IRepository<ListaFavorito, Guid> listaFavoritoRepository,
            IRepository<PreferenciasNotificacion, Guid> preferenciasRepo,
            IRepository<ColaResumenSemanalEmail, Guid> colaEmailRepo,
            IdentityUserManager userManager,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator,
            ILogger<ManejadorEventosOpinion> logger)
        {
            _notificacionRepository = notificacionRepository;
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _favoritosRepository = favoritosRepository;
            _listaFavoritoRepository = listaFavoritoRepository;
            _preferenciasRepo = preferenciasRepo;
            _colaEmailRepo = colaEmailRepo;
            _userManager = userManager;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<Opinion> eventData)
        {
            var opinion = eventData.Entity;
            var userId = opinion.UserId;
            var destinoId = opinion.DestinoTuristicoId;

            // Obtenemos info del destino para los mensajes
            var destino = await _destinoRepository.GetAsync(destinoId);

            // ---------------------------------------------------------
            // 1. LOGROS Y GAMIFICACIÓN (Para el autor)
            // ---------------------------------------------------------
            var totalOpinionesUsuario = await _opinionRepository.CountAsync(x => x.UserId == userId);

            // Primer Hito
            if (totalOpinionesUsuario == 1)
            {
                await CrearNotificacionConEmail(userId, "¡Primer Hito Desbloqueado! 🚀",
                    "Has publicado tu primera reseña. Tu pasaporte virtual ha comenzado.",
                    TipoNotificacion.Exito, "fa-passport");
            }
            // Nivel Experto
            else if (totalOpinionesUsuario == 10)
            {
                await CrearNotificacionConEmail(userId, "¡Subiste de Nivel! 🌟",
                    "Con 10 reseñas, ahora eres un Viajero Experimentado.",
                    TipoNotificacion.Exito, "fa-star");
            }

            // Racha de Viajero: contamos destinos distintos opinados por el usuario
            var opinionesQueryable = await _opinionRepository.GetQueryableAsync();
            var destinosDistintos = await opinionesQueryable
                .Where(o => o.UserId == userId)
                .Select(o => o.DestinoTuristicoId)
                .Distinct()
                .CountAsync();

            // Disparo de notificación cuando el usuario tiene 3 destinos distintos
            if (destinosDistintos == 3)
            {
                await CrearNotificacionConEmail(userId, "¡Racha de Viajero! 🔥",
                   "Estás compartiendo muchas experiencias. ¡Sigue así!",
                   TipoNotificacion.Exito, "fa-fire");
            }

            // ---------------------------------------------------------
            // 2. EL PIONERO (Status)
            // ---------------------------------------------------------
            var totalOpinionesDestino = await _opinionRepository.CountAsync(x => x.DestinoTuristicoId == destinoId);

            if (totalOpinionesDestino == 1)
            {
                // Si es 1, significa que esta es la primera (o acaba de ser creada y es la única)
                await CrearNotificacionConEmail(userId, "¡Sos un Pionero! 🚩",
                    $"Abriste el camino. Sos el primero en opinar sobre {destino.Nombre}.",
                    TipoNotificacion.Exito, "fa-flag");
            }

            // ---------------------------------------------------------
            // 3. MIS FAVORITOS (Para OTROS usuarios) - Optimizado
            // ---------------------------------------------------------
            // Evitar N+1: hacemos JOIN entre LineaListaFavorito y ListaFavorito y obtenemos los UserId distintos
            var favQueryable = await _favoritosRepository.GetQueryableAsync();
            var listasQueryable = await _listaFavoritoRepository.GetQueryableAsync();

            var ownerIds = await (from f in favQueryable
                                  join l in listasQueryable on f.ListaFavoritoId equals l.Id
                                  where f.DestinoTuristicoId == destinoId && l.UserId != userId
                                  select l.UserId)
                                 .Distinct()
                                 .ToListAsync();

            foreach (var ownerId in ownerIds)
            {
                if (ownerId == Guid.Empty || ownerId == userId) continue;

                var tituloFav = "¡Novedades en tus favoritos! 🔔";
                var mensajeFav = $"Alguien acaba de opinar sobre {destino.Nombre}. Mira qué dicen.";

                await CrearNotificacionConEmail(ownerId, tituloFav, mensajeFav,
                    TipoNotificacion.Social, "fa-heart", $"/destinos/{destinoId}", destinoId);
            }
        }

        /// <summary>
        /// Crea una notificación en pantalla y/o envía email según las preferencias del usuario.
        /// Respeta la frecuencia: inmediata o resumen semanal.
        /// </summary>
        private async Task CrearNotificacionConEmail(
            Guid userId, string titulo, string mensaje,
            TipoNotificacion tipo, string icono,
            string link = null, Guid? destinoId = null)
        {
            try
            {
                var preferencias = await _preferenciasRepo.FirstOrDefaultAsync(p => p.UserId == userId);
                bool enPantalla = preferencias?.EnPantalla ?? true;
                bool porEmail = preferencias?.PorEmail ?? true;
                FrecuenciaNotificacion frecuencia = preferencias?.Frecuencia ?? FrecuenciaNotificacion.Inmediata;

                if (frecuencia == FrecuenciaNotificacion.Inmediata)
                {
                    // Campanita
                    if (enPantalla)
                    {
                        await _notificacionRepository.InsertAsync(
                            new Notificacion(_guidGenerator.Create(), userId, titulo, mensaje, tipo, link, icono)
                        );
                    }

                    // Email inmediato
                    if (porEmail)
                    {
                        var user = await _userManager.FindByIdAsync(userId.ToString());
                        var email = user?.Email;
                        if (!string.IsNullOrEmpty(email))
                        {
                            string htmlBody;
                            if (tipo == TipoNotificacion.Social && destinoId.HasValue)
                                htmlBody = EmailTemplateHelper.GenerarEmailOpinionFavorito(titulo, mensaje, destinoId);
                            else
                                htmlBody = EmailTemplateHelper.GenerarEmailLogro(titulo, mensaje);

                            await _emailSender.SendAsync(email, titulo, htmlBody, isBodyHtml: true);
                        }
                    }
                }
                else
                {
                    // Modo semanal: encolar para el resumen del domingo
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    var email = user?.Email ?? "";

                    var colaEmail = new ColaResumenSemanalEmail(
                        _guidGenerator.Create(),
                        userId,
                        email,
                        titulo,
                        mensaje
                    );
                    await _colaEmailRepo.InsertAsync(colaEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación/email para usuario {UserId}", userId);
            }
        }
    }
}