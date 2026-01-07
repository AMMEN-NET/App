using System;
using System.IO;
using System.Threading.Tasks;
using AmmenTravel.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;
using Volo.Abp.Users;

namespace AmmenTravel.Controllers
{
    [Authorize]
    [Route("api/app/foto-perfil")]
    public class FotoPerfilController : AbpController
    {
        private readonly IBlobContainer<ContenedorFotoPerfil> _contenedorBlob;
        private readonly ICurrentUser _usuarioActual;

        public FotoPerfilController(
            IBlobContainer<ContenedorFotoPerfil> contenedorBlob,
            ICurrentUser usuarioActual)
        {
            _contenedorBlob = contenedorBlob;
            _usuarioActual = usuarioActual;
        }

        [HttpPost]
        [Route("subir")]
        public async Task<IActionResult> SubirAsync(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("No se ha seleccionado ningún archivo.");
            }

            if (archivo.Length > 5 * 1024 * 1024) // Límite 5MB
            {
                return BadRequest("La imagen es demasiado pesada (Máx 5MB).");
            }

            var usuarioId = _usuarioActual.Id.Value;
            var nombreBlob = usuarioId.ToString();

            using (var memoryStream = new MemoryStream())
            {
                await archivo.CopyToAsync(memoryStream);

                // Guardamos la foto con el ID del usuario como nombre
                await _contenedorBlob.SaveAsync(nombreBlob, memoryStream.ToArray(), overrideExisting: true);
            }

            return Ok();
        }

        [HttpGet]
        [Route("obtener/{usuarioId}")]
        [AllowAnonymous] // Permitir ver fotos públicas si se desea
        public async Task<IActionResult> ObtenerAsync(Guid usuarioId)
        {
            var nombreBlob = usuarioId.ToString();

            var existe = await _contenedorBlob.ExistsAsync(nombreBlob);

            if (!existe)
            {
                return NotFound();
            }

            var bytes = await _contenedorBlob.GetAllBytesAsync(nombreBlob);
            return File(bytes, "image/jpeg");
        }

        [HttpGet]
        [Route("mi-foto")]
        public async Task<IActionResult> ObtenerMiFotoAsync()
        {
            if (!_usuarioActual.IsAuthenticated) return Unauthorized();
            return await ObtenerAsync(_usuarioActual.Id.Value);
        }
    }
}