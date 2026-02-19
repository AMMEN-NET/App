using AmmenTravel.Destinos;
using AmmenTravel.Experiencias;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.ExperienciaTest
{
    public abstract class classExperienciaTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IExperienciaAppService _service;
        private readonly IRepository<Experiencia, Guid> _experienciaRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IIdentityUserRepository _userRepository;

        protected classExperienciaTestAppServiceTest()
        {
            _service = GetRequiredService<IExperienciaAppService>();
            _experienciaRepository = GetRequiredService<IRepository<Experiencia, Guid>>();
            _destinoRepository = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            _userRepository = GetRequiredService<IIdentityUserRepository>();
        }

        [Fact]
        public async Task CreateAsync_CreaYPersisteExperiencia()
        {
            // Arrange: crear destino y usuario en BD (el servicio espera que el usuario exista)
            var destino = new DestinoTuristico(Guid.NewGuid())
            {
                Nombre = "DestinoTest",
                Pais = "PaisTest",
                Poblacion = 1000,
                Latitud = 1.0f,
                Longitud = 1.0f
            };

            var currentUserId = CurrentUser.Id ?? throw new Exception("CurrentUser.Id debe existir en tests");

            await WithUnitOfWorkAsync(async () =>
            {
                // Insertamos destino
                await _destinoRepository.InsertAsync(destino, autoSave: true);

                // Aseguramos que el IdentityUser con el mismo Id esté en la BD para que _userRepository.GetAsync(...) funcione
                var identityUser = new IdentityUser(currentUserId, "testuser", "test@mail.com");
                await _userRepository.InsertAsync(identityUser, autoSave: true);
            });

            var input = new CreateUpdateExperienciaDto
            {
                DestinoId = destino.Id,
                Valoracion = TipoExperiencia.MuyBueno,
                Comentario = "Comentario de prueba suficientemente largo"
            };

            // Act
            var dto = await WithUnitOfWorkAsync(async () => await _service.CreateAsync(input));

            // Assert
            dto.ShouldNotBeNull();
            dto.DestinoId.ShouldBe(destino.Id);
            dto.Comentario.ShouldBe(input.Comentario);

            await WithUnitOfWorkAsync(async () =>
            {
                var saved = await _experienciaRepository.GetAsync(dto.Id);
                saved.ShouldNotBeNull();
                saved.Comentario.ShouldBe(input.Comentario);
            });
        }

        [Fact]
        public async Task UpdateAsync_PermiteAlPropietarioActualizar()
        {
            var userId = CurrentUser.Id.Value;

            var destino = new DestinoTuristico(Guid.NewGuid()) { Nombre = "D1", Pais = "P", Poblacion = 1, Latitud = 0, Longitud = 0 };
            var experiencia = new Experiencia(Guid.NewGuid(), destino.Id, TipoExperiencia.Malo, "Texto viejo");

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepository.InsertAsync(destino, autoSave: true);
                // Setear CreatorId antes de persistir
                typeof(Experiencia).GetProperty("CreatorId")!.SetValue(experiencia, userId);
                await _experienciaRepository.InsertAsync(experiencia, autoSave: true);
            });

            var input = new CreateUpdateExperienciaDto
            {
                DestinoId = destino.Id,
                Valoracion = TipoExperiencia.MuyBueno,
                Comentario = "Texto nuevo actualizado"
            };

            // Act
            var updatedDto = await WithUnitOfWorkAsync(async () => await _service.UpdateAsync(experiencia.Id, input));

            // Assert
            updatedDto.ShouldNotBeNull();
            updatedDto.Valoracion.ShouldBe(input.Valoracion);
            updatedDto.Comentario.ShouldBe(input.Comentario);

            await WithUnitOfWorkAsync(async () =>
            {
                var saved = await _experienciaRepository.GetAsync(experiencia.Id);
                saved.Valoracion.ShouldBe(input.Valoracion);
                saved.Comentario.ShouldBe(input.Comentario);
            });
        }

        [Fact]
        public async Task UpdateAsync_LanzaSiNoEsPropietario()
        {
            var destino = new DestinoTuristico(Guid.NewGuid()) { Nombre = "D2", Pais = "P", Poblacion = 1, Latitud = 0, Longitud = 0 };
            var experiencia = new Experiencia(Guid.NewGuid(), destino.Id, TipoExperiencia.Malo, "Old");
            var otherUser = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepository.InsertAsync(destino, autoSave: true);
                typeof(Experiencia).GetProperty("CreatorId")!.SetValue(experiencia, otherUser);
                await _experienciaRepository.InsertAsync(experiencia, autoSave: true);
            });

            var input = new CreateUpdateExperienciaDto
            {
                DestinoId = destino.Id,
                Valoracion = TipoExperiencia.MuyBueno,
                Comentario = "Intento cambio"
            };

            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await WithUnitOfWorkAsync(async () => await _service.UpdateAsync(experiencia.Id, input));
            });
        }

        [Fact]
        public async Task DeleteAsync_BorraComoSoftDelete_SoloPropietarioPuede()
        {
            var userId = CurrentUser.Id.Value;

            var destino = new DestinoTuristico(Guid.NewGuid()) { Nombre = "D3", Pais = "P", Poblacion = 1, Latitud = 0, Longitud = 0 };
            var experiencia = new Experiencia(Guid.NewGuid(), destino.Id, TipoExperiencia.Neutral, "Para borrar");

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepository.InsertAsync(destino, autoSave: true);
                typeof(Experiencia).GetProperty("CreatorId")!.SetValue(experiencia, userId);
                await _experienciaRepository.InsertAsync(experiencia, autoSave: true);
            });

            // Act
            await WithUnitOfWorkAsync(async () => await _service.DeleteAsync(experiencia.Id));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var queryable = await _experienciaRepository.GetQueryableAsync();
                var e = await queryable.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == experiencia.Id);
                e.ShouldNotBeNull();
                e.IsDeleted.ShouldBeTrue();
            });
        }


        [Fact]
        public async Task GetListPorUsuarioAsync_RetornaSoloLasDelUsuario()
        {
            var userId = CurrentUser.Id.Value;
            var otherUser = Guid.NewGuid();
            var destino = new DestinoTuristico(Guid.NewGuid()) { Nombre = "DLU", Pais = "P", Poblacion = 1, Latitud = 0, Longitud = 0 };

            var e1 = new Experiencia(Guid.NewGuid(), destino.Id, TipoExperiencia.MuyBueno, "Mía 1");
            var e2 = new Experiencia(Guid.NewGuid(), destino.Id, TipoExperiencia.Malo, "Otra");
            var e3 = new Experiencia(Guid.NewGuid(), destino.Id, TipoExperiencia.Neutral, "Mía 2");

            await WithUnitOfWorkAsync(async () =>
            {
                // Insertamos destino
                await _destinoRepository.InsertAsync(destino, autoSave: true);

                // Insertamos IdentityUser para CurrentUser.Id porque el servicio hace _userRepository.GetAsync(userId)
                var identityUser = new IdentityUser(userId, "testuser", "test@mail.com");
                await _userRepository.InsertAsync(identityUser, autoSave: true);

                // Seteamos los CreatorId y persistimos experiencias
                typeof(Experiencia).GetProperty("CreatorId")!.SetValue(e1, userId);
                typeof(Experiencia).GetProperty("CreatorId")!.SetValue(e2, otherUser);
                typeof(Experiencia).GetProperty("CreatorId")!.SetValue(e3, userId);
                await _experienciaRepository.InsertAsync(e1, autoSave: true);
                await _experienciaRepository.InsertAsync(e2, autoSave: true);
                await _experienciaRepository.InsertAsync(e3, autoSave: true);
            });

            var lista = await WithUnitOfWorkAsync(async () => await _service.GetListPorUsuarioAsync(userId));

            lista.ShouldNotBeNull();
            lista.Count.ShouldBe(2);
            lista.All(x => x.CreatorId == userId).ShouldBeTrue();
        }

    }
}