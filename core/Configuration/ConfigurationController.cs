using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EfConfigurationProvider.Core
{

    [Route("api/__configuration")]
    [ApiController]
    [Authorize, AllowAnonymous]
    public class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationRoot configuration;
        private readonly ConfigurationProvider provider;
        private readonly IMediator mediator;
        private readonly AuthorizationService authorizationService;
        private readonly PartialConfigurations partialConfigurations;

        public ConfigurationController(
            IConfiguration configuration,
            IMediator mediator,
            AuthorizationService authorizationService,
            PartialConfigurations partialConfigurations
        )
        {
            this.configuration = configuration as ConfigurationRoot;
            provider = this.configuration.Providers.OfType<ConfigurationProvider>().FirstOrDefault();
            if (provider == null)
            {
                throw new System.Exception("ConfigurationProvider not found. Did you forget add GlowConfiguration?");
            }
            this.mediator = mediator;
            this.authorizationService = authorizationService;
            this.partialConfigurations = partialConfigurations;
        }

        [HttpGet("values")]
        public async Task<ActionResult<IDictionary<string, string>>> Get()
        {
            if (await authorizationService.ReadAllAllowed())
            {
                return Ok(provider.GetData());
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("current")]
        public async Task<ActionResult<Configuration>> GetCurrentConfiguration()
        {
            if (await authorizationService.ReadAllAllowed())
            {
                return Ok(provider.GetConfiguration());
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(Update request)
        {
            if (!await authorizationService.UpdateAllAllowed())
            {
                return Unauthorized();
            }

            try
            {
                await mediator.Send(request);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet("partial-configurations")]
        public IEnumerable<IPartialConfiguration> GetPartialConfigurations()
        {
            return partialConfigurations.Get();
        }
    }

    public class PartialConfigurations
    {
        private readonly AssembliesCache assemblies;

        public PartialConfigurations(AssembliesCache assemblies)
        {
            this.assemblies = assemblies;
        }

        public IEnumerable<PartialConfigurationDto> Get()
        {
            IEnumerable<PartialConfigurationDto> attributes = assemblies
                .SelectMany(v => v.GetExportedTypes().Where(v => v.GetCustomAttributes(typeof(GeneratedControllerAttribute), true).Any())
                    .SelectMany(v => v.GetCustomAttributes<GeneratedControllerAttribute>())
                ).Select(v => v.ToPartialConfiguration());
            return attributes;
        }
    }
}
