using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AmmenTravel.BackgroundWorkers
{
    public class NotificarEventosFavoritosWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public NotificarEventosFavoritosWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
            : base(timer, serviceScopeFactory)
        {
            // Dejalo en 10 segundos para probar:
            Timer.Period = 10000;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<NotificarEventosFavoritosWorker>>();
            logger.LogInformation("Ejecutando Worker de Ticketmaster...");

            // Pedimos nuestro nuevo servicio limpio y ejecutamos
            var notificador = workerContext.ServiceProvider.GetRequiredService<NotificadorEventosService>();
            await notificador.ProcesarEventosAsync();
        }
    }
}