using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AmmenTravel.Notificaciones
{
    [Authorize]
    public class NotificacionAppService : ApplicationService
    {
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;

        public NotificacionAppService(IRepository<Notificacion, Guid> notificacionRepository)
        {
            _notificacionRepository = notificacionRepository;
        }

        // Obtener las últimas 10 notificaciones del usuario actual
        public async Task<List<NotificacionDto>> GetMisNotificacionesAsync()
        {
            var userId = CurrentUser.Id ?? throw new UserFriendlyException("Usuario no autenticado.");

            var query = await _notificacionRepository.GetQueryableAsync();

            var items = await query
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreationTime)
                .Take(10)
                .ToListAsync();

            return items
                .Select(x => new NotificacionDto
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Mensaje = x.Mensaje,
                    Leida = x.Leida,
                    Icono = x.Icono ?? string.Empty,
                    LinkReferencia = x.LinkReferencia ?? string.Empty,
                    Fecha = x.CreationTime,
                    Color = ObtenerColorPorTipo(x.Tipo)
                })
                .ToList();
        }

        // Contar no leídas (para el numerito rojo en la campana)
        public async Task<int> GetCantidadNoLeidasAsync()
        {
            var userId = CurrentUser.Id ?? throw new UserFriendlyException("Usuario no autenticado.");
            return await _notificacionRepository.CountAsync(x => x.UserId == userId && !x.Leida);
        }

        // Marcar como leída
        public async Task MarcarComoLeidaAsync(Guid id)
        {
            var userId = CurrentUser.Id ?? throw new UserFriendlyException("Usuario no autenticado.");

            var notificacion = await _notificacionRepository.GetAsync(id);
            if (notificacion.UserId == userId)
            {
                notificacion.Leida = true;
                await _notificacionRepository.UpdateAsync(notificacion);
            }
            else
            {
                throw new UserFriendlyException("No tienes permiso para modificar esa notificación.");
            }
        }

        // Marcar todas: implementación robusta compatible con todas las versiones de IRepository
        public async Task MarcarTodasComoLeidasAsync()
        {
            var userId = CurrentUser.Id ?? throw new UserFriendlyException("Usuario no autenticado.");

            var list = await _notificacionRepository.GetListAsync(x => x.UserId == userId && !x.Leida);
            if (!list.Any()) return;

            // Si UpdateManyAsync no existe o no guarda automáticamente, actualizamos cada entidad.
            foreach (var n in list)
            {
                n.Leida = true;
                // Usamos UpdateAsync por entidad con autoSave: true para asegurar persistencia.
                await _notificacionRepository.UpdateAsync(n, autoSave: true);
            }
        }

        private string ObtenerColorPorTipo(TipoNotificacion tipo)
        {
            return tipo switch
            {
                TipoNotificacion.Exito => "text-green-500",
                TipoNotificacion.Alerta => "text-red-500",
                TipoNotificacion.Social => "text-blue-500",
                TipoNotificacion.Recordatorio => "text-yellow-500",
                _ => "text-gray-500"
            };
        }
    }

    public class NotificacionDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public bool Leida { get; set; }
        public string? Icono { get; set; }          // Ahora nullable
        public string? LinkReferencia { get; set; } // Ahora nullable
        public DateTime Fecha { get; set; }
        public string Color { get; set; } // Clase CSS para el color
    }
}