using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using ToDo.Models;

namespace ToDo.API.Services
{
    public static class MvcServices
    {
        public static void AddMvcServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddFluentValidation(fv => {
                    fv.RegisterValidatorsFromAssemblyContaining<TodoContext>();
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
                    fv.ImplicitlyValidateChildProperties = true;
                });
        }
    }
}
