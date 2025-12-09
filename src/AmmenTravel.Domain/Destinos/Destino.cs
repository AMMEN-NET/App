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

        public required float Latitud { get; set; }

        public required float Longitud { get; set; }

        public string? IdExterno { get; set; } // Para guardar el ID de GeoDB

        /* NO HACE FALTA DECLARAR ID, CREATION TIME, NI LAST MODIFICATION TIME. AuditedAggregateRoot ya lo hace */

        // --- ES OBLIGATORIO PARA PASAR EL ID ---
        public DestinoTuristico(Guid id) : base(id) { }

        // --- OBLIGATORIO PARA QUE EF CORE FUNCIONE ---
        protected DestinoTuristico() { }
    }
}
