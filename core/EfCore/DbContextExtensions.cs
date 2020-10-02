using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Glow.Core.EfCore
{
    public class OnChangeHandler
    {
        private readonly IMediator mediator;

        public OnChangeHandler(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public void HandleChanges(List<EntityChanged> changes)
        {
            mediator.Publish(new DbChangeNotification(changes));
        }
    }

    public class DbChangeNotification: INotification
    {
        public DbChangeNotification(List<EntityChanged> changes)
        {
            Changes = changes;
        }

        public List<EntityChanged> Changes { get; }
    }

    public class SignalrDbChangeNotificationHandler : INotificationHandler<DbChangeNotification>
    {
        private readonly IHubContext<DbNotificationHub> hubContext;

        public SignalrDbChangeNotificationHandler(IHubContext<DbNotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public Task Handle(DbChangeNotification notification, CancellationToken cancellationToken)
        {
            return hubContext.Clients.All.SendAsync("DbChanged", notification.Changes);
        }
    }

    public class DbNotificationHub : Hub
    {
  
    }

    //public class Key
    //{
    //    public string Name { get; set; }
    //    public object Value { get; set; }
    //}

    public class EntityChanged
    {
        public Dictionary<string, object> Key { get; set; }
        public EntityState State { get; set; }
        public string EntityName { get; set; }
    }

    public static class DbContextExtensions
    {
        public static List<EntityChanged> CollectChanges(this ChangeTracker changeTracker)
        {
            IEnumerable<EntityEntry> entries = changeTracker.Entries()
                .Where(v => v.State != EntityState.Detached && v.State != EntityState.Unchanged);

            var changes = entries.Select(entry => new EntityChanged
            {
                Key = entry.Metadata.FindPrimaryKey().Properties.Select(key => new
                {
                    key.Name,
                    Value = entry.Property(key.Name).CurrentValue
                }).ToDictionary(v => v.Name, v => v.Value),
                EntityName = entry.Entity.GetType().FullName,
                State = entry.State
            }).ToList();

            return changes;
        }

        public static void Migrate(this DbContext v, string migration = "")
        {
            if (string.IsNullOrEmpty(migration))
            {
                v.Database.Migrate();
            }
            else
            {
                IMigrator migrator = v.GetInfrastructure().GetService<IMigrator>();
                migrator.Migrate(migration);
            }
        }

        public static Task MigrateAsync(this DbContext v, string migration = "")
        {
            if (string.IsNullOrEmpty(migration))
            {
                return v.Database.MigrateAsync();
            }
            else
            {
                IMigrator migrator = v.GetInfrastructure().GetService<IMigrator>();
                return migrator.MigrateAsync(migration);
            }
        }
    }
}
