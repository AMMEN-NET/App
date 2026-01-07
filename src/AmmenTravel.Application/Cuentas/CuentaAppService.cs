using System;
using System.Threading.Tasks;
using AmmenTravel.Opiniones;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace AmmenTravel.Cuentas
{
    [Authorize] // Solo usuarios logueados pueden entrar aquí
    public class CuentaAppService : ApplicationService, ICuentaAppService
    {
        private readonly ICurrentUser _usuarioActual;
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        // Inyectamos lo necesario
        public CuentaAppService(
            ICurrentUser usuarioActual,
            IdentityUserManager userManager,
            IRepository<Opinion, Guid> opinionRepository)
        {
            _usuarioActual = usuarioActual;
            _userManager = userManager;
            _opinionRepository = opinionRepository;
        }

        public async Task EliminarMiCuentaAsync()
        {
            var usuarioId = _usuarioActual.Id;
            if (usuarioId == null) return;

            // 1. OBTENER Y ELIMINAR OPINIONES (SOFT DELETE)
            // Buscamos todas las opiniones de este usuario
            var opinionesDelUsuario = await _opinionRepository.GetListAsync(x => x.UserId == usuarioId.Value);

            foreach (var opinion in opinionesDelUsuario)
            {
                // Al hacer DeleteAsync, ABP pone IsDeleted = true automáticamente
                await _opinionRepository.DeleteAsync(opinion);
            }

            // 2. ELIMINAR EL USUARIO (SOFT DELETE)
            // IdentityUser de ABP también soporta Soft Delete
            var usuario = await _userManager.GetByIdAsync(usuarioId.Value);

            // Esto marca al usuario como borrado y lo invalida para el login

            // ATENCION: SUPONGO QUE CUANDO HAGAMOS LAS EXPERIENCIAS DE USUARIO, TAMBIÉN HABRÁ QUE ELIMINARLAS EXPERIENCIAS ASOCIADAS A ESTE USUARIO!
            await _userManager.DeleteAsync(usuario);
        }
    }
}