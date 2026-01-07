using System;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.Experiencias
{
    public class ExperienciaDto : FullAuditedEntityDto<Guid>
    {
        public Guid DestinoId { get; set; }
        public TipoExperiencia Valoracion { get; set; }
        public string Comentario { get; set; }
        public string UserName { get; set; }
    }
}