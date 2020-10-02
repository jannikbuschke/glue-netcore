using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Glow.Configurations;
using Glow.Core;
using Glow.Core.EfCore;
using Glow.Sample.Configurations;
using Glow.Sample.Users;
using Glow.TypeScript;
using JannikB.Glue.AspNetCore.Tests;
using MediatR;
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

namespace Glow.Sample
{
    public class Test : TypeScriptProfile
    {
        public Test()
        {
            Add<Foo>();
            Add<Bar>();
        }

        public class Foo
        {
            public string Property0 { get; set; }
            public int Property1 { get; set; }
            public int? Property2 { get; set; }
            public Guid Property3 { get; set; }
            public Guid? Property4 { get; set; }
            public DateTime? Property5 { get; set; }
            public DateTime Property6 { get; set; }
            //public Dictionary<string,int> Property7 { get; set; }
            public Bar Property8 { get; set; }
            public string[] Property9 { get; set; }
            public IEnumerable<int> Property10 { get; set; }
            public IEnumerable<string> Property11 { get; set; }
            public List<Bar> Property12 { get; set; }
            public Collection<Bar> Property13 { get; set; }
            public Dictionary<string, string> MyProperty14 { get; set; }
            public Dictionary<string, object> MyProperty15 { get; set; }
            public Dictionary<string, long> MyProperty16 { get; set; }
            public Dictionary<string, Bar> MyProperty17 { get; set; }
        }

        public class Bar
        {
            public int Property0 { get; set; }
        }
    }

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
            services.AddAuthorization(options =>
            {
                options.AddPolicy("test-policy", v =>
                {
                    v.RequireAuthenticatedUser();
                });
            });
            services.AddGlow();

            UserDto testUser = TestUsers.TestUser();
            services.AddTestAuthentication(testUser.Id, testUser.DisplayName, testUser.Email);

            services.Configure<SampleConfiguration>(configuration.GetSection("sample-configuration"));

            services.AddEfConfiguration(options =>
            {
                options.SetPartialReadPolicy("sample-configuration", "test-policy");
                options.SetPartialWritePolicy("sample-configuration", "test-policy");
            }, new[] { typeof(Startup).Assembly });

            services.AddMediatR(typeof(Startup), typeof(Clocks.Clock));
            services.AddAutoMapper(cfg => { cfg.AddCollectionMappers(); }, typeof(Startup));

            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                //options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=glow-sample;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                options.EnableSensitiveDataLogging(true);
            });

            services.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            services.AddOptions();

            services.AddSignalR();

            services.AddTypescriptGeneration(new[] { Assembly.GetExecutingAssembly() });
            services.AddTransient<IStartupFilter, GenerateApiClientsAtStartup>();
            services.AddTransient<IStartupFilter, CreateTypescriptDefinitions>();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.SetTimeZoneInfo(TimeZoneInfo.Utc);
            });

            app.Map("/hello", app =>
            {
                app.Run(async (context) =>
                {
                    await context.Response.WriteAsync("hello world");
                });
            });

            app.UseEndpoints(v =>
            {
                v.MapHub<DbNotificationHub>("/notifications/db-changes");
            });

            new string[] { "/odata", "/api", "/notifications" }.ForEach(v =>
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
