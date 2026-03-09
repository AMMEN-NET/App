using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Notificaciones
{
    /// <summary>
    /// Cola de emails pendientes para el resumen semanal.
    /// Se procesan por un worker semanal que los agrupa por usuario.
    /// </summary>
    public class ColaResumenSemanalEmail : CreationAuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string EmailDestino { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public bool Procesado { get; set; }

        protected ColaResumenSemanalEmail() { } // Constructor para EF Core

        public ColaResumenSemanalEmail(Guid id, Guid userId, string emailDestino, string titulo, string mensaje)
            : base(id)
        {
            UserId = userId;
            EmailDestino = emailDestino;
            Titulo = titulo;
            Mensaje = mensaje;
            Procesado = false;
        }
    }
}
