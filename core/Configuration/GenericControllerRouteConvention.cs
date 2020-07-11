using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EfConfigurationProvider.Core
{
    public class GenericControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType)
            {
                Type genericType = controller.ControllerType.GenericTypeArguments[0];
                GeneratedControllerAttribute customNameAttribute = genericType.GetCustomAttribute<GeneratedControllerAttribute>();

                if (customNameAttribute?.Route != null)
                {
                    controller.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("api/configurations/" + customNameAttribute.Id)),
                    });
                }
            }
        }
    }
}
