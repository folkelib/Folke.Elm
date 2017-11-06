using System;
using Folke.Elm.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Folke.Elm
{
    public static class ElmServiceCollectionExtensions
    {
        public static IServiceCollection AddElm<TDatabaseDriver>(this IServiceCollection services, Action<ElmOptions> options = null)
            where TDatabaseDriver: class, IDatabaseDriver
        {
            if (options != null)
            {
                services.Configure(options);
            }

            services.AddSingleton<IDatabaseDriver, TDatabaseDriver>();
            services.AddSingleton<IMapper, Mapper>();
            return services.AddScoped<IFolkeConnection, FolkeConnection>();
        }
    }
}
