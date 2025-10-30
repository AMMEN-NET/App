using System;
using System.ComponentModel.DataAnnotations;
using AmmenTravel.Opiniones;

namespace AmmenTravel.Opiniones.OpinionesDTO
{
    public class createUpdateOpinionDto
    {
        [Required]
        public Guid DestinoId { get; set; }

        [Required]
        public ValorPuntuacion Puntuacion { get; set; } // Se enviará como entero desde el cliente

        [Required]
        public string Comentario { get; set; }
    }
}