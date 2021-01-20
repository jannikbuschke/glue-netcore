using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Glow.Configurations
{
    public class ConfigurationUpdate : IRequest
    {
        public string ConfigurationId { get; set; }
        public string Name { get; set; } = Options.DefaultName;
        public Dictionary<string, object> Values { get; set; }
    }

    public class ConfigurationUpdateHandler : IRequestHandler<ConfigurationUpdate>
    {
        private readonly Configurations partialConfigurations;

        public ConfigurationUpdateHandler(Configurations partialConfigurations)
        {
            this.partialConfigurations = partialConfigurations;
        }

        public async Task<Unit> Handle(ConfigurationUpdate request, CancellationToken cancellationToken)
        {
            Meta partialConfiguration = partialConfigurations.Get().Single(v => v.Id == request.ConfigurationId);

            var builder = new DbContextOptionsBuilder<ConfigurationDataContext>();
            StartupExtensions.optionsAction(builder);
            using var ctx = new ConfigurationDataContext(builder.Options);

            ConfigurationVersion current = await ctx
                .GlowConfigurations
                .Where(v => v.Id == request.ConfigurationId && v.Name == request.Name)
                .OrderByDescending(v => v.Version)
                .FirstOrDefaultAsync() ?? new ConfigurationVersion
                {
                    Version = 0,
                    Values = new Dictionary<string, object>()
                };

            var nextValues = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> value in request.Values)
            {
                var key = $"{partialConfiguration.SectionId}{(request.Name == Options.DefaultName ? "" : $":{request.Name}")}:{value.Key}";
                nextValues[key] = value.Value;
            }

            ctx.GlowConfigurations.Add(new ConfigurationVersion
            {
                Id = request.ConfigurationId,
                Name = request.Name,
                Version = current.Version + 1,
                Values = nextValues,
                Created = DateTime.UtcNow,
                Message = "",
                User = null
            });

            await ctx.SaveChangesAsync();
            EfConfigurationProvider.SetReloadNecessary();
            return Unit.Value;
        }
    }
}
