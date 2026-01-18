using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity; // Para usuarios
using AmmenTravel.Opiniones;

namespace AmmenTravel.Notificaciones
{
    public class ReactivacionUsuarioJob : AsyncBackgroundJob<ReactivacionArgs>, ITransientDependency
    {
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;

        public ReactivacionUsuarioJob(
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<Notificacion, Guid> notificacionRepository)
        {
            _userRepository = userRepository;
            _opinionRepository = opinionRepository;
            _notificacionRepository = notificacionRepository;
        }

        public override async Task ExecuteAsync(ReactivacionArgs args)
        {
            // Lógica: Buscar usuarios cuya última opinión fue hace > 30 días
            // Esto es pesado si tienes millones de usuarios, idealmente se hace con SQL crudo o Batch.
            // Para el MVP, lo hacemos simple:

            var usuarios = await _userRepository.GetListAsync();

            foreach (var usuario in usuarios)
            {
                var ultimaOpinion = await _opinionRepository.FirstOrDefaultAsync(o => o.UserId == usuario.Id);

                // Si nunca opinó o su última opinión es vieja
                bool estaInactivo = false;
                if (ultimaOpinion != null && ultimaOpinion.CreationTime < DateTime.Now.AddDays(-30))
                {
                    estaInactivo = true;
                }
                else if (ultimaOpinion == null && usuario.CreationTime < DateTime.Now.AddDays(-15))
                {
                    // Usuario registrado hace 15 días pero sin opiniones
                    estaInactivo = true;
                }

                if (estaInactivo)
                {
                    // Verificar si ya le mandamos notificación recientemente para no hacer spam
                    var spamCheck = await _notificacionRepository.AnyAsync(n => n.UserId == usuario.Id && n.Tipo == TipoNotificacion.Recordatorio && n.CreationTime > DateTime.Now.AddDays(-7));

                    if (!spamCheck)
                    {
                        // Crear Notificación
                        // ... InsertAsync ...
                    }
                }
            }
        }
    }

    public class ReactivacionArgs { }
}