using AmmenTravel.Opiniones.OpinionesDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmenTravel.Opiniones
{
    public interface ICreateUpdateOpinion
    {
        Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input);
        Task<OpinionDto> ActualizarOpinionAsync(Guid opinionId, createUpdateOpinionDto input);
        Task EliminarOpinionAsync(Guid opinionId);
    }
}