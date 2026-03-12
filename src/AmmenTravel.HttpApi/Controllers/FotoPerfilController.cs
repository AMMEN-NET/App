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
            var contentType = DetectarMimeType(bytes);
            return File(bytes, contentType);
        }

        [HttpGet]
        [Route("mi-foto")]
        public async Task<IActionResult> ObtenerMiFotoAsync()
        {
            if (!_usuarioActual.IsAuthenticated) return Unauthorized();
            return await ObtenerAsync(_usuarioActual.Id.Value);
        }

        private static string DetectarMimeType(byte[] bytes)
        {
            if (bytes.Length >= 4)
            {
                // PNG: 89 50 4E 47
                if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                    return "image/png";

                // GIF: 47 49 46 38
                if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
                    return "image/gif";

                // WebP: 52 49 46 46 ... 57 45 42 50
                if (bytes.Length >= 12 &&
                    bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                    bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                    return "image/webp";
            }

            // JPEG: FF D8 FF
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";

            return "image/jpeg"; // fallback
        }
    }
}