using Microsoft.AspNetCore.Builder;

namespace ToDo.API.ApplicationBuilders
{
    public static class NSwagBuilder
    {
        public static void UseNSwagBuilder(this IApplicationBuilder app)
        {
            // Serve OpenAPI/Swagger documents
            app.UseOpenApi();  

            app.UseSwaggerUi3(settings =>
            {
                settings.DocExpansion = "list";
            });
        }
    }
}
