using System.Reflection;
using Glow.Clocks;
using Glow.Core.Authentication;
using Glow.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Glow.Core
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Mvc (with Netwonsoft), FileService, Clock, MockExternalSystems, SpaStaticFiles,
        /// SignalR, HttpClient, HttpContextAccessor,
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddGlowApplicationServices(
            this IServiceCollection services,
            params Assembly[] assembliesToScan
        )
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = true;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            });

            services.AddGlow();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "web/build";
            });

            services.AddSignalR();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddGlowAadIntegration(
            this IServiceCollection services
        )
        {
            services.AddScoped<IGraphTokenService, GraphTokenService>();

            return services;
        }

        /// <summary>
        /// Adds FileService, MockExternalSystems and Clock
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddGlow(this IServiceCollection services)
        {
            services.AddSingleton<FileService>();
            services.AddSingleton<MockExternalSystems>();
            services.AddSingleton<IClock, Clock>();

            return services;
        }
    }
}
