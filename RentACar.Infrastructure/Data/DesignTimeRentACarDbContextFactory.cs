using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // <-- Make sure this is present
using System.IO;

namespace RentACar.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RentACarDbContext>
    {
        public RentACarDbContext CreateDbContext(string[] args)
        {
            // ... (environment setup code as before) ...

            IConfigurationRoot configuration = new ConfigurationBuilder()
                // Now SetBasePath should be recognized
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../RentACar.Web"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<RentACarDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find a connection string named 'DefaultConnection'. Check SetBasePath and appsettings.json existence.");
            }

            builder.UseSqlServer(connectionString); // Or your specific provider

            return new RentACarDbContext(builder.Options);
        }
    }
}