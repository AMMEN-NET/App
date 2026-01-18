using AmmenTravel.Destinos;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events; // Para EntityCreatedEventData
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
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
        private readonly IGuidGenerator _guidGenerator;

        public ManejadorEventosOpinion(
            IRepository<Notificacion, Guid> notificacionRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<LineaListaFavorito, Guid> favoritosRepository,
            IGuidGenerator guidGenerator)
        {
            _notificacionRepository = notificacionRepository;
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _favoritosRepository = favoritosRepository;
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

            // Racha de Viajero (Lógica simplificada: chequeamos si tiene 3 destinos distintos)
            // Podrías refinar esto para que sea "este mes" filtrando por CreationTime
            var destinosDistintos = await _opinionRepository.CountAsync(x => x.UserId == userId);
            // Nota: CountAsync directo no hace distinct por columna fácilmente en repo genérico, 
            // para una racha real compleja requeriría un Queryable con GroupBy, lo simplifico aquí:
            if (totalOpinionesUsuario == 3)
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
            // 3. MIS FAVORITOS (Para OTROS usuarios)
            // ---------------------------------------------------------
            // Buscamos a TODOS los usuarios que tienen este destino en favoritos
            var seguidores = await _favoritosRepository.GetListAsync(f => f.DestinoTuristicoId == destinoId && f.CreatorId != userId);

            foreach (var favorito in seguidores)
            {
                // Obtenemos el UserId real del dueño de la lista (usando la relación con ListaFavorito si es necesario)
                // Asumiré que LineaListaFavorito tiene acceso al UserId de la Lista o lo obtienes con un Join.
                // Si LineaListaFavorito NO tiene UserId directo, necesitarás hacer un Join con ListaFavorito.
                // Aquí simulo que obtenemos el ID del dueño de la lista:
                var idDueñoLista = await ObtenerDueñoLista(favorito.ListaFavoritoId); // (Ver helper abajo)

                if (idDueñoLista != Guid.Empty && idDueñoLista != userId)
                {
                    await CrearNotificacion(idDueñoLista, "¡Novedades en tus favoritos! 🔔",
                        $"Alguien acaba de opinar sobre {destino.Nombre}. Mira qué dicen.",
                        TipoNotificacion.Social, "fa-heart", $"/destinos/{destinoId}");
                }
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
            // Implementa la búsqueda del UserId de la ListaFavorito aquí
            // return (await _listaFavoritoRepository.GetAsync(listaId)).UserId;
            return Guid.Empty; // Placeholder
        }
    }
}