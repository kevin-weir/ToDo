using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag;
using ToDo.API.Helpers;

namespace ToDo.API.Services
{
    public static class NSwagService
    {
        public static void AddNSwagService(this IServiceCollection services, IConfiguration configuration)
        {
            var nswagSettings = configuration.GetSection(nameof(NSwagSettings)).Get<NSwagSettings>();

            services.AddOpenApiDocument(document =>
                {
                    document.PostProcess = doc =>
                    {
                        doc.Info.Title = nswagSettings.Title;
                        doc.Info.Description = nswagSettings.Description;
                        doc.Info.TermsOfService = nswagSettings.TermsOfService;

                        doc.Info.Contact = new OpenApiContact
                        {
                            Email = nswagSettings.Contact.Email,
                            Name = nswagSettings.Contact.Name
                        };

                        doc.Info.License = new OpenApiLicense
                        {
                            Url = nswagSettings.License.Url.ToString()
                        };
                    };

                    document.OperationProcessors.Add(new ReponseCleanupOperationProcessor());
                });
        }
    }

    public class ReponseCleanupOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var reponseCodes = new string[] { "200", "201", "204", "400" };

            // Find response codes that are not in response code list and clear their content
            var openApiReponses = from resp in context.OperationDescription.Operation.Responses
                                    where reponseCodes.Contains(resp.Key.ToString()) == false
                                    select resp.Value;

            foreach (var response in openApiReponses)
            {
                response.Content.Clear();
            }

            // return false to exclude the operation from the document
            return true;
        }
    }
}
