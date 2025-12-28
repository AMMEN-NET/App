using AmmenTravel.Common;
using AmmenTravel.Destinos;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Opiniones
{
    public class Opinion : FullAuditedEntity<Guid>, IUserOwned
    {
        // Constructor para uso de la aplicación (asegura invariantes)
        public Opinion(Guid destinoTuristicoId, Guid userId, ValorPuntuacion puntuacion, string comentario)
        {
            DestinoTuristicoId = destinoTuristicoId;
            UserId = userId;
            Puntuacion = puntuacion;
            Comentario = comentario;
        }

        // Constructor parameterless para EF Core
        protected Opinion()
        {
        }

        public  ValorPuntuacion Puntuacion { get; set; }
        public  string? Comentario { get; set; }
        public  Guid DestinoTuristicoId { get; set; } // foreign key
        public  Guid UserId { get; set; } // foreign key

        public virtual DestinoTuristico DestinoTuristico { get; set; }
    }

    public enum ValorPuntuacion
    {
        Uno = 1,
        Dos = 2,
        Tres = 3,
        Cuatro = 4,
        Cinco = 5
    }
}
