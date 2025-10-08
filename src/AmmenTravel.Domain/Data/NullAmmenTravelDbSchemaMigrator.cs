using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AmmenTravel.Data;

/* This is used if database provider does't define
 * IAmmenTravelDbSchemaMigrator implementation.
 */
public class NullAmmenTravelDbSchemaMigrator : IAmmenTravelDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
