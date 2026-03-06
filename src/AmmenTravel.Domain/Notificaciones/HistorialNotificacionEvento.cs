using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Notificaciones
{
    // Usamos CreationAuditedEntity para saber automáticamente cuándo se le notificó
    public class HistorialNotificacionEvento : CreationAuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid DestinoTuristicoId { get; set; }
        public string EventoTicketmasterId { get; set; } // El string ID alfanumérico que manda la API

        protected HistorialNotificacionEvento() { } // Constructor para EF Core

        public HistorialNotificacionEvento(Guid id, Guid userId, Guid destinoTuristicoId, string eventoTicketmasterId)
            : base(id)
        {
            UserId = userId;
            DestinoTuristicoId = destinoTuristicoId;
            EventoTicketmasterId = eventoTicketmasterId;
        }
    }
}