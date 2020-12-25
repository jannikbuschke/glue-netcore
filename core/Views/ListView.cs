using System;
using System.Linq;
using Glow.Core.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Glow.Core.Views
{
    [ApiController]
    public abstract class ListView<T> : ControllerBase
    {
        protected abstract IQueryable<T> Get(string search);

        [PaginateFilter]
        [HttpGet]
        [Obsolete("use /query instead")]
        public IQueryable<T> Query(string search)
        {
            return Get(search);
        }

        [HttpPost("query")]
        public QueryResult<T> Query(Query query)
        {
            return Get(query.Search).Apply(query);
        }
    }
}
