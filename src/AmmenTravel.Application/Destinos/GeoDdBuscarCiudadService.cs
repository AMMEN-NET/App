using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AmmenTravel.ExternalService;
using Volo.Abp.DependencyInjection;
using System.Linq;
using System;

namespace AmmenTravel.Application.ExternalServices
{
    public class GeoDbService : IBuscarCiudadService, ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://wft-geo-db.p.rapidapi.com/v1/geo";

        public GeoDbService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "28b89efbaemsh750f2ed4984863ap14d14djsnd3061a788080");
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "wft-geo-db.p.rapidapi.com");
        }

        public async Task<string> BuscarCiudadAsync(string nombreCiudad)
        {
            var url = $"/cities?namePrefix={nombreCiudad}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public Task<CiudadResultadoDTO> BuscarCiudadesAsync(CiudadBuscadaDTO request)
        {
            throw new System.NotImplementedException();
        }
    }
}