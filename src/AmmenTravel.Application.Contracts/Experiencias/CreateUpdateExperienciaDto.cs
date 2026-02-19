using System;
using System.ComponentModel.DataAnnotations;

namespace AmmenTravel.Experiencias
{
    public class CreateUpdateExperienciaDto
    {
        [Required]
        public Guid DestinoId { get; set; }

        [Required]
        public TipoExperiencia Valoracion { get; set; } // 0, 1, 2

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Comentario { get; set; }
    }
}