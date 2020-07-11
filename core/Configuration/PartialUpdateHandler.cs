using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EfConfigurationProvider.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EfConfigurationProvider
{

    public class PartialUpdateHandler : IRequestHandler<PartialUpdate>
    {
        private readonly PartialConfigurations partialConfigurations;

        public PartialUpdateHandler(PartialConfigurations partialConfigurations)
        {
            this.partialConfigurations = partialConfigurations;
        }

        public async Task<Unit> Handle(PartialUpdate request, CancellationToken cancellationToken)
        {
            var partialConfiguration = partialConfigurations.Get().Single(v => v.Id == request.ConfigurationId);

            var builder = new DbContextOptionsBuilder<ConfigurationDataContext>();
            EntityFrameworkExtensions.optionsAction(builder);
            using var ctx = new ConfigurationDataContext(builder.Options);

            Configuration current = await ctx.GlowConfigurations.OrderByDescending(v => v.Created).FirstOrDefaultAsync();

            Dictionary<string, string> values = current?.Values ?? new Dictionary<string, string>();
            var @new = new Dictionary<string, string>(values);
            foreach (ConfigurationValue value in request.Values)
            {
                @new[partialConfiguration.SectionId + ":" + value.Name] = value.Value;
            }

            ctx.GlowConfigurations.Add(new Configuration
            {
                Values = @new,
                Created = DateTime.UtcNow,
            });

            await ctx.SaveChangesAsync();
            ConfigurationProvider.Value.Reload();

            return Unit.Value;
        }
    }
}
