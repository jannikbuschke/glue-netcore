using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Glow.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Glow.Core.Actions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        public string Route { get; set; }
        public string Policy { get; set; }
    }

    public class ActionsControllerProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IEnumerable<Assembly> assemblies;

        public ActionsControllerProvider(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = assemblies ?? throw new NullReferenceException();
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            Serilog.Log.Logger.Information("Populate " + nameof(ActionsControllerProvider));

            IEnumerable<Type> candidates = assemblies
                .SelectMany(v => v.GetExportedTypes()
                .Where(x => x.GetCustomAttributes(typeof(ActionAttribute), true).Any()));

            foreach (Type candidate in candidates)
            {
                try
                {
                    //var foo = typeof(ActionController<,>)
                    //    .MakeGenericType(candidate, typeof(Unit));

                    Type[] interfaces = candidate.GetInterfaces();

                    if (interfaces.Contains(typeof(IRequest)))
                    {
                        feature.Controllers.Add(
                            typeof(ActionController<,>).MakeGenericType(candidate, typeof(Unit)).GetTypeInfo()
                        );
                    }
                    else
                    {
                        Type arg = interfaces.FirstOrDefault().GenericTypeArguments?.FirstOrDefault();
                        feature.Controllers.Add(
                            typeof(ActionController<,>).MakeGenericType(candidate, arg).GetTypeInfo()
                        );
                    }

                    //var g = candidate.GenericTypeArguments;

                    //feature.Controllers.Add(
                    //    typeof(ActionController<,>).MakeGenericType(candidate).GetTypeInfo()
                    //);
                }
                catch (Exception)
                {

                }
            }
        }
    }

    [ApiController]
    // workaround: ApiController attributes need a RouteAttribute
    [Route("api/actions/__generic")]
    public class ActionController<T, Payload> where T : IRequest<Payload>
    {
        private readonly IMediator mediator;

        public ActionController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Validatable]
        [HttpPost]
        public Task<Payload> Execute(T request)
        {
            return mediator.Send(request);
        }
    }

    public class ActionsControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType)
            {
                Type genericType = controller.ControllerType.GenericTypeArguments[0];
                ActionAttribute attribute = genericType.GetCustomAttribute<ActionAttribute>();
                if (attribute != null)
                {
                    controller.Filters.Add(new ApiControllerAttribute());
                    controller.Filters.Add(new AuthorizeFilter(attribute.Policy));
                    controller.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(attribute.Route)),
                    });
                }
            }
        }
    }
}
