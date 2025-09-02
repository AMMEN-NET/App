using System.Threading.Tasks;

namespace AmmenTravel.Data;

public interface IAmmenTravelDbSchemaMigrator
{
    Task MigrateAsync();
}
