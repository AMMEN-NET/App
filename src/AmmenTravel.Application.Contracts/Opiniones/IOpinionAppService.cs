using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using AmmenTravel.Opiniones.OpinionesDTO;

namespace AmmenTravel.Opiniones
{
    public interface IOpinionAppService : IApplicationService
    {
        Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input);
        Task<OpinionDto> ActualizarOpinionAsync(Guid opinionId, createUpdateOpinionDto input);
        Task EliminarOpinionAsync(Guid opinionId);
        Task<List<OpinionDto>> ObtenerPorUsuarioAsync(Guid usuarioId);
        Task<List<OpinionPublicaDto>> ObtenerListaPublicaPorDestinoAsync(string idExternoGeoDB);
        Task<bool> EsOpinionadoAsync(Guid destinoId);
    }
}