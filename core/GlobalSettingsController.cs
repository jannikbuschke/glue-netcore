using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Glow.Glue.AspNetCore
{
    // TODO, create a generic statically typed version
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class GlobalSettingsController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public GlobalSettingsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        [HttpGet("value")]
        public ActionResult<GlobalSettings> GetSettings()
        {
            var settings = new GlobalSettings();
            configuration.Bind("Global", settings);
            return settings;
        }

        [HttpGet("value-unauthenticated")]
        public ActionResult<GlobalSettings> GetSettingsUnauthenticated()
        {
            var settings = new GlobalSettings()
            {
                { "GloballyRequireAuthenticatedUser" , "true" }
            };
            return settings;
        }
    }
}
