using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ToDo.API.ApplicationBuilders;
using ToDo.API.Services;

namespace ToDo.API
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add SQL Server database service
            services.AddDatabaseService(Configuration);

            // Add distributed memory caching service
            services.AddDistributedMemoryCacheService(Configuration);

            // Add NSwag documentation service
            services.AddNSwagService(Configuration);

            // Add API health checks service
            services.AddHealthCheckService(Configuration);

            // Add MVC services
            services.AddMvcServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/error-local-development");

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseNSwagBuilder();
            }
            else
            {
                app.UseExceptionHandler("/error");
                
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Use builder to add Serilog request logging
            app.UseSerilogBuilder();

            app.UseRouting();

            // Configure and use HealthCheck options
            app.UseHealthCheckBuilder(Configuration);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
