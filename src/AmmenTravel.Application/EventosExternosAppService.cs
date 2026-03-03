using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Services;

namespace AmmenTravel.ExternalService
{
    public class EventosExternosAppService : ApplicationService, IEventosExternosAppService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public EventosExternosAppService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public async Task<List<EventoTicketmasterDto>> ObtenerEventosPorUbicacionAsync(string latitud, string longitud)
        {
            var baseUrl = _configuration["Ticketmaster:BaseUrl"];
            var apiKey = _configuration["Ticketmaster:ApiKey"];

            // Construir la URL con coordenadas. El parámetro radius es en millas por defecto.
            var url = $"{baseUrl}events.json?apikey={apiKey}&latlong={latitud},{longitud}&radius=50&sort=date,asc";

            var client = _httpClientFactory.CreateClient("TicketmasterClient");
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Manejar el error apropiadamente (lanzar UserFriendlyException, registrar en log, etc.)
                throw new Exception("Error al consultar la API de Ticketmaster");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Aquí deserializas el JSON complejo de Ticketmaster a tu DTO simplificado
            return ParsearRespuestaTicketmaster(jsonResponse);
        }

        private List<EventoTicketmasterDto> ParsearRespuestaTicketmaster(string json)
        {
            var eventos = new List<EventoTicketmasterDto>();
            using var document = JsonDocument.Parse(json);

            // Ticketmaster envuelve los resultados en _embedded -> events
            if (document.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("events", out var eventsArray))
            {
                foreach (var evt in eventsArray.EnumerateArray())
                {
                    var dto = new EventoTicketmasterDto();

                    // Usamos TryGetProperty para que no explote si la API no manda algún campo
                    if (evt.TryGetProperty("id", out var idElement))
                    {
                        dto.Id = idElement.GetString();
                    }

                    if (evt.TryGetProperty("name", out var nameElement))
                    {
                        dto.Nombre = nameElement.GetString();
                    }

                    if (evt.TryGetProperty("url", out var urlElement))
                    {
                        dto.UrlTicket = urlElement.GetString();
                    }
                    else
                    {
                        // Por si el evento no tiene URL de compra, le ponemos un fallback
                        dto.UrlTicket = "https://www.ticketmaster.com";
                    }

                    eventos.Add(dto);
                }
            }
            return eventos;
        }
    }
}