using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glow.Core.EfCore;
using Glow.Sample.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Glow.Sample
{
    public class DataContext : DbContext
    {
        private readonly OnChangeHandler onChangeHandler;

        public DataContext(DbContextOptions<DataContext> options, OnChangeHandler onChangeHandler) : base(options)
        {
            this.onChangeHandler = onChangeHandler;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            List<EntityChanged> changes = ChangeTracker.CollectChanges();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            onChangeHandler.HandleChanges(changes);
            return result;
        }

        public DbSet<PortfolioFile> PortfolioFiles { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
    }
}
