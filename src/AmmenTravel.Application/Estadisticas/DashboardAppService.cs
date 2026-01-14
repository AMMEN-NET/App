using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using AmmenTravel.ListaFavoritos; // Para contar favoritos
using Microsoft.AspNetCore.Authorization; // Para seguridad

namespace AmmenTravel.Estadisticas
{
    // [Authorize(AmmenTravelPermissions.Dashboard.Host)] // Podrías descomentar esto luego para seguridad
    public class DashboardAppService : AmmenTravelAppService, IDashboardAppService
    {
        private readonly IRepository<HistorialBusqueda, Guid> _historialRepository;
        private readonly IRepository<RegistroApiExterna, Guid> _registroApiRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaFavoritoRepository;

        public DashboardAppService(
            IRepository<HistorialBusqueda, Guid> historialRepository,
            IRepository<RegistroApiExterna, Guid> registroApiRepository,
            IRepository<LineaListaFavorito, Guid> lineaFavoritoRepository)
        {
            _historialRepository = historialRepository;
            _registroApiRepository = registroApiRepository;
            _lineaFavoritoRepository = lineaFavoritoRepository;
        }

        public async Task<DashboardResumenDto> GetResumenAsync()
        {
            // 1. Obtener Métricas de Búsqueda
            var totalBusquedas = await _historialRepository.GetCountAsync();

            // Para el Top 5, necesitamos acceder al Queryable para usar GroupBy de SQL
            var queryBusquedas = await _historialRepository.GetQueryableAsync();

            var topBusquedas = await queryBusquedas
                .GroupBy(x => x.TerminoBusqueda)
                .Select(g => new TerminoBusquedaDto
                {
                    Termino = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();

            // 2. Obtener Métricas de Favoritos (Destinos guardados)
            var totalFavoritos = await _lineaFavoritoRepository.GetCountAsync();

            // 3. Obtener Métricas de API Externa
            var queryApi = await _registroApiRepository.GetQueryableAsync();

            // Hacemos una sola consulta agregada para eficiencia si es posible, 
            // o traemos datos básicos. Aquí lo haré por separado para claridad y seguridad.
            var totalLlamadas = await _registroApiRepository.GetCountAsync();

            // Contar errores (donde FueExitoso es falso)
            var totalErrores = await _registroApiRepository.CountAsync(x => !x.FueExitoso);

            // Calcular promedio (manejando caso de 0 division)
            double tiempoPromedio = 0;
            if (totalLlamadas > 0)
            {
                tiempoPromedio = await queryApi.AverageAsync(x => x.TiempoDuracionMs);
            }

            return new DashboardResumenDto
            {
                TotalBusquedas = (int)totalBusquedas,
                TopBusquedas = topBusquedas,
                TotalDestinosGuardados = (int)totalFavoritos,
                TotalLlamadasApi = (int)totalLlamadas,
                TotalErroresApi = (int)totalErrores,
                TiempoPromedioRespuestaMs = Math.Round(tiempoPromedio, 2)
            };
        }
    }
}