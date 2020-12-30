using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Glow.Glue.AspNetCore.Tests
{
    public abstract class BaseRequestBuilder<T> where T : class
    {
        private readonly WebApplicationFactory<T> factory;
        protected string Url { get; set; }
        protected string UserId { get; set; }

        public BaseRequestBuilder(WebApplicationFactory<T> factory)
        {
            this.factory = factory;
        }

        protected async Task<R> Read<R>()
        {
            HttpResponseMessage response = await ExecuteRaw();
            if (response.IsSuccessStatusCode)
            {
                R responsePayload = await response.Content.ReadAsAsync<R>();
                return responsePayload;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new BadRequestException(content);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception("Http call was unsuccessfull: " + response.StatusCode + " " + response.ReasonPhrase + " " + content);
            }
        }

        protected async Task<HttpResponseMessage> ExecuteRaw()
        {
            using HttpClient client = factory.CreateClient();

            client.DefaultRequestHeaders.Add("x-submit-intent", "execute");
            if (!string.IsNullOrWhiteSpace(UserId))
            {
                client.DefaultRequestHeaders.Add("x-userid", UserId);
            }

            HttpResponseMessage response = await client.GetAsync(Url);
            return response;
        }
    }

}
