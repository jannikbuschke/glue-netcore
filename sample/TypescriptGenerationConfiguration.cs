using System;
using System.Collections.Generic;
using Glow.Configurations;
using Glow.Core.EfCore;
using Glow.Sample.Files;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Fluent;
using RT;

namespace Glow
{
    public static class InterfaceExportBuilderExtensions
    {
        public static InterfaceExportBuilder<T> WithDefaults<T>(this InterfaceExportBuilder<T> builder)
        {
            return builder.WithPublicProperties()
                .Substitute(typeof(string), new RtSimpleTypeName("string|null"))
                .Substitute(typeof(Guid), new RtSimpleTypeName("string"))
                .Substitute(typeof(Guid?), new RtSimpleTypeName("string|null"))
                .Substitute(typeof(DateTime?), new RtSimpleTypeName("string|null"))
                .Substitute(typeof(DateTime), new RtSimpleTypeName("string"))
                .Substitute(typeof(IEnumerable<string>), new RtSimpleTypeName("string[]"))
                .Substitute(typeof(string[]), new RtSimpleTypeName("string[]"))
                .AutoI(false);
        }
    }

    public class ReinforcedConfiguration
    {
        public static void Configure(ConfigurationBuilder builder)
        {
            builder.Global(options =>
            {
                options.CamelCaseForProperties(true);
                options.UseModules(true);
            });

            builder.ExportAsInterface<Portfolio>()
                .WithDefaults();

            builder.ExportAsInterface<PortfolioFile>()
                .WithDefaults();

            builder.ExportAsInterface<UpdatePortfolio>()
                .WithDefaults();

            builder.ExportAsInterface<CreatePortfolio>()
                .WithDefaults();

            builder.ExportAsInterface<DeletePortfolio>()
                .WithDefaults();

            builder.ExportAsInterface<Unit>()
                .WithDefaults();

            builder.ExportAsInterface<IConfigurationMeta>()
                .WithDefaults();

            builder.ExportAsInterface<EntityChanged>()
                .Substitute(typeof(Dictionary<string,object>),new RtSimpleTypeName("{ [key:string]: any }"))
                .WithDefaults();
        }
    }

    public class CreateTypescriptDefinitions : IStartupFilter
    {
        private readonly IWebHostEnvironment environment;

        public CreateTypescriptDefinitions(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (!environment.IsDevelopment())
            {
                return next;
            }
            Reinforced.Typings.TsExporter rt = ReinforcedTypings.Initialize(config =>
            {
                ReinforcedConfiguration.Configure(config);
            });
            rt.Export(); // <-- this will create the ts files

            return next;
        }
    }
}
