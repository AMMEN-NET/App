using System.ComponentModel.DataAnnotations;

namespace AmmenTravel.ExternalService
{
    public class CiudadBuscadaDTO
    {
        [Required]
        [StringLength(128)]
        public string Nombre { get; set; }

        public string? Pais { get; set; }

        public int? PoblacionMinima { get; set; }
    }
}