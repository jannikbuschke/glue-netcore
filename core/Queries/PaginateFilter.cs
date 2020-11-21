using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Glow.Core.Queries
{
    public class PaginateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            IQueryCollection query = context.HttpContext.Request.Query;

            var objectResult = context.Result as ObjectResult;

            if (!(objectResult?.Value is IQueryable<object> queryable))
            {
                return;
            }

            var skipParam = query["$skip"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(skipParam) && int.TryParse(skipParam, out var skip))
            {
                queryable = queryable?.Skip(skip);
            }

            var takeParam = query["$take"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(takeParam) && int.TryParse(skipParam, out var take))
            {
                queryable = queryable?.Skip(take);
            }

            objectResult.Value = queryable;
            base.OnActionExecuted(context);
        }
    }
}