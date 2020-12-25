using System;
using Glow.Configurations;
using Glow.Core.EfCore;
using Glow.TokenCache;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Glow.AzdoAuthentication
{
    public static class StartupExtension
    {
        public static AuthenticationBuilder AddAzdo(
            this AuthenticationBuilder builder,
            Action<AzdoConfig> configure,
            DatabaseProvider dbProvider,
            string connectionString
        )
        {
            IServiceCollection services = builder.Services;

            services.AddSingleton<AzdoAuthenticationService>();
            services.AddSingleton<AzdoClients>();
            services.AddSingleton<ActiveUsersCache>();
            var config = new AzdoConfig();
            configure(config);
            services.AddSingleton(config);
            builder.AddCookie(AzdoDefaults.CookieAuthenticationScheme);

            switch (dbProvider)
            {
                case DatabaseProvider.SqlServer:
                    services.AddDbContext<SqlServerTokenDataContext>(options=>
                    {
                        options.UseSqlServer(connectionString, options =>
                        {
                            options.MigrationsAssembly(typeof(TokenDataContext).Assembly.FullName);
                        });
                    });
                    services.AddScoped<ITokenDataContext, SqlServerTokenDataContext>();

                    //services.AddDbContext<SqlServerConfigurationDataContext>(configureDb);
                    //services.AddScoped<IConfigurationDataContext, SqlServerConfigurationDataContext>();

                    break;
                default:
                    throw new Exception("Unsupported Database Provider");
            }

            return builder;
        }
    }
}
