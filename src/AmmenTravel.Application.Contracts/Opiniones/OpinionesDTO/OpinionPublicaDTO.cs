using System;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.Opiniones.OpinionesDTO
{
    public class OpinionPublicaDto : EntityDto<Guid>
    {
        public string NombreUsuario { get; set; } // Para mostrar "Juan Pérez"
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
        public DateTime CreationTime { get; set; } // Para mostrar "Hace 2 días"
    }
}