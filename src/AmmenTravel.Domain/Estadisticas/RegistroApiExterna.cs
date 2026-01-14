using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Estadisticas
{
    public class RegistroApiExterna : CreationAuditedEntity<Guid>
    {
        public string NombreApi { get; set; } // Aquí guardaremos "GeoDB Cities"
        public string Endpoint { get; set; }  // Ej: "/v1/geo/cities"
        public int TiempoDuracionMs { get; set; } // Para medir latencia
        public int CodigoEstadoHttp { get; set; } // 200, 404, 429 (límite excedido), etc.
        public bool FueExitoso { get; set; }
        public string? MensajeError { get; set; } // Opcional porque si es 200 me tiraba error

        protected RegistroApiExterna() { }

        public RegistroApiExterna(Guid id, string nombreApi, string endpoint, int tiempoDuracionMs, int codigoEstadoHttp, bool fueExitoso, string mensajeError = null)
            : base(id)
        {
            NombreApi = nombreApi;
            Endpoint = endpoint;
            TiempoDuracionMs = tiempoDuracionMs;
            CodigoEstadoHttp = codigoEstadoHttp;
            FueExitoso = fueExitoso;
            MensajeError = mensajeError;
        }
    }
}