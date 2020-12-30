using System.Net.Http;
using System.Threading.Tasks;
using Glow.Users;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Glow.Glue.AspNetCore.Tests
{
    public class GetBuilder<T> : BaseRequestBuilder<T> where T : class
    {
        public GetBuilder(WebApplicationFactory<T> factory) : base(factory)
        {

        }

        public GetBuilder<T> From(string url)
        {
            Url = url;
            return this;
        }

        public GetBuilder<T> As(string userId)
        {
            UserId = userId;
            return this;
        }

        public GetBuilder<T> As(UserDto user)
        {
            UserId = user.Id;
            return this;
        }

        public new Task<R> Read<R>()
        {
            return base.Read<R>();
        }

        public new Task<HttpResponseMessage> ExecuteRaw()
        {
            return base.ExecuteRaw();
        }
    }

}
