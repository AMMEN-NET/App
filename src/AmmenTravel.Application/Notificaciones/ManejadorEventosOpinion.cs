using AmmenTravel.Destinos;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events; // Para EntityCreatedEventData
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Guids;

namespace AmmenTravel.Notificaciones
{
    public class ManejadorEventosOpinion :
        ILocalEventHandler<EntityCreatedEventData<Opinion>>, // Escucha cuando se CREA una opinión
        ITransientDependency
    {
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _favoritosRepository;
        private readonly IRepository<ListaFavorito, Guid> _listaFavoritoRepository;
        private readonly IGuidGenerator _guidGenerator;

        public ManejadorEventosOpinion(
            IRepository<Notificacion, Guid> notificacionRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<LineaListaFavorito, Guid> favoritosRepository,
            IRepository<ListaFavorito, Guid> listaFavoritoRepository,
            IGuidGenerator guidGenerator)
        {
            _notificacionRepository = notificacionRepository;
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _favoritosRepository = favoritosRepository;
            _listaFavoritoRepository = listaFavoritoRepository;
            _guidGenerator = guidGenerator;
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
                await CrearNotificacion(userId, "¡Primer Hito Desbloqueado! 🚀",
                    "Has publicado tu primera reseña. Tu pasaporte virtual ha comenzado.",
                    TipoNotificacion.Exito, "fa-passport");
            }
            // Nivel Experto
            else if (totalOpinionesUsuario == 10)
            {
                await CrearNotificacion(userId, "¡Subiste de Nivel! 🌟",
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
                await CrearNotificacion(userId, "¡Racha de Viajero! 🔥",
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
                await CrearNotificacion(userId, "¡Sos un Pionero! 🚩",
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

                await CrearNotificacion(ownerId, "¡Novedades en tus favoritos! 🔔",
                    $"Alguien acaba de opinar sobre {destino.Nombre}. Mira qué dicen.",
                    TipoNotificacion.Social, "fa-heart", $"/destinos/{destinoId}");
            }
        }

        private async Task CrearNotificacion(Guid userId, string titulo, string mensaje, TipoNotificacion tipo, string icono, string link = null)
        {
            await _notificacionRepository.InsertAsync(
                new Notificacion(_guidGenerator.Create(), userId, titulo, mensaje, tipo, link, icono)
            );
        }

        // Helper auxiliar (implementar lógica real con repositorios)
        private async Task<Guid> ObtenerDueñoLista(Guid listaId)
        {
            if (listaId == Guid.Empty) return Guid.Empty;

            // Intentamos obtener la lista; FirstOrDefaultAsync evita excepciones si no existe
            var lista = await _listaFavoritoRepository.FirstOrDefaultAsync(l => l.Id == listaId);

            return lista?.UserId ?? Guid.Empty;
        }
    }
}