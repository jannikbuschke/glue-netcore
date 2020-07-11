using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EfConfigurationProvider.Core
{
    [Route("api/config")]
    [Authorize, AllowAnonymous]
    public class PartialUpdateController<T> : ControllerBase where T : class, new()
    {
        private readonly IMediator mediator;
        private readonly PartialConfigurations partialConfigurations;
        private readonly IConfigurationRoot cfg;

        public PartialUpdateController(
            IMediator mediator,
            IConfiguration configuration,
            PartialConfigurations partialConfigurations

        )
        {
            this.mediator = mediator;
            this.partialConfigurations = partialConfigurations;
            var cs = configuration.GetValue<string>("ConnectionString");
            cfg = new ConfigurationBuilder()
                .AddEFConfiguration(options => options.UseSqlServer(cs, configure =>
                {
                    configure.MigrationsAssembly(typeof(EntityFrameworkExtensions).Assembly.FullName);
                })).Build();
        }

        [HttpGet]
        public T Get()
        {
            var path = Request.Path.Value;
            var position = path.LastIndexOf("/") + 1;
            var configurationId = path[position..];

            var configuration = partialConfigurations.Get().Single(v => v.Id == configurationId);
            var options = Activator.CreateInstance<T>();
            cfg.GetSection(configuration.SectionId).Bind(options);
            return options;
            //return options.Value;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PartialUpdateRaw<T> value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await mediator.Send(value.ToPartialUpdate());
            return Ok();
        }
    }
}
