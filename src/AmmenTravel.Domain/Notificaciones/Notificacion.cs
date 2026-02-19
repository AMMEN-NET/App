using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Entities;
using AmmenTravel.Common; // Asumo que aquí está IUserOwned, si no ajustalo

namespace AmmenTravel.Notificaciones
{
    public class Notificacion : CreationAuditedEntity<Guid>, IUserOwned
    {
        public Guid UserId { get; set; } // El usuario que recibe la notificación
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public TipoNotificacion Tipo { get; set; }
        public bool Leida { get; set; }

        // Datos opcionales para redirigir al usuario al hacer clic
        public string? LinkReferencia { get; set; } // Ej: "/destinos/paris"
        public string? Icono { get; set; } // Ej: "fa-trophy"

        protected Notificacion() { }

        public Notificacion(Guid id, Guid userId, string titulo, string mensaje, TipoNotificacion tipo, string? linkReferencia = null, string? icono = null)
            : base(id)
        {
            UserId = userId;
            Titulo = titulo;
            Mensaje = mensaje;
            Tipo = tipo;
            Leida = false;
            LinkReferencia = linkReferencia;
            Icono = icono;
        }
    }

    public enum TipoNotificacion
    {
        Informativa, // Bienvenida, Sistema
        Exito,       // Logros, Aprobaciones
        Alerta,      // Seguridad
        Social,      // Favoritos, Compañero de Viaje
        Recordatorio // Reactivación
    }
}