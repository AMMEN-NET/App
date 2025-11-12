using AmmenTravel.DestinosDTO;
using AmmenTravel.ExternalService;
using AmmenTravel.InterfaceDestinoAppService;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AmmenTravel.Destinos
{
    public class DestinoAppService :
          CrudAppService<
              DestinoTuristico, //The Destino entity
              guardarDestinoDTO, //Used to show destinos
              Guid, //Primary key of the destino entity
              PagedAndSortedResultRequestDto, //Used for paging/sorting
              CreateUpdateDestinoDTO>, //Used to create/update a destino
          IDestinoAppService //implement the IDestinoAppService
    {
        private readonly IBuscarCiudadService _buscarCiudadService;

        public DestinoAppService(IRepository<DestinoTuristico, Guid> repository, IBuscarCiudadService buscarCiudadService)
            : base(repository)
        {
            _buscarCiudadService = buscarCiudadService;
        }

        public async Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            return await _buscarCiudadService.BuscarCiudadesAsync(request);
        }
    }
}