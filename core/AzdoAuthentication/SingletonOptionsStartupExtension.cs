using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Glow.AzdoAuthentication
{
    public static class SingletonOptionsStartupExtension
    {
        public static void ConfigureSingleton<T>(this IServiceCollection services, string configurationSectionKey) where T : class, new()
        {
            ServiceProvider provider = services.BuildServiceProvider();
            services.Configure<T>(provider.GetRequiredService<IConfiguration>().GetSection(configurationSectionKey));
            services.AddSingleton(services =>
            {
                IOptions<T> options = services.GetRequiredService<IOptions<T>>();
                return options.Value;
            });
        }
    }
}
