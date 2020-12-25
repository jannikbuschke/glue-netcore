using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly Assembly[] assembliesToScan;
        private readonly Options options;

        public GenerateTsModelsAtStartupV2(
            IWebHostEnvironment environment,
            Assembly[] assembliesToScan,
            Options options
        )
        {
            this.environment = environment;
            this.assembliesToScan = assembliesToScan;
            this.options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!environment.IsDevelopment())
            {
                Console.WriteLine("Skip TS model generation");
                return Task.CompletedTask;
            }
            Console.WriteLine("Generate TS models");
            IEnumerable<Type> profileTypes = assembliesToScan
                .SelectMany(v => v.GetTypes())
                .Where(v => v.IsSubclassOf(typeof(TypeScriptProfile)));

            var builder = new TypeCollectionBuilder();
            // duplication?
            profileTypes.Select(v => Activator.CreateInstance(v) as TypeScriptProfile)
                .ForEach(type =>
                {
                    builder.AddRange(type.Types);
                });

            IEnumerable<Type> additionalTypes = assembliesToScan
                .SelectMany(v => v.GetExportedTypes()
                .Where(x => x.GetCustomAttributes(typeof(GenerateTsInterface), true).Any()));

            builder.AddRange(additionalTypes);
  
            //builder.Insert(0, "/* eslint-disable prettier/prettier */");
            var path = $"{options.GetPath()}ts-models.ts";

            Render.ToDisk(builder.Generate(), path);

            //File.WriteAllText(path, text);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
