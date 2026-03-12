namespace AmmenTravel.Notificaciones
{
    public class UpdatePreferenciasDto
    {
        public bool EnPantalla { get; set; }
        public bool PorEmail { get; set; }
        public FrecuenciaNotificacion Frecuencia { get; set; }
    }
}
