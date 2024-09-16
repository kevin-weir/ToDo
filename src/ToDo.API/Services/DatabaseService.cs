// This service makes use of the following Nuget packages:
// Microsoft.EntityFrameworkCore.SqlServer
// Microsoft.EntityFrameworkCore.Tools

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ToDo.API.Helpers;
using ToDo.Models;

namespace ToDo.API.Services
{
    public static class DatabaseService
    {
        public static void AddDatabaseService(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStrings = configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();

            services.AddDbContextPool<TodoContext>(options =>
            {
                options.UseSqlServer(connectionStrings.ToDo,
                    assembly => assembly.MigrationsAssembly(typeof(TodoContext).Assembly.FullName));
            });
        }
    }
}
