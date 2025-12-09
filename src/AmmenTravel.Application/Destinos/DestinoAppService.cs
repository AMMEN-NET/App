using AmmenTravel.DestinosDTO;
using AmmenTravel.ExternalService;
using AmmenTravel.InterfaceDestinoAppService;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using AmmenTravel.Permissions;
using AmmenTravel.Destinos;
using Microsoft.AspNetCore.Mvc;

namespace AmmenTravel.Destinos
{
    // Aplicar el atributo de autorización base
    [Authorize]
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

        [HttpGet("buscar-ciudades")]
        public async Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            return await _buscarCiudadService.BuscarCiudadesAsync(request);
        }

        public async Task<Guid> BuscarOCrearDestinoDesdeApiAsync(CiudadDTO input)
        {
            // Validamos que tengamos un ID externo para buscar
            // Si el DTO vino sin ID, intentamos buscar por Nombre+Pais como respaldo
            var destinoExistente = !string.IsNullOrEmpty(input.GeoDBId)
                ? await Repository.FirstOrDefaultAsync(d => d.IdExterno == input.GeoDBId)
                : await Repository.FirstOrDefaultAsync(d => d.Nombre == input.Nombre && d.Pais == input.Pais);

            if (destinoExistente != null)
            {
                return destinoExistente.Id;
            }

            // 2. Mapear datos. 

            var nuevoDestino = new DestinoTuristico(GuidGenerator.Create())
            {
                Nombre = input.Nombre,
                Pais = input.Pais,
                Poblacion = input.Poblacion,
                Latitud = input.Latitud,
                Longitud = input.Longitud,
                IdExterno = input.GeoDBId
            };

            await Repository.InsertAsync(nuevoDestino, autoSave: true);

            return nuevoDestino.Id;
        }
    }
}