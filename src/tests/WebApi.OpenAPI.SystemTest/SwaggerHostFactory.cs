using Workleap.Extensions.OpenAPI;

namespace WebApi.OpenAPI.SystemTest;

public static class SwaggerHostFactory
{
    public static IHost CreateHost()
    {
        return Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(UseSwaggerHostStartup)
            .UseEnvironment(Environments.Development)
            .Build();
    }

    private static void UseSwaggerHostStartup(IWebHostBuilder builder)
    {
        builder.UseStartup<SwaggerHostStartup>();
    }

    private sealed class SwaggerHostStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwagger();
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}