using System.Net;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using EfConfigurationProvider.Core;
using Glow.Core;
using MediatR;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Services.Common;
using RT;

namespace Glow.Sample
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration config)
        {
            configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });
            services.AddGlow();
            services.AddEfConfiguration(options => { }, new[] { typeof(Startup).Assembly });

            services.AddTransient<IStartupFilter, CreateTypescriptDefinitions>();

            services.AddMediatR(typeof(Startup), typeof(Clocks.Clock));
            services.AddAutoMapper(cfg => { cfg.AddCollectionMappers(); }, typeof(Startup));

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=glow-sample;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                options.EnableSensitiveDataLogging(true);
            });

            services.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            services.AddOData().EnableApiVersioning();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            VersionedODataModelBuilder modelBuilder
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.SetTimeZoneInfo(System.TimeZoneInfo.Utc);
                routes.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                routes.MapVersionedODataRoutes("odata", "odata", modelBuilder.GetEdmModels());
                routes.EnableDependencyInjection();
            });

            app.Map("/hello", app =>
            {
                app.Run(async (context) =>
                {
                    await context.Response.WriteAsync("hello world");
                });
            });

            new string[] { "/odata", "/api" }.ForEach(v =>
            {
                app.Map(v, app =>
                {
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = (int) HttpStatusCode.NotFound;
                        await ctx.Response.WriteAsync("Not found");
                    });
                });
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "web";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                }
            });
        }
    }
}
