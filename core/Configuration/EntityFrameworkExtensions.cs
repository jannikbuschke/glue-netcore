using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EfConfigurationProvider.Core
{
    public static class EntityFrameworkExtensions
    {
        internal static Action<DbContextOptionsBuilder> optionsAction;
        public static IConfigurationBuilder AddEFConfiguration(
            this IConfigurationBuilder builder,
            Action<DbContextOptionsBuilder> optionsAction)
        {
            EntityFrameworkExtensions.optionsAction = optionsAction;
            return builder.Add(new ConfigurationSource(optionsAction));
        }

        public static IServiceCollection AddEfConfiguration(
            this IServiceCollection services,
            Action<Options> configure,
            IEnumerable<Assembly> assemblies
        )
        {
            var configuration = new Options();
            configure(configuration);
            services.AddSingleton(configuration);
            services.AddSingleton<AuthorizationService>();
            services.AddHttpContextAccessor();
            services.AddSingleton((services) => new AssembliesCache(assemblies));
            //services.AddMvcCore().AddApplicationPart(typeof(EntityFrameworkExtensions).Assembly);

            services.AddSingleton<PartialConfigurations>();

            services.AddMvcCore(options =>
            {
                options.Conventions.Add(new GenericControllerRouteConvention());
            })
            .ConfigureApplicationPartManager(m =>
            {
                m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider(assemblies));
            });

            return services;
        }

        //public static IServiceCollection AddEfConfiguration(this IServiceCollection services)
        //{
        //    return AddEfConfiguration(services, options => { }, );
        //}
    }
}
