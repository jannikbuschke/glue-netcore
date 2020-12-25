using System.Linq;
using Glow.Glue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;


namespace Glow.Validation
{
    public class ValidatableAttribute : ActionFilterAttribute
    {
        private const string ParameterName = "x-submit-intent";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            StringValues header = context.HttpContext.Request.Headers[ParameterName];
            var intent = header.FirstOrDefault();
            if (intent == null)
            {
                context.Result = new BadRequestObjectResult($"Missing header '{ParameterName}'");
                return;
            }

            switch (intent)
            {
                case "validate":
                    {
                        context.Result = new ObjectResult(new SerializableValidationResult
                        {
                            IsValid = context.ModelState.IsValid,
                            Errors = new SerializableError(context.ModelState)
                        });
                    }
                    break;
                case "execute":
                    {
                        if (!context.ModelState.IsValid)
                        {
                            context.Result = new BadRequestObjectResult(context.ModelState);
                        }
                        break;
                    }
                default: context.Result = new BadRequestObjectResult($"Unknown _action parameter value: '{ParameterName}'"); break;
            }

        }
    }
}
