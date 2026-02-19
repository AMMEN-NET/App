using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using AmmenTravel.ListaFavoritos;
using Microsoft.AspNetCore.Authorization;
using AmmenTravel.Opiniones;    // Para la entidad Opinion
using AmmenTravel.Experiencias; // Para la entidad Experiencia
using Volo.Abp.Identity;        // Para la entidad IdentityUser

namespace AmmenTravel.Estadisticas
{
    // [Authorize(AmmenTravelPermissions.Dashboard.Host)]
    public class DashboardAppService : AmmenTravelAppService, IDashboardAppService
    {
        private readonly IRepository<HistorialBusqueda, Guid> _historialRepository;
        private readonly IRepository<RegistroApiExterna, Guid> _registroApiRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaFavoritoRepository;

        // Nuevos repositorios inyectados
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<Experiencia, Guid> _experienciaRepository;
        private readonly IRepository<IdentityUser, Guid> _usuarioRepository;

        public DashboardAppService(
            IRepository<HistorialBusqueda, Guid> historialRepository,
            IRepository<RegistroApiExterna, Guid> registroApiRepository,
            IRepository<LineaListaFavorito, Guid> lineaFavoritoRepository,
            // Inyectamos los nuevos en el constructor
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<Experiencia, Guid> experienciaRepository,
            IRepository<IdentityUser, Guid> usuarioRepository)
        {
            _historialRepository = historialRepository;
            _registroApiRepository = registroApiRepository;
            _lineaFavoritoRepository = lineaFavoritoRepository;

            // Asignamos las variables
            _opinionRepository = opinionRepository;
            _experienciaRepository = experienciaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<DashboardResumenDto> GetResumenAsync()
        {
            // 1. Obtener Métricas de Búsqueda (Historial)
            // El historial a veces es privado, si implementa IUserOwned, hay que ignorar filtro también.
            var totalBusquedas = await (await _historialRepository.GetQueryableAsync())
                                        .IgnoreQueryFilters()
                                        .CountAsync();

            // Top 5 Búsquedas
            var queryBusquedas = await _historialRepository.GetQueryableAsync();
            var topBusquedas = await queryBusquedas
                .IgnoreQueryFilters() // <--- IMPORTANTE
                .GroupBy(x => x.TerminoBusqueda)
                .Select(g => new TerminoBusquedaDto
                {
                    Termino = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();

            // 2. Obtener Métricas de Favoritos (Globales)
            // Si LineaListaFavorito es IUserOwned, usa esto. Si no, no hace daño ponerlo.
            var totalFavoritos = await (await _lineaFavoritoRepository.GetQueryableAsync())
                                        .IgnoreQueryFilters()
                                        .Where(x => !x.IsDeleted)
                                        .CountAsync();

            // 3. NUEVAS MÉTRICAS: Usando IgnoreQueryFilters
            // -------------------------------------------------------------------------

            // Contar TODAS las opiniones (ignorando que sean de otros usuarios)
            // Nota: Agregamos !x.IsDeleted porque IgnoreQueryFilters también ignora el SoftDelete
            var queryOpiniones = await _opinionRepository.GetQueryableAsync();
            var queryUsuarios = await _usuarioRepository.GetQueryableAsync();

            var totalOpiniones = await queryOpiniones
            .IgnoreQueryFilters()
            .Where(o => !o.IsDeleted) // Opinión activa
            .Join(
                queryUsuarios.IgnoreQueryFilters().Where(u => !u.IsDeleted), // Join con Usuarios Activos
                opinion => opinion.UserId,
                usuario => usuario.Id,
                (opinion, usuario) => opinion
            )
            .CountAsync();

            // Contar TODAS las experiencias
            var totalExperiencias = await (await _experienciaRepository.GetQueryableAsync())
                                        .IgnoreQueryFilters()
                                        .Where(x => !x.IsDeleted)
                                        .CountAsync();

            // Usuarios (IdentityUser no suele tener el filtro IUserOwned custom, pero por las dudas)
            var totalUsuarios = await _usuarioRepository.GetCountAsync();
            // -------------------------------------------------------------------------

            // 4. Obtener Métricas de API Externa
            // Estas tablas suelen ser logs del sistema, no siempre tienen UserId.
            var totalLlamadas = await _registroApiRepository.GetCountAsync();
            var totalErrores = await _registroApiRepository.CountAsync(x => !x.FueExitoso);

            double tiempoPromedio = 0;
            if (totalLlamadas > 0)
            {
                var queryApi = await _registroApiRepository.GetQueryableAsync();
                tiempoPromedio = await queryApi.AverageAsync(x => x.TiempoDuracionMs);
            }

            return new DashboardResumenDto
            {
                TotalBusquedas = totalBusquedas,
                TopBusquedas = topBusquedas,
                TotalDestinosGuardados = totalFavoritos,

                TotalOpiniones = totalOpiniones,       // Ahora debería darte el número correcto
                TotalExperiencias = totalExperiencias,
                TotalUsuarios = (int)totalUsuarios,

                TotalLlamadasApi = (int)totalLlamadas,
                TotalErroresApi = (int)totalErrores,
                TiempoPromedioRespuestaMs = Math.Round(tiempoPromedio, 2)
            };
        }
    }
}