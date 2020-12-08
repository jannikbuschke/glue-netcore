using System;
using System.IO;
using Glow.Configurations;
using Glow.Core.EfCore;
using Glow.GlowStartup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace Glow.Sample
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = GetPreStartLogger();
            var name = typeof(Program).Namespace;
            Log.Information($"Starting {name}");

            try
            {
                Log.Information($"Build host {name}");
                IWebHost host = CreateWebHostBuilder(args).Build();

                host.MigrateDatabase<DataContext>();

                DataContext ctx = host.Services.GetRequiredService<DataContext>();
                ctx.Portfolios.Add(new Files.Portfolio { DisplayName = "JBU" });
                ctx.SaveChanges();

                host.Run();
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return 1;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return
                new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    IConfigurationRoot cfg = config.Build();
                    config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    var cs = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=glow-dev;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False" ?? cfg.GetValue<string>("ConnectionString");
                    if (cs != null)
                    {
                        config.AddEFConfiguration(options => options.UseSqlServer(cs, configure =>
                        {
                            configure.MigrationsAssembly(typeof(StartupExtensions).Assembly.FullName);
                        }));
                    }
                });
        }

        private static string EnvironmentName
        {
            get
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            }
        }

        private static Logger GetPreStartLogger()
        {
            return EnvironmentName == "Production"
                ? new LoggerConfiguration()
                    .WriteTo.RollingFile("log/log-{Date}.txt")
                    .CreateLogger()
                : new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
        }
    }
}
