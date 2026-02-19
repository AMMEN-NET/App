using System;
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
        public string? IdExterno { get; set; }

        // Constructor para uso general (con todos los parámetros)
        public DestinoTuristico(Guid id, string nombre, string pais, int poblacion, float latitud, float longitud, string? idExterno = null)
            : base(id)
        {
            Nombre = nombre;
            Pais = pais;
            Poblacion = poblacion;
            Latitud = latitud;
            Longitud = longitud;
            IdExterno = idExterno;
        }

        // Constructor para cuando solo quieres setear el ID y usar inicializador { }
        public DestinoTuristico(Guid id) : base(id) { }

        protected DestinoTuristico() { }
    }
}