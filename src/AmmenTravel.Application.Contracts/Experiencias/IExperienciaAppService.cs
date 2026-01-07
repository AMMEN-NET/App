using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.Experiencias
{
    public interface IExperienciaAppService : IApplicationService
    {
        // 4.1 Crear
        Task<ExperienciaDto> CreateAsync(CreateUpdateExperienciaDto input);

        // 4.2 Editar
        Task<ExperienciaDto> UpdateAsync(Guid id, CreateUpdateExperienciaDto input);

        // 4.3 Eliminar
        Task DeleteAsync(Guid id);

        // 4.4, 4.5, 4.6 Consultar con filtros
        Task<List<ExperienciaDto>> GetListAsync(Guid destinoId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null);
    }
}