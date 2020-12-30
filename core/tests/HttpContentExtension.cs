using System.Net.Http;
using System.Threading.Tasks;

namespace Glow.Tests
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
}
