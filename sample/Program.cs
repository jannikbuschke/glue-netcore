using System;
using System.IO;
using EfConfigurationProvider.Core;
using Glow.GlowStartup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            return GlowStartup.Program.CreateDefaultWebHostBuilder<Startup>(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build())
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    IConfigurationRoot cfg = config.Build();

                    var cs = cfg.GetValue<string>("ConnectionString");
                    if (cs != null)
                    {
                        config.AddEFConfiguration(options => options.UseSqlServer(cs, configure =>
                        {
                            configure.MigrationsAssembly(typeof(EntityFrameworkExtensions).Assembly.FullName);
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
