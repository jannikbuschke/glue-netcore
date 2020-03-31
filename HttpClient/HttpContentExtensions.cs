using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JannikB.Glue.AspNetCore
{
    public static class HttpExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync());
        }

        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string url, T data)
        {

            var dataAsString = JObject.FromObject(data).ToString();// JsonSerializer.Serialize<T>(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return httpClient.PostAsync(url, content);
        }
    }
}
