using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Glow.Core.Linq;
using Glow.Glue.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glow.Configurations
{
    [Route("api/glow-configuration")]
    [ApiController]
    [Authorize]
    public class ConfigurationController
    {
        private readonly IConfigurationDataContext ctx;
        private readonly IAuthorizationService authorizationService;
        private readonly ConfigurationOptions options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ConfigurationController(
            IConfigurationDataContext ctx,
            IAuthorizationService authorizationService,
            ConfigurationOptions options,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.ctx = ctx;
            this.authorizationService = authorizationService;
            this.options = options;
            this.httpContextAccessor = httpContextAccessor;
        }

        private async Task TestPermission()
        {
            AuthorizationResult result = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User, options.ReadAllPolicy);
            if (!result.Succeeded)
            {
                throw new ForbiddenException("Forbidden");
            }
        }

        [HttpGet("all")]
        public async Task<IEnumerable<ConfigurationVersion>> List()
        {
            await TestPermission();
            return ctx.GlowConfigurations.OrderByDescending(v => v.Created);
        }

        [HttpGet("list")]
        public async Task<IEnumerable<ConfigurationVersion>> LatestList()
        {
            await TestPermission();
            return ctx.GlowConfigurations.OrderByDescending(v => v.Created).DistinctBy(v => v.Id + "." + v.Name);
        }

        [HttpGet("single/{id}")]
        public async Task<ConfigurationVersion> Single(string id, int? version, string name)
        {
            await TestPermission();
            var n = name ?? "";
            ConfigurationVersion r = await ctx.GlowConfigurations.SingleOrDefaultAsync(v => v.Id == id && v.Version == version && v.Name == n);
            return r;
        }
    }
}
