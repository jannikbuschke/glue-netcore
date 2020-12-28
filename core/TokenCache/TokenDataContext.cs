using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glow.TokenCache
{
    public interface ITokenDataContext
    {
        DbSet<UserToken> GlowTokenCache { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }

    public class TokenDataContext : DbContext
    {
        public TokenDataContext(DbContextOptions options) : base(options) { }

        public DbSet<UserToken> GlowTokenCache { get; set; }

        protected override void OnModelCreating(ModelBuilder model)
        {
            EntityTypeBuilder<UserToken> userToken = model.Entity<UserToken>();
            userToken.Property(v => v.UserId).IsRequired();
            userToken.Property(v => v.AccessToken).IsRequired();
        }
    }

    internal class SqlServerTokenDataContext : TokenDataContext, ITokenDataContext
    {
        public SqlServerTokenDataContext(DbContextOptions<SqlServerTokenDataContext> options) : base(options)
        {
        }
    }

    internal class SqlServerContextFactory : IDesignTimeDbContextFactory<SqlServerTokenDataContext>
    {
        public SqlServerTokenDataContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<SqlServerTokenDataContext>();
            options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=azdo-token-cache-dev;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

            return new SqlServerTokenDataContext(options.Options);
        }
    }
}
