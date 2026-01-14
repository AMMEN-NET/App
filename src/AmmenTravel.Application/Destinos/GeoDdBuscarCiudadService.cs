using AmmenTravel.ExternalService;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using AmmenTravel.Destinos;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using AmmenTravel.Estadisticas;
using Volo.Abp.Guids;
using System.Diagnostics; // Para el Stopwatch

namespace AmmenTravel.Application.ExternalServices
{
    public class GeoDdBuscarCiudadService : IBuscarCiudadService
    {
        private const string rapidApiKey = "28b89efbaemsh750f2ed4984863ap14d14djsnd3061a788080";
        private const string rapidApiHost = "wft-geo-db.p.rapidapi.com";
        private const string BaseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";

        private readonly HttpClient _httpClient;

        // --- REPOSITORIOS EXISTENTES ---
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        // --- NUEVAS INYECCIONES PARA ESTADÍSTICAS ---
        private readonly IRepository<HistorialBusqueda, Guid> _historialRepository;
        private readonly IRepository<RegistroApiExterna, Guid> _registroApiRepository;
        private readonly IGuidGenerator _guidGenerator;

        public GeoDdBuscarCiudadService(
            HttpClient httpClient,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<HistorialBusqueda, Guid> historialRepository,
            IRepository<RegistroApiExterna, Guid> registroApiRepository,
            IGuidGenerator guidGenerator)
        {
            _httpClient = httpClient;
            _destinoRepository = destinoRepository;
            _opinionRepository = opinionRepository;
            _historialRepository = historialRepository;
            _registroApiRepository = registroApiRepository;
            _guidGenerator = guidGenerator;
        }

        public async Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            var result = new CiudadResultadoDTO();

            if (string.IsNullOrWhiteSpace(request?.Nombre))
                return result;

            // --- PREPARACIÓN DE URL ---
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

            // VARIABLES PARA MÉTRICAS
            var stopwatch = Stopwatch.StartNew();
            bool fueExitoso = false;
            int codigoEstado = 0;
            string? mensajeError = null;
            int cantidadEncontrada = 0;

            try
            {
                // --- LLAMADA A LA API ---
                var response = await _httpClient.SendAsync(httpRequest);

                stopwatch.Stop(); // Paramos el reloj justo después de recibir respuesta
                codigoEstado = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    mensajeError = $"Error HTTP {codigoEstado}";
                    return result;
                }

                var json = await response.Content.ReadFromJsonAsync<GeoDbResponse>();

                fueExitoso = true; // Si llegamos aquí y el JSON parseó, consideramos éxito técnico

                if (json?.Data == null)
                {
                    // Guardamos historial vacío
                    await GuardarHistorial(request.Nombre, false, request.PoblacionMinima);
                    await GuardarMetricaApi(url, stopwatch.ElapsedMilliseconds, codigoEstado, true, "Data nula");
                    return new CiudadResultadoDTO { Ciudades = new List<CiudadDTO>() };
                }

                var cities = json.Data.Select(c => new CiudadDTO
                {
                    Nombre = c.City ?? string.Empty,
                    Pais = c.Country ?? string.Empty,
                    Poblacion = c.Population ?? 0,
                    Latitud = c.Latitude ?? 0,
                    Longitud = c.Longitude ?? 0,
                    GeoDBId = c.Id?.ToString() ?? string.Empty
                })
                .Where(c => (!request.PoblacionMinima.HasValue || c.Poblacion >= request.PoblacionMinima.Value) &&
                            (string.IsNullOrWhiteSpace(request.Pais) || (c.Pais != null && c.Pais.Contains(request.Pais, StringComparison.OrdinalIgnoreCase))))
                .ToList();

                cantidadEncontrada = cities.Count;

                // --- LÓGICA DE PROMEDIO GLOBAL (EXISTENTE) ---
                if (cities.Any())
                {
                    var externalIds = cities.Where(c => !string.IsNullOrEmpty(c.GeoDBId)).Select(c => c.GeoDBId).ToList();
                    var destinosLocales = await _destinoRepository.GetListAsync(d => externalIds.Contains(d.IdExterno));

                    if (destinosLocales.Any())
                    {
                        var idsLocales = destinosLocales.Select(d => d.Id).ToList();
                        var queryable = await _opinionRepository.GetQueryableAsync();
                        var opiniones = await queryable
                            .IgnoreQueryFilters()
                            .Where(o => idsLocales.Contains(o.DestinoTuristicoId) && !o.IsDeleted)
                            .ToListAsync();

                        foreach (var city in cities)
                        {
                            var destinoLocal = destinosLocales.FirstOrDefault(d => d.IdExterno == city.GeoDBId);
                            if (destinoLocal != null)
                            {
                                var opinionesDelDestino = opiniones.Where(o => o.DestinoTuristicoId == destinoLocal.Id).ToList();
                                if (opinionesDelDestino.Any())
                                {
                                    city.CantidadOpiniones = opinionesDelDestino.Count;
                                    city.PromedioPuntuacion = opinionesDelDestino.Average(o => (int)o.Puntuacion);
                                }
                            }
                        }
                    }
                }

                // --- GUARDAR ESTADÍSTICAS AL FINAL DEL PROCESO EXITOSO ---
                await GuardarHistorial(request.Nombre, cantidadEncontrada > 0, request.PoblacionMinima);
                await GuardarMetricaApi(url, stopwatch.ElapsedMilliseconds, codigoEstado, true, null);

                return new CiudadResultadoDTO { Ciudades = cities };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // En caso de excepción, guardamos el fallo en la métrica
                await GuardarMetricaApi(url, stopwatch.ElapsedMilliseconds, 500, false, ex.Message);
                throw new Exception($"Error al buscar ciudades: {ex.Message}");
            }
        }

        // --- MÉTODOS PRIVADOS PARA GUARDAR DATOS ---

        private async Task GuardarHistorial(string termino, bool encontro, int? poblacionMinima)
        {
            try
            {
                string filtros = poblacionMinima.HasValue ? $"MinPop: {poblacionMinima}" : "Ninguno";
                var historial = new HistorialBusqueda(
                    _guidGenerator.Create(),
                    termino,
                    encontro,
                    filtros
                );
                await _historialRepository.InsertAsync(historial);
            }
            catch
            {
                // No queremos que falle la búsqueda si falla el guardado del historial
            }
        }

        private async Task GuardarMetricaApi(string url, long duracion, int codigo, bool exito, string? error)
        {
            try
            {
                var registro = new RegistroApiExterna(
                    _guidGenerator.Create(),
                    "GeoDB Cities",
                    url,
                    (int)duracion,
                    codigo,
                    exito,
                    error
                );
                await _registroApiRepository.InsertAsync(registro);
            }
            catch
            {
                // Ignorar errores de log
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