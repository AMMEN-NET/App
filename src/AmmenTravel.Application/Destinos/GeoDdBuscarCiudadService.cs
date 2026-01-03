using AmmenTravel.ExternalService;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using AmmenTravel.Destinos; 
using AmmenTravel.Opiniones; 

namespace AmmenTravel.Application.ExternalServices
{
    public class GeoDdBuscarCiudadService : IBuscarCiudadService
    {
        private const string rapidApiKey = "28b89efbaemsh750f2ed4984863ap14d14djsnd3061a788080";
        private const string rapidApiHost = "wft-geo-db.p.rapidapi.com";
        private const string BaseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";
        private readonly HttpClient _httpClient;

        // --- INYECCIÓN DE REPOSITORIOS ---
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        public GeoDdBuscarCiudadService(
            HttpClient httpClient,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<Opinion, Guid> opinionRepository)
        {
            _httpClient = httpClient;
            _destinoRepository = destinoRepository;
            _opinionRepository = opinionRepository;
        }

        public async Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            var result = new CiudadResultadoDTO();

            if (string.IsNullOrWhiteSpace(request?.Nombre))
                return result;

            var queryParams = new List<string>
            {
                $"namePrefix={Uri.EscapeDataString(request.Nombre)}",
                "limit=10"
            };

            if (request.PoblacionMinima.HasValue && request.PoblacionMinima.Value > 0)
            {
                queryParams.Add($"minPopulation={request.PoblacionMinima.Value}");
            }

            var url = $"{BaseUrl}/cities?{string.Join("&", queryParams)}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("X-RapidAPI-Key", rapidApiKey);
            httpRequest.Headers.Add("X-RapidAPI-Host", rapidApiHost);

            try
            {
                var response = await _httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                    return result;

                var json = await response.Content.ReadFromJsonAsync<GeoDbResponse>();
                if (json?.Data == null)
                    return new CiudadResultadoDTO { Ciudades = new List<CiudadDTO>() };

                // 1. Mapeo inicial de datos externos
                var cities = json.Data.Select(c => new CiudadDTO
                {
                    Nombre = c.City ?? string.Empty,
                    Pais = c.Country ?? string.Empty,
                    Poblacion = c.Population ?? 0,
                    Latitud = c.Latitude ?? 0,
                    Longitud = c.Longitude ?? 0,
                    GeoDBId = c.Id?.ToString() ?? string.Empty
                })
                .Where(c =>
                    (string.IsNullOrWhiteSpace(request.Pais) ||
                        c.Pais.Contains(request.Pais, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Pais, request.Pais, StringComparison.OrdinalIgnoreCase))
                    &&
                    (!request.PoblacionMinima.HasValue || c.Poblacion >= request.PoblacionMinima.Value)
                )
                .ToList();

                // --- LÓGICA DE PROMEDIOS ---
                if (cities.Any())
                {
                    // Obtenemos los IDs externos de las ciudades encontradas
                    var externalIds = cities.Where(c => !string.IsNullOrEmpty(c.GeoDBId))
                                            .Select(c => c.GeoDBId)
                                            .ToList();

                    // Buscamos cuáles de esas ciudades ya existen en nuestra BD local
                    var destinosLocales = await _destinoRepository.GetListAsync(d => externalIds.Contains(d.IdExterno));

                    if (destinosLocales.Any())
                    {
                        var idsLocales = destinosLocales.Select(d => d.Id).ToList();

                        // Traemos las opiniones de esos destinos (ABP filtra automáticamente los SoftDeleted)
                        var opiniones = await _opinionRepository.GetListAsync(o => idsLocales.Contains(o.DestinoTuristicoId) && !o.IsDeleted);

                        // Cruzamos la información
                        foreach (var city in cities)
                        {
                            var destinoLocal = destinosLocales.FirstOrDefault(d => d.IdExterno == city.GeoDBId);
                            if (destinoLocal != null)
                            {
                                var opinionesDelDestino = opiniones.Where(o => o.DestinoTuristicoId == destinoLocal.Id).ToList();

                                if (opinionesDelDestino.Any())
                                {
                                    city.CantidadOpiniones = opinionesDelDestino.Count;
                                    // Calculamos promedio
                                    city.PromedioPuntuacion = opinionesDelDestino.Average(o => (int)o.Puntuacion);
                                }
                            }
                        }
                    }
                }

                return new CiudadResultadoDTO { Ciudades = cities };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar ciudades: {ex.Message}");
            }
        }

        private class GeoDbResponse
        {
            public List<GeoDbCity> Data { get; set; } = new();
        }

        private class GeoDbCity
        {
            public string? City { get; set; }
            public string? Country { get; set; }
            public int? Population { get; set; }
            public float? Latitude { get; set; }
            public float? Longitude { get; set; }
            public int? Id { get; set; }
        }
    }
}