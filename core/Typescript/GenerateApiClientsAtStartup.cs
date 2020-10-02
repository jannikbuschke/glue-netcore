using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glow.Core.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Glow.TypeScript
{
    public class GenerateApiClientsAtStartup : IStartupFilter
    {
        private readonly IApiDescriptionGroupCollectionProvider descriptionGroupCollectionProvider;
        private readonly IApiDescriptionProvider descriptionProvider;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<GenerateApiClientsAtStartup> logger;

        public GenerateApiClientsAtStartup(
            IApiDescriptionGroupCollectionProvider descriptionGroupCollectionProvider,
            IApiDescriptionProvider descriptionProvider,
            IWebHostEnvironment environment,
            ILogger<GenerateApiClientsAtStartup> logger
        )
        {
            this.descriptionGroupCollectionProvider = descriptionGroupCollectionProvider;
            this.descriptionProvider = descriptionProvider;
            this.environment = environment;
            this.logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (!environment.IsDevelopment())
            {
                return next;
            }

            var items = descriptionGroupCollectionProvider.ApiDescriptionGroups.Items.First().Items.Take(2);

            var descriptions = descriptionGroupCollectionProvider.ApiDescriptionGroups.Items.Select(v => new
            {
                v.GroupName,
                Items = v.Items.Select(v => {
                    var name = v.ActionDescriptor.RouteValues["action"];
                    var controller = v.ActionDescriptor.RouteValues["controller"];
                    var id = $"{controller}_{name}";
                    return new RequestDescription
                    {
                        Id = id,
                        ActionName = name,
                        ControllerName = controller,
                        RelativePath = v.RelativePath,
                        HttpMethod = v.HttpMethod,
                        ParameterDescriptions = v.ParameterDescriptions.Select(v => new ParameterDescription
                        {
                            Name = v.Name,
                            Type = v.Type,
                            Source = v.Source
                        }),
                        //ActionDescriptor = new
                        //{
                        //    v.ActionDescriptor.DisplayName,
                        //    v.ActionDescriptor.RouteValues,
                        //}
                    }; }).ToList()
            });
            var customTypes = new HashSet<Type>();
            var builder = new StringBuilder();
            foreach (var v in descriptions)
            {
                foreach (RequestDescription a in v.Items.DistinctBy(v => v.Id))
                {
                    foreach (ParameterDescription item in a.ParameterDescriptions.Where(v => v.Name != "api-version"))
                    {
                        if (!item.Type.FullName.StartsWith("System"))
                        {
                            logger.LogInformation("type {type} ", item.Type.FullName);
                            customTypes.Add(item.Type);
                        }
                    }

                    builder.AppendLine($"export module {a.ControllerName} {{");
                    if (a.HttpMethod?.ToLower() == "post")
                    {
                        builder.Append(
$@"  export async function {a.ControllerName}{a.ActionName}({string.Join(",", a.ParameterDescriptions.Where(v => v.Name != "api-version").Select(v => $"{v.Name}: {ToTsType(v.Type)}"))}) {{
    const response = await fetch(`/{a.RelativePath}?api-version=1.0`, {{
      method: ""POST"",
      headers: {{ ""content-type"": ""application/json"" }},
      body: JSON.stringify(request),
    }})
    const data = await response.json()
    return data
  }}
");
                    }
                    else
                    {
                        builder.Append(
$@"  export async function {a.ControllerName}{a.ActionName}({string.Join(",", a.ParameterDescriptions.Where(v => v.Name != "api-version").Select(v => $"{v.Name}: {ToTsType(v.Type)}"))}) {{
    const response = await fetch(`/{a.RelativePath.Replace("{", "${")}?api-version=1.0`)
    const data = await response.json()
    return data
  }}
");
                    }
                    builder.AppendLine($"}}");
                }
            }

            builder.Insert(0, "\r\n");
            builder.Insert(0, @$"import {{ {string.Join(", ", customTypes.Select(v => ToTsType(v)))} }} from ""./models""");
            builder.Insert(0, "\r\n");
            builder.Insert(0, "/* eslint-disable prettier/prettier */");
            System.IO.File.WriteAllText("web/src/api.ts", builder.ToString());

            //Reinforced.Typings.TsExporter rt = ReinforcedTypings.Initialize(config =>
            //{
            //    JannikB.Invoices.TypescriptGenerationConfiguration.Configure(config, customTypes);
            //});
            //rt.Export(); // <-- this will create the ts files

            return next;
            //return builder => { next(builder); };
        }

        private string ToTsType(Type t)
        {
            var types = new Dictionary<Type, string>
            {
                { typeof(string), "string" },
                { typeof(int), "number" },
                { typeof(int?), "number | null" },
                { typeof(DateTime), "string" },
                { typeof(DateTime?), "string | null" },
                { typeof(Guid), "string" },
                { typeof(Guid?), "string | null" },
                { typeof(bool), "boolean" },
                { typeof(bool?), "boolean | null" },
            };

            if (types.TryGetValue(t, out var result))
            {
                return result;
            }

            return t.Name;
        }
    }
}

