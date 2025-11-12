using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmenTravel.ExternalService
{
    public interface IBuscarCiudadService
    {
        Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request);

    }
}