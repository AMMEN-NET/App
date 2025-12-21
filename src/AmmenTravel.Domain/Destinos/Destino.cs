using Polly.Simmy.Latency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Destinos
{
    public class DestinoTuristico : AuditedAggregateRoot<Guid>
    {
        public required string Nombre { get; set; }

        public required string Pais { get; set; }

        public required int Poblacion { get; set; }

        public required string FotoURL { get; set; }

        public required float Latitud { get; set; }
        
        public required float Longitud { get; set; }

        /* NO HACE FALTA DECLARAR ID, CREATION TIME, NI LAST MODIFICATION TIME. AuditedAggregateRoot ya lo hace */
    }
}
