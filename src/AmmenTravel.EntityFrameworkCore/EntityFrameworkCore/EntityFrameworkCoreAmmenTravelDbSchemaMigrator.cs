using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AmmenTravel.Data;
using Volo.Abp.DependencyInjection;

namespace AmmenTravel.EntityFrameworkCore;

public class EntityFrameworkCoreAmmenTravelDbSchemaMigrator
    : IAmmenTravelDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAmmenTravelDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AmmenTravelDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AmmenTravelDbContext>()
            .Database
            .MigrateAsync();
    }
}
