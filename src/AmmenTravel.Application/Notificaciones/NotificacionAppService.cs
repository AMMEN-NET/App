using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

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
            var items = await _notificacionRepository.GetListAsync(x => x.UserId == CurrentUser.Id);

            return items.OrderByDescending(x => x.CreationTime)
                        .Take(10)
                        .Select(x => new NotificacionDto
                        {
                            Id = x.Id,
                            Titulo = x.Titulo,
                            Mensaje = x.Mensaje,
                            Leida = x.Leida,
                            Icono = x.Icono,
                            LinkReferencia = x.LinkReferencia,
                            Fecha = x.CreationTime,
                            Color = ObtenerColorPorTipo(x.Tipo)
                        }).ToList();
        }

        // Contar no leídas (para el numerito rojo en la campana)
        public async Task<int> GetCantidadNoLeidasAsync()
        {
            return await _notificacionRepository.CountAsync(x => x.UserId == CurrentUser.Id && !x.Leida);
        }

        // Marcar como leída
        public async Task MarcarComoLeidaAsync(Guid id)
        {
            var notificacion = await _notificacionRepository.GetAsync(id);
            if (notificacion.UserId == CurrentUser.Id)
            {
                notificacion.Leida = true;
                await _notificacionRepository.UpdateAsync(notificacion);
            }
        }

        // Marcar todas
        public async Task MarcarTodasComoLeidasAsync()
        {
            var list = await _notificacionRepository.GetListAsync(x => x.UserId == CurrentUser.Id && !x.Leida);
            foreach (var n in list)
            {
                n.Leida = true;
            }
            await _notificacionRepository.UpdateManyAsync(list);
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
        public string Icono { get; set; }
        public string LinkReferencia { get; set; }
        public DateTime Fecha { get; set; }
        public string Color { get; set; } // Clase CSS para el color
    }
}