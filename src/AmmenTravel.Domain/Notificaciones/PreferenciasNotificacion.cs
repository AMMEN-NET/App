using System;
using Volo.Abp.Domain.Entities;

namespace AmmenTravel.Notificaciones
{
    /// <summary>
    /// Preferencias de notificación del usuario.
    /// Relación 1:1 con el usuario de Identity.
    /// Si no existe registro para un usuario, se asumen valores por defecto:
    /// EnPantalla = true, PorEmail = true, Frecuencia = Inmediata
    /// </summary>
    public class PreferenciasNotificacion : Entity<Guid>
    {
        public Guid UserId { get; set; }
        public bool EnPantalla { get; set; }
        public bool PorEmail { get; set; }
        public FrecuenciaNotificacion Frecuencia { get; set; }

        protected PreferenciasNotificacion() { } // Constructor para EF Core

        public PreferenciasNotificacion(Guid id, Guid userId, bool enPantalla = true, bool porEmail = true, FrecuenciaNotificacion frecuencia = FrecuenciaNotificacion.Inmediata)
            : base(id)
        {
            UserId = userId;
            EnPantalla = enPantalla;
            PorEmail = porEmail;
            Frecuencia = frecuencia;
        }
    }
}
