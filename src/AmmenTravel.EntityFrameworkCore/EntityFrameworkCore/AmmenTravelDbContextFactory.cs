using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AmmenTravel.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AmmenTravelDbContextFactory : IDesignTimeDbContextFactory<AmmenTravelDbContext>
{
    public AmmenTravelDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        AmmenTravelEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<AmmenTravelDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new AmmenTravelDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AmmenTravel.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
