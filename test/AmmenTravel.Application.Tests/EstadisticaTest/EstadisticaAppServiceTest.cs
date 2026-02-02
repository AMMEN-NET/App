using AmmenTravel.Destinos;
using AmmenTravel.Estadisticas;
using AmmenTravel.Experiencias;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using Autofac.Core;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Xunit;
using Microsoft.EntityFrameworkCore;


namespace AmmenTravel.EstadisticaTest
{
    public abstract class classEstadisticaAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
      where TStartupModule : IAbpModule
    {
        private readonly DashboardAppService _service;
        private readonly IRepository<HistorialBusqueda, Guid> _historialRepo;
        public classEstadisticaAppServiceTest() { _service = GetRequiredService<DashboardAppService>(); _historialRepo = GetRequiredService<IRepository<HistorialBusqueda, Guid>>(); }


        [Fact]
        public async Task GetResumenAsync_CuentaBusquedasCorrectamente()
        {
            await WithUnitOfWorkAsync(async () => // Ejecuta el bloque dentro de una UnitOfWork para compartir el mismo DbContext y transacción.
            {
                // Resolvemos las dependencias dentro del UnitOfWork para que compartan el mismo DbContext
                var service = GetRequiredService<DashboardAppService>();
                var historialRepo = GetRequiredService<IRepository<HistorialBusqueda, Guid>>();

                // Arrange: insertamos y forzamos guardado (autoSave: true)
                await historialRepo.InsertAsync(
                    new HistorialBusqueda(Guid.NewGuid(), "Buenos Aires", true, "minPoblacion=100000"),
                    autoSave: true);// Inserta y guarda inmediatamente para que los datos estén disponibles en el mismo UoW.


                await historialRepo.InsertAsync(
                    new HistorialBusqueda(Guid.NewGuid(), "Chile", true, ""),
                    autoSave: true);

                await historialRepo.InsertAsync(
                    new HistorialBusqueda(Guid.NewGuid(), "Mendoza", true, "maxPoblacion=100000"),
                    autoSave: true);

                // Act: llamamos al servicio resuelto dentro del mismo UoW
                var resumen = await service.GetResumenAsync();

                // Assert
                Assert.Equal(3, resumen.TotalBusquedas);
            });
        }



        [Fact]
        public async Task GetResumenAsync_RetornaTopBusquedas()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Resolvemos dentro del UoW para compartir el mismo DbContext
                var service = GetRequiredService<DashboardAppService>();
                var historialRepo = GetRequiredService<IRepository<HistorialBusqueda, Guid>>();

                // Arrange: insertamos y guardamos para que la lectura posterior vea los datos
                await historialRepo.InsertAsync(new HistorialBusqueda(Guid.NewGuid(), "Mendoza", true, ""), autoSave: true);
                await historialRepo.InsertAsync(new HistorialBusqueda(Guid.NewGuid(), "Mendoza", true, ""), autoSave: true);
                await historialRepo.InsertAsync(new HistorialBusqueda(Guid.NewGuid(), "Chile", true, ""), autoSave: true);

                // Act
                var resumen = await service.GetResumenAsync();

                // Assert
                Assert.Equal("Mendoza", resumen.TopBusquedas.First().Termino);
                Assert.Equal(2, resumen.TopBusquedas.First().Cantidad);
            });
        }
        [Fact]
        public async Task GetResumenAsync_CuentaFavoritosCorrectamente()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Resolvemos servicios/repositorios dentro del UoW
                var service = GetRequiredService<DashboardAppService>();
                var listaRepo = GetRequiredService<IRepository<ListaFavorito, Guid>>();
                var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
                var lineaRepo = GetRequiredService<IRepository<LineaListaFavorito, Guid>>();

                // 1) Crear la lista padre (ListaFavorito)
                var lista = new ListaFavorito
                {
                    UserId = Guid.NewGuid(), // puede ser cualquier Guid de prueba
                                             // Si ListaFavorito tiene otros campos requeridos, inicializalos aquí
                };
                await listaRepo.InsertAsync(lista, autoSave: true);

                // 2) Crear el destino padre (DestinoTuristico)

                var destinoId = Guid.NewGuid();
                var destino = new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Test",
                    Pais = "Argentina",
                    Poblacion = 100000,
                    Latitud = -34.6f,
                    Longitud = -58.4f,
                    IdExterno = "test-geo-001"
                };

                await destinoRepo.InsertAsync(destino, autoSave: true);

                // 3) Crear las líneas apuntando a las entidades padre ya persistidas
                var linea1 = new LineaListaFavorito
                {
                    ListaFavoritoId = lista.Id,
                    DestinoTuristicoId = destino.Id,
                    IsDeleted = false
                    // Inicializá otros campos NOT NULL si existen
                };

                var linea2 = new LineaListaFavorito
                {
                    ListaFavoritoId = lista.Id,
                    DestinoTuristicoId = destino.Id,
                    IsDeleted = false
                };

                await lineaRepo.InsertAsync(linea1, autoSave: true);
                await lineaRepo.InsertAsync(linea2, autoSave: true);

                // Act: llamamos al servicio que calcula el resumen
                var resumen = await service.GetResumenAsync();

                // Assert: esperamos 2 favoritos guardados
                Assert.Equal(2, resumen.TotalDestinosGuardados);
            });
        }

        [Fact]
        public async Task GetResumenAsync_CuentaOpinionesCorrectamente()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var service = GetRequiredService<DashboardAppService>();
                var opinionRepo = GetRequiredService<IRepository<Opinion, Guid>>();
                var usuarioRepo = GetRequiredService<IRepository<IdentityUser, Guid>>();
                var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();

                // 1) Crear e insertar usuario (padre) usando la firma correcta del ctor
                var usuario = new IdentityUser(
                    Guid.NewGuid(),
                    "testuser",
                    "test@example.com",
                    null
                );
                await usuarioRepo.InsertAsync(usuario, autoSave: true);

                // 2) Crear e insertar destino (padre) — usa el ctor público correcto
                var destinoId = Guid.NewGuid();
                var destino = new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Test",
                    Pais = "Argentina",
                    Poblacion = 100000,
                    Latitud = -34.6f,
                    Longitud = -58.4f,
                    IdExterno = "test-geo-001"
                };
                await destinoRepo.InsertAsync(destino, autoSave: true);

                // 3) Crear e insertar opinion apuntando a usuario.Id y destino.Id
                // Ajustá la firma del constructor de Opinion si es distinta; si la entidad tiene
                // una propiedad DestinoTuristicoId, asígnala antes de InsertAsync.
                var opinion = new Opinion(
                    Guid.NewGuid(),
                    usuario.Id,
                    ValorPuntuacion.Cinco,
                    "Excelente"
                )
                {
                    // Asegurate de inicializar la FK al destino si existe en tu modelo
                    DestinoTuristicoId = destino.Id,
                    IsDeleted = false
                    // Inicializa aquí otras propiedades required si las tiene
                };

                await opinionRepo.InsertAsync(opinion, autoSave: true);

                // Act
                var resumen = await service.GetResumenAsync();

                // Assert
                Assert.Equal(1, resumen.TotalOpiniones);
            });
        }

        [Fact]
        public async Task GetResumenAsync_CuentaUsuariosCorrectamente()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Arrange: resolvemos dependencias reales dentro del mismo UoW
                var service = GetRequiredService<DashboardAppService>();
                var usuarioRepo = GetRequiredService<IRepository<IdentityUser, Guid>>();

                await usuarioRepo.DeleteAsync(u => true, autoSave: true);

                // Creamos un usuario usando el ctor correcto de IdentityUser
                var usuario = new IdentityUser(
                    Guid.NewGuid(),
                    "testuser",
                    "test@example.com",
                    null // tenantId si aplica
                );

                // Insertamos y guardamos inmediatamente
                await usuarioRepo.InsertAsync(usuario, autoSave: true);

                // Act: llamamos al servicio
                var resumen = await service.GetResumenAsync();

                // Assert: debe contar 1 usuario
                Assert.Equal(1, resumen.TotalUsuarios);
            });
        }

        [Fact]
        public async Task GetResumenAsync_CalculaPromedioApiCorrectamente()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Arrange: resolvemos dependencias reales
                var service = GetRequiredService<DashboardAppService>();
                var apiRepo = GetRequiredService<IRepository<RegistroApiExterna, Guid>>();

                // Insertamos dos registros de API con tiempos de respuesta distintos
                var registro1 = new RegistroApiExterna(
                    Guid.NewGuid(),
                    "GeoDB",
                    "/cities",
                    tiempoDuracionMs: 100,
                    codigoEstadoHttp: 200,
                    fueExitoso: true,
                    mensajeError: null
                );

                var registro2 = new RegistroApiExterna(
                    Guid.NewGuid(),
                    "GeoDB",
                    "/cities",
                    tiempoDuracionMs: 200,
                    codigoEstadoHttp: 200,
                    fueExitoso: true,
                    mensajeError: null
                );

                await apiRepo.InsertAsync(registro1, autoSave: true);
                await apiRepo.InsertAsync(registro2, autoSave: true);

                // Act
                var resumen = await service.GetResumenAsync();

                // Assert: el promedio debe ser (100 + 200) / 2 = 150
                Assert.Equal(150, resumen.TiempoPromedioRespuestaMs);
            });
        }






    }
}


