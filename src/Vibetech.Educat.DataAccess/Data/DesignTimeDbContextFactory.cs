using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Vibetech.Educat.DataAccess.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EducatDbContext>
{
    public EducatDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<EducatDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        builder.UseNpgsql(connectionString);

        return new EducatDbContext(builder.Options);
    }
} 