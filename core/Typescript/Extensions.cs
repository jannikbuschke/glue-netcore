using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Glow.Core.Typescript;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Glow.TypeScript
{
    public class AssembliesToScan
    {
        public Assembly[] Value { get; set; }
    }

    public class Options
    {
        public string Path { get; set; }
        public bool GenerateApi { get; set; }

        public string GetPath()
        {
            return Path ?? "web/src/";
        }
    }


    public static class Extensions
    {
        public static string CamelCase(this string value)
        {
            return (char.ToLowerInvariant(value[0]) + value.Substring(1))
                .Replace("_", string.Empty);
        }

        public static bool IsEnumerable(this Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>) || t.GetInterfaces().Any(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static void AddTypescriptGeneration(
            this IServiceCollection services,
            Assembly[] assembliesToScan,
            Action<Options> configureOptions = null
        )
        {
            var options = new Options();
            configureOptions?.Invoke(options);
            services.AddSingleton(options);

            //services.AddSingleton(new AssembliesToScan() { Value = assembliesToScan });
            //services.AddHostedService<GenerateApiClientsAtStartup>();
            //services.AddHostedService<GenerateTsModelsAtStartupV2>();
            services.AddHostedService(provider => new GenerateTsModelsAtStartupV2(
                provider.GetService<IWebHostEnvironment>(),
                assembliesToScan,
                options
            ));
        }
    }
}
