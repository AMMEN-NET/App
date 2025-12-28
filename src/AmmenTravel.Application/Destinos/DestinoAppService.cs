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
using Microsoft.AspNetCore.Mvc;

namespace AmmenTravel.Destinos
{
    [Authorize]
    public class DestinoAppService :
          CrudAppService<
              DestinoTuristico,
              guardarDestinoDTO,
              Guid,
              PagedAndSortedResultRequestDto,
              CreateUpdateDestinoDTO>,
          IDestinoAppService
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

        // --- SOBRESCRIBIMOS EL MÉTODO CREATE ASYNC ---
        // Esto intercepta la llamada del frontend cuando hace "this.destinoService.create(...)"
        public override async Task<guardarDestinoDTO> CreateAsync(CreateUpdateDestinoDTO input)
        {
            DestinoTuristico destinoExistente = null;

            // 1. Intentamos buscar por ID Externo (GeoDB) si viene en el input
            if (!string.IsNullOrEmpty(input.IdExterno))
            {
                destinoExistente = await Repository.FirstOrDefaultAsync(d => d.IdExterno == input.IdExterno);
            }

            // 2. Si no se encontró (o no había ID externo), buscamos por Nombre + País como respaldo
            if (destinoExistente == null)
            {
                destinoExistente = await Repository.FirstOrDefaultAsync(d => d.Nombre == input.Nombre && d.Pais == input.Pais);
            }

            // 3. Si YA existe, NO lo creamos de nuevo. Devolvemos el existente mapeado al DTO.
            if (destinoExistente != null)
            {
                // Mapeamos la entidad existente al DTO de respuesta
                return ObjectMapper.Map<DestinoTuristico, guardarDestinoDTO>(destinoExistente);
            }

            // 4. Si NO existe, dejamos que la lógica base lo cree.
            return await base.CreateAsync(input);
        }

        // Este método queda por si lo usas manualmente en otro lado, 
        // pero el CreateAsync de arriba es el que arregla el problema del frontend.
        public async Task<Guid> BuscarOCrearDestinoDesdeApiAsync(CiudadDTO input)
        {
            var destinoExistente = !string.IsNullOrEmpty(input.GeoDBId)
                ? await Repository.FirstOrDefaultAsync(d => d.IdExterno == input.GeoDBId)
                : await Repository.FirstOrDefaultAsync(d => d.Nombre == input.Nombre && d.Pais == input.Pais);

            if (destinoExistente != null)
            {
                return destinoExistente.Id;
            }

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