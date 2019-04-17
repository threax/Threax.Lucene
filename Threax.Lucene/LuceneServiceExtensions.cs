using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threax.Lucene;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LuceneServiceExtensions
    {
        /// <summary>
        /// Add a search service to the dependency injection. The TSearchService must match your
        /// search class's injected arguments or you will get type errors on initialization.
        /// The search service will be added to your app as a Singleton.
        /// </summary>
        public static IServiceCollection AddThreaxLucene<TSearchService>(this IServiceCollection services, Action<LuceneServiceOptions<TSearchService>> configure = null)
            where TSearchService : class
        {
            return AddThreaxLucene<TSearchService, TSearchService>(services, configure);
        }

        /// <summary>
        /// Add a search service to the dependency injection. The TSearchService must match your
        /// search class's injected arguments or you will get type errors on initialization.
        /// The search service will be added to your app as a Singleton.
        /// </summary>
        public static IServiceCollection AddThreaxLucene<TISearchService, TSearchService>(this IServiceCollection services, Action<LuceneServiceOptions<TISearchService>> configure = null)
            where TISearchService : class
            where TSearchService : class, TISearchService
        {
            var options = new LuceneServiceOptions<TISearchService>();
            configure?.Invoke(options);
            services.AddSingleton<LuceneServiceOptions<TISearchService>>(options);
            if (options.UseDirectoryIndex)
            {
                services.AddSingleton<ILuceneDirectoryProvider<TISearchService>, FileDirectoryProvider<TISearchService>>();
            }
            else
            {
                services.AddSingleton<ILuceneDirectoryProvider<TISearchService>, RamDirectoryProvider<TISearchService>>();
            }
            services.AddSingleton<TISearchService, TSearchService>();

            return services;
        }

        /// <summary>
        /// Add a search service to the dependency injection. This version does not register a
        /// service itself, you must register your own singleton. This version is provided for
        /// backward compatability or for using a single lucene index.
        /// </summary>
        public static IServiceCollection AddThreaxLucene(this IServiceCollection services, Action<LuceneServiceOptions> configure = null)
        {
            var options = new LuceneServiceOptions();
            configure?.Invoke(options);
            services.AddSingleton<LuceneServiceOptions>(options);
            if (options.UseDirectoryIndex)
            {
                services.AddSingleton<ILuceneDirectoryProvider, FileDirectoryProvider>();
            }
            else
            {
                services.AddSingleton<ILuceneDirectoryProvider, RamDirectoryProvider>();
            }

            return services;
        }
    }
}
