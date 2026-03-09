using System;

namespace AmmenTravel.Notificaciones
{
    public class PreferenciasNotificacionDto
    {
        public Guid? Id { get; set; }
        public bool EnPantalla { get; set; }
        public bool PorEmail { get; set; }
        public FrecuenciaNotificacion Frecuencia { get; set; }
    }
}
