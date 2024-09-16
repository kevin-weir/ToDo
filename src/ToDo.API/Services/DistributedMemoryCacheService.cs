// This service implements distrubuted memory cache using the following Nuget packages:
// Microsoft.Extensions.Caching.Distributed
// Microsoft.Extensions.Caching.StackExchangeRedis
// Microsoft.Extensions.Caching.SqlServer

// Refer To: 
// https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.0#distributed-memory-cache
// https://github.com/aspnet/AspNetCore.Docs/blob/master/aspnetcore/performance/caching/distributed/samples/3.x/DistCacheSample/Startup.cs


// SQL SERVER CACHE INSTALL

// Refer to:
// https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.0#distributed-memory-cache

// To install and configure SQL server cache run the following two commands from PowerShell or the Command Line:

// dotnet tool install --global dotnet-sql-cache
// dotnet sql-cache create "Persist Security Info=False;Integrated Security=true;Initial Catalog=ToDo;Server=localhost" dbo ToDoCache

// NOTE: For last command the first param is connection string followed by SQL server database schema and finally
// the table name to create


// REDIS CACHE INSTALL

// Refer to:
// https://chocolatey.org/install
// https://chocolatey.org/packages/redis-64/#install
// https://redis.io/

// Run the following commands from PowerShell to install:

// Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
// choco install redis-64
// redis-server

// NOTE: The default port the API connects to is localhost:6379

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDo.API.Helpers;

namespace ToDo.API.Services
{
    public static class DistributedMemoryCacheService
    {
        public static void AddDistributedMemoryCacheService(this IServiceCollection services, IConfiguration configuration)
        {
            var distributedMemoryCacheSettings = configuration.GetSection(nameof(DistributedMemoryCacheSettings)).Get<DistributedMemoryCacheSettings>();

            #region AddDistributedSqlServerCache
            if (distributedMemoryCacheSettings.DefaultProvider.Name == distributedMemoryCacheSettings.Providers.SqlServer.Name)
            {
                services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = configuration[distributedMemoryCacheSettings.Providers.SqlServer.SqlServerCacheSettings.ConnectionString];
                    options.SchemaName = distributedMemoryCacheSettings.Providers.SqlServer.SqlServerCacheSettings.SchemaName;
                    options.TableName = distributedMemoryCacheSettings.Providers.SqlServer.SqlServerCacheSettings.TableName;
                });
            }
            #endregion

            #region AddStackExchangeRedisCache
            if (distributedMemoryCacheSettings.DefaultProvider.Name == distributedMemoryCacheSettings.Providers.Redis.Name)
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = distributedMemoryCacheSettings.Providers.Redis.RedisCacheSettings.Configuration;
                    options.InstanceName = distributedMemoryCacheSettings.Providers.Redis.RedisCacheSettings.InstanceName;
                });
            }
            #endregion

            #region AddDistributedMemoryCache
            // If the default provider is Local or its not a valid provider then default to the Local provider
            if (distributedMemoryCacheSettings.DefaultProvider.Name == distributedMemoryCacheSettings.Providers.Local.Name ||
                (distributedMemoryCacheSettings.DefaultProvider.Name != distributedMemoryCacheSettings.Providers.Local.Name & 
                 distributedMemoryCacheSettings.DefaultProvider.Name != distributedMemoryCacheSettings.Providers.SqlServer.Name & 
                 distributedMemoryCacheSettings.DefaultProvider.Name != distributedMemoryCacheSettings.Providers.Redis.Name))
            {
                services.AddDistributedMemoryCache();
            }
            #endregion
        }
    }
}
