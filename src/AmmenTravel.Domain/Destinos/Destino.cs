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
        public string nombre { get; set; }

        public string pais { get; set; }

        public int poblacion { get; set; }

        public string fotoURL { get; set; }

        /* NO HACE FALTA DECLARAR ID, CREATION TIME, NI LAST MODIFICATION TIME. AuditedAggregateRoot ya lo hace */
    }
}
