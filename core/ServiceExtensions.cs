using Glow.Clocks;
using Glow.Core.EfCore;
using Glue.Files;
using Microsoft.Extensions.DependencyInjection;

namespace Glow.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddGlow(this IServiceCollection services)
        {
            services.AddSingleton<FileService>();
            services.AddSingleton<MockExternalSystems>();
            services.AddSingleton<IClock, Clock>();
            services.AddSingleton<OnChangeHandler>();
            return services;
        }
    }
}
