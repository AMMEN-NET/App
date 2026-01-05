using System.ComponentModel.DataAnnotations;

namespace AmmenTravel.ExternalService
{
    public class CiudadDTO
    {
        [Required]
        [StringLength(128)]
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public int Poblacion { get; set; }
        public float Latitud { get; set; }
        public float Longitud { get; set; }
        public string GeoDBId { get; set; }
        public double? PromedioPuntuacion { get; set; }
        public double ? CantidadOpiniones { get; set; }
    }
}