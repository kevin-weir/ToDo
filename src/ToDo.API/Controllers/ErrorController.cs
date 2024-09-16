using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ToDo.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        // Refer to: https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-3.0

        [Route("/error")]
        public IActionResult Error() => Problem();

        [Route("/error-local-development")]
        public IActionResult ErrorLocalDevelopment(
                [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName != "Development")
            {
                throw new InvalidOperationException(
                    "This shouldn't be invoked in non-development environments.");
            }

            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (context == null || context.Error == null)
            {
                return Problem();
            }
            else
            {
                return Problem(
                    detail: context.Error.StackTrace,
                    title: context.Error.Message);
            }
        }
    }
}
