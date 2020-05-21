using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Glow.Core.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EfConfigurationProvider.Core
{
    public class Configuration
    {
        public int Id { get; set; }
        public Dictionary<string, string> Values { get; set; }
        public DateTime Created { get; set; }
        public string User { get; set; }
    }

    public class ConfigurationDataContext : DbContext
    {
        public ConfigurationDataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Configuration> GlowConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Configuration>().Property(v => v.Values)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
        }
    }

    public interface IConfigurationDataContext
    {
        DbSet<Configuration> GlowConfigurations { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }

    internal class SqlServerConfigurationDataContext : ConfigurationDataContext, IConfigurationDataContext
    {
        public SqlServerConfigurationDataContext(DbContextOptions<SqlServerConfigurationDataContext> options) : base(options)
        {
        }
    }

    internal class SqlServerContextFactory : IDesignTimeDbContextFactory<SqlServerConfigurationDataContext>
    {
        public SqlServerConfigurationDataContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<SqlServerConfigurationDataContext>();
            options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=glow-configuration-design-time-dev;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

            return new SqlServerConfigurationDataContext(options.Options);
        }
    }

    public static class IServiceScopeMigrateExtensions
    {
        public static void GlowMigrateConfigurationSqlServer(this IServiceProvider serviceProvider)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            SqlServerConfigurationDataContext db = scope.ServiceProvider.GetRequiredService<SqlServerConfigurationDataContext>();
            db.Migrate();
        }
    }
}
