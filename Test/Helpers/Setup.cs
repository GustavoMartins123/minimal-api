using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Dominio.Interfaces;
using Test.Mocks;
using MinimalAPI;

namespace Test.Helpers
{
    public class Setup
    {
        public const string PORT = "5001";
        public static TestContext textContext = default!;
        public static WebApplicationFactory<Startup> http = default!;
        public static HttpClient client = default!;

        public static void ClassInit(TestContext context)
        {
            textContext = context;
            http = new WebApplicationFactory<Startup>();

            http = http.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("https_port", PORT).UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IAdministradorServico, AdministradorServicoMock>();
                    services.AddScoped<IVeiculoServico, VeiculoServicoMock>();
                });
            });
            client = http.CreateClient();
        }

        public static void ClassCleanup()
        {
            http.Dispose();
        }
    }
}
