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
