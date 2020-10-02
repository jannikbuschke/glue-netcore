using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Glow.Configurations
{
    //[Route("api/__configurations")]
    //[Authorize]
    public class UpdateController<T> : ControllerBase where T : class, new()
    {
        private readonly IMediator mediator;
        private readonly IConfiguration configuration;
        private readonly Configurations partialConfigurations;
        private readonly ConfigurationAuthorizationService authorization;
        private readonly IWebHostEnvironment environment;
        private readonly IConfigurationRoot efConfiguration;

        public UpdateController(
            IMediator mediator,
            IConfiguration configuration,
            Configurations partialConfigurations,
            ConfigurationAuthorizationService authorization,
            IWebHostEnvironment environment
        )
        {
            this.mediator = mediator;
            this.configuration = configuration;
            this.partialConfigurations = partialConfigurations;
            this.authorization = authorization;
            this.environment = environment;

            // TODO remove duplication
            var cs = configuration.GetValue<string>("ConnectionString");
            efConfiguration = new ConfigurationBuilder()
                .AddEFConfiguration(options => options.UseSqlServer(cs, configure =>
                {
                    configure.MigrationsAssembly(GetType().Assembly.FullName);
                })).Build();
        }

        [HttpGet]
        public async Task<ActionResult<T>> Get()
        {
            var isAllowed = await authorization.ReadPartialAllowed(Request.Path.Value);
            if (!isAllowed)
            {
                return Unauthorized();
            }
            var path = Request.Path.Value;
            var position = path.LastIndexOf("/") + 1;
            var configurationId = path[position..];

            Meta configurationMeta = partialConfigurations.Get().Single(v => v.Id == configurationId);
            T options = Activator.CreateInstance<T>();
            efConfiguration.GetSection(configurationMeta.SectionId).Bind(options);
            return options;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<T>> Get(string name)
        {
            var isAllowed = await authorization.ReadPartialAllowed(Request.Path.Value);
            if (!isAllowed)
            {
                return Unauthorized();
            }
            var path = Request.Path.Value.Replace("/" + name, "");
            var position = path.LastIndexOf("/") + 1;
            var configurationId = path[position..];

            Meta configurationMeta = partialConfigurations.Get().Single(v => v.Id == configurationId);
            T options = Activator.CreateInstance<T>();
            efConfiguration.GetSection(configurationMeta.SectionId + ":" + name).Bind(options);
            return options;
        }

        [Obsolete]
        [HttpGet("from-options")]
        public ActionResult<T> GetFromOptions([FromServices] IOptionsSnapshot<T> options)
        {
            if (environment.IsDevelopment())
            {
                return options.Value;
            }
            return NotFound();
        }

        [HttpGet("__description")]
        public TypeDescription GetDescription()
        {
            return GetDescription(typeof(T));
        }

        private TypeDescription GetDescription(Type t)
        {
            return new TypeDescription
            {
                Name = t.FullName,
                IsClass = t.IsClass,
                IsPrimitive = t.IsPrimitive,
                IsGenericType = t.IsGenericType,
                IsEnum = t.IsEnum,
                Properties = t.IsPrimitive ? null : t.GetProperties().Select(v => new PropertyDescription
                {
                    Name = v.Name,
                    TypeDescription = GetDescription(v.PropertyType)
                }).ToList()
            };
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] UpdateRaw<T> value)
        {
            var isAllowed = await authorization.UpdatePartialAllowed(Request.Path.Value);
            if (!isAllowed)
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await mediator.Send(value.ToConfigurationUpdate());
            return Ok();
        }
    }

    public class TypeDescription
    {
        public string Name { get; set; }
        public bool IsClass { get; set; }
        public bool IsGenericType { get; set; }
        public bool IsPrimitive { get; set; }
        public bool IsEnum { get; set; }
        public IEnumerable<PropertyDescription> Properties { get; set; }
    }

    public class PropertyDescription
    {
        public string Name { get; set; }
        public TypeDescription TypeDescription { get; set; }
    }
}
