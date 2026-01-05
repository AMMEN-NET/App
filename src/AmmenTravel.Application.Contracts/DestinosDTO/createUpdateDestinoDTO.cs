using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmenTravel.DestinosDTO
{

    public class CreateUpdateDestinoDTO
    {
        [Required]
        [StringLength(128)]

        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Pais { get; set; } = string.Empty;

        [Required]
        public int Poblacion { get; set; }

        [Required]
        public float Latitud { get; set; }
        [Required]
        public float Longitud { get; set; }

        [Required]
        public string? IdExterno { get; set; } // Para guardar el ID de GeoDB

    }


}

