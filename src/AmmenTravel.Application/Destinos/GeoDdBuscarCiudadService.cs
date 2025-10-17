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

        public async Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            var result = new CiudadResultadoDTO();

            if (string.IsNullOrWhiteSpace(request?.Nombre))
                return result;

            var url = $"{BaseUrl}/cities?namePrefix={Uri.EscapeDataString(request.Nombre)}&limit=5";
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
                }).ToList();

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
        }
    }
}