using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AmmenTravel.BackgroundWorkers
{
    /// <summary>
    /// Worker que procesa el resumen semanal de emails.
    /// Se ejecuta cada hora y verifica si es domingo entre 20:00-21:00 para enviar los resúmenes.
    /// </summary>
    public class WorkerResumenSemanalEmails : AsyncPeriodicBackgroundWorkerBase
    {
        public WorkerResumenSemanalEmails(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
            : base(timer, serviceScopeFactory)
        {
            // Cada 1 hora (3600000 ms)
            Timer.Period = 3600000;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<WorkerResumenSemanalEmails>>();
            
            // Verificamos si es el momento correcto: Domingo entre 20:00 y 21:00
            var ahora = DateTime.Now;
            
            if (ahora.DayOfWeek == DayOfWeek.Sunday && ahora.Hour == 20)
            {
                logger.LogInformation("¡Es domingo a las 20hs! Ejecutando envío de resumen semanal...");
                
                var resumenService = workerContext.ServiceProvider.GetRequiredService<ResumenSemanalEmailService>();
                await resumenService.ProcesarResumenSemanalAsync();
                
                logger.LogInformation("Resumen semanal completado.");
            }
            else
            {
                logger.LogDebug($"No es momento de resumen semanal. Actual: {ahora.DayOfWeek} {ahora.Hour}:00");
            }
        }
    }
}
