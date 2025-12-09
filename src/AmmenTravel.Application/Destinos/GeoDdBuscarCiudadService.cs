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


namespace AmmenTravel.Application.ExternalServices
{
    public class GeoDdBuscarCiudadService : IBuscarCiudadService
    {

        private const string rapidApiKey = "28b89efbaemsh750f2ed4984863ap14d14djsnd3061a788080";
        private const string rapidApiHost = "wft-geo-db.p.rapidapi.com";
        private const string BaseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";
        private readonly HttpClient _httpClient;

        public GeoDdBuscarCiudadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Adaptado: ahora soporta filtros por Pais y PoblacionMinima desde CiudadBuscadaDTO

        public async Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            var result = new CiudadResultadoDTO();

            if (string.IsNullOrWhiteSpace(request?.Nombre))
                return result;

            // Construir parámetros de consulta de forma segura
            var queryParams = new List<string>
            {
                $"namePrefix={Uri.EscapeDataString(request.Nombre)}",
                "limit=10"
            };

            // Añadir filtro de población mínima si se indicó
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
                    // Filtrar por país si se indicó: aceptar coincidencia por inclusión
                    (string.IsNullOrWhiteSpace(request.Pais) ||
                        c.Pais.Contains(request.Pais, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Pais, request.Pais, StringComparison.OrdinalIgnoreCase))
                    &&
                    // Filtrar por población mínima si se indicó
                    (!request.PoblacionMinima.HasValue || c.Poblacion >= request.PoblacionMinima.Value)
                )
                .ToList();

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
            public int? Id { get; set; }   // Le agregue esto para poder saber si una ciudad estaba o no antes en la BD interna (Es el ID de la ciudad).
        }
    }
}