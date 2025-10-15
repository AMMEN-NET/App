using System.ComponentModel.DataAnnotations;

namespace AmmenTravel.ExternalService
{
    public class CiudadDTO
    {
        [Required]
        [StringLength(128)]
        public string Nombre { get; set; }
        public string Pais { get; set; }
    }
}