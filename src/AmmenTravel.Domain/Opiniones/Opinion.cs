using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Opiniones
{
    public class Opinion : FullAuditedEntity<Guid> 
    {
        public required ValorPuntuacion Puntuacion { get; set; }
        public required string Comentario { get; set; }
        public required Guid DestinoTuristicoId { get; set; } //Es para hacer la foreign key
        public required Guid UsuarioId { get; set; } //Es para hacer la foreign key
        
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
