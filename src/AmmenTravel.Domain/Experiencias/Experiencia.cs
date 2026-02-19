using System;
using Volo.Abp.Domain.Entities.Auditing;
using AmmenTravel.Destinos;

namespace AmmenTravel.Experiencias
{
    public class Experiencia : FullAuditedEntity<Guid>
    {
        public Guid DestinoId { get; set; }

        public virtual DestinoTuristico Destino { get; set; }

        public TipoExperiencia Valoracion { get; set; } // Malo, Neutral, Bueno
        public string Comentario { get; set; }

        // Constructores
        protected Experiencia() { }

        public Experiencia(Guid id, Guid destinoId, TipoExperiencia valoracion, string comentario)
            : base(id)
        {
            DestinoId = destinoId;
            Valoracion = valoracion;
            Comentario = comentario;
        }
    }
}