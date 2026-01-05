using System;
using AmmenTravel.Opiniones;

namespace AmmenTravel.Opiniones.OpinionesDTO
{
    public class OpinionDto
    {
        public Guid Id { get; set; }
        public Guid DestinoTuristicoId { get; set; }
        public string NombreDestino { get; set; }
        public Guid UserId { get; set; }
        public ValorPuntuacion Puntuacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDeleted { get; set; }

        public bool EsFavorito {get; set;}
    }
}