using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.Experiencias
{
    public interface IExperienciaAppService : IApplicationService
    {
        Task<ExperienciaDto> CreateAsync(CreateUpdateExperienciaDto input);
        Task<ExperienciaDto> UpdateAsync(Guid id, CreateUpdateExperienciaDto input);
        Task DeleteAsync(Guid id);
        Task<List<ExperienciaDto>> GetListAsync(string destinoId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null);
        Task<List<ExperienciaDto>> GetListPorUsuarioAsync(Guid userId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null);
    }
}