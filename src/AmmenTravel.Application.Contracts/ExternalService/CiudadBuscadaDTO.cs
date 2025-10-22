using System.ComponentModel.DataAnnotations;

namespace AmmenTravel.ExternalService
{
    public class CiudadBuscadaDTO
    {
        [Required]
        [StringLength(128)]
        public string Nombre { get; set; }
     
    }
}