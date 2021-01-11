using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glow.TypeScript;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Services.Common;

namespace Glow.Core.Typescript
{
    public class GenerateTsModelsAtStartupV2 : IHostedService
    {
        private readonly IWebHostEnvironment environment;
        private readonly TsGenerationOptions[] options;

        public GenerateTsModelsAtStartupV2(
            IWebHostEnvironment environment,
            TsGenerationOptions[] options
        )
        {
            this.environment = environment;
            this.options = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (TsGenerationOptions option in options)
            {
                await GenerateTypes(option);
            }
        }

        private Task GenerateTypes(TsGenerationOptions option)
        {
            if (!environment.IsDevelopment())
            {
                Console.WriteLine("Skip TS model generation");
                return Task.CompletedTask;
            }
            Console.WriteLine("Generate TS models");
            IEnumerable<Type> profileTypes = option.Assemblies
                .SelectMany(v => v.GetTypes())
                .Where(v => v.IsSubclassOf(typeof(TypeScriptProfile)));

            var builder = new TypeCollectionBuilder();
            // duplication?
            profileTypes.Select(v => Activator.CreateInstance(v) as TypeScriptProfile)
                .ForEach(type =>
                {
                    builder.AddRange(type.Types);
                });

            IEnumerable<Type> additionalTypes = option.Assemblies
                .SelectMany(v => v.GetExportedTypes()
                    .Where(x => x.GetCustomAttributes(typeof(GenerateTsInterface), true).Any()));

            builder.AddRange(additionalTypes);

            if (builder.Count == 0)
            {
                Console.WriteLine("Skipping Ts generation, as there are not types");
            }
            else
            {
                //builder.Insert(0, "/* eslint-disable prettier/prettier */");

                var configuredPath = option.GetPath();
                if(!configuredPath.EndsWith(".ts") && !configuredPath.EndsWith("/"))
                {
                    throw new Exception($"configured path '{configuredPath}' must end with / or .ts");
                }
                var path = configuredPath.EndsWith(".ts")
                    ? configuredPath
                    : $"{option.GetPath()}ts-models.ts";
                Console.WriteLine(path);

                Render.ToDisk(builder.Generate(), path);
            }

            //File.WriteAllText(path, text);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
