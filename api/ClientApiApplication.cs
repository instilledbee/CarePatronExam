using api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace api;

/// <summary>
/// Exposes the minimal API as a class for use with unit tests
/// </summary>
class ClientApiApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ensure we are using a data context only for testing
            services.RemoveAll<DataSeeder>();
            services.RemoveAll<DbContextOptions<DataContext>>();
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("Testing"));
        });

        return base.CreateHost(builder);
    }
}