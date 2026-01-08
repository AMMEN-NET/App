using System;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.Experiencias
{
    public class ExperienciaDto : FullAuditedEntityDto<Guid>
    {
        public Guid DestinoId { get; set; }
        public string DestinoNombre { get; set; } // <--- AGREGADO: Para mostrar el nombre del lugar
        public TipoExperiencia Valoracion { get; set; }
        public string Comentario { get; set; }
        public string UserName { get; set; }
    }
}