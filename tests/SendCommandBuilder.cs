using System.Net.Http;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JannikB.Glue.AspNetCore.Tests
{

    public static class HttpContentExtension
    {
        public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                T responsePayload = await response.Content.ReadAsAsync<T>();
                return responsePayload;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new System.Exception(response.ReasonPhrase + ": " + content);
            }
        }
    }

    public class SendCommandBuilderV2<R, Startup> where Startup : class
    {
        private readonly WebApplicationFactory<Startup> factory;
        private readonly IRequest<R> request;
        private string url;
        private UserDto user;

        public SendCommandBuilderV2(WebApplicationFactory<Startup> factory, IRequest<R> request)
        {
            this.factory = factory;
            this.request = request;
        }

        public SendCommandBuilderV2<R, Startup> To(string url)
        {
            this.url = url;
            return this;
        }

        public SendCommandBuilderV2<R, Startup> As(string userId)
        {
            user = new UserDto { Id = userId };
            return this;
        }

        public SendCommandBuilderV2<R, Startup> As(UserDto user)
        {
            this.user = user;
            return this;
        }

        public async Task<R> ExecuteAndRead()
        {
            HttpResponseMessage response = await Execute();
            return await response.ReadAsAsync<R>();
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
