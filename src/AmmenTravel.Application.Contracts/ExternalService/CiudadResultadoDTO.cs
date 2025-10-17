using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AmmenTravel.ExternalService
{
    public class CiudadResultadoDTO
    {
        public List<CiudadDTO> Ciudades { get; set; } = new();
    }
}