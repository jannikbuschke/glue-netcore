using System.Net.Http;
using System.Threading.Tasks;
using Glow.Users;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Glow.Tests
{
    public class SendBuilder<TRequest, TStartup> where TStartup : class
    {
        private readonly WebApplicationFactory<TStartup> factory;
        private readonly object request;
        private string url;
        private UserDto user;

        public SendBuilder(WebApplicationFactory<TStartup> factory, object request)
        {
            this.factory = factory;
            this.request = request;
        }

        public SendBuilder<TRequest, TStartup> To(string url)
        {
            this.url = url;
            return this;
        }

        public SendBuilder<TRequest, TStartup> As(string userId)
        {
            user = new UserDto { Id = userId };
            return this;
        }

        public SendBuilder<TRequest, TStartup> As(UserDto user)
        {
            this.user = user;
            return this;
        }

        public async Task<TRequest> ExecuteAndRead()
        {
            HttpResponseMessage response = await Execute();
            return await response.ReadAsAsync<TRequest>();
        }

        public async Task<HttpResponseMessage> Execute()
        {
            using HttpClient client = factory.CreateClient();

            client.DefaultRequestHeaders.Add("x-submit-intent", "execute");
            if (user != null)
            {
                if (user.Id != null)
                {
                    client.DefaultRequestHeaders.Add("x-userid", user.Id);
                }
                if (user.DisplayName != null)
                {
                    client.DefaultRequestHeaders.Add("x-username", user.DisplayName);
                }
            }
            HttpResponseMessage response = await client.PostAsJsonAsync(url, request);
            return response;
        }
    }
}
