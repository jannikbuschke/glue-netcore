using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Glow.Core.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Glow.MsGraph
{
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/graph")]
    public class GraphProxyController : ControllerBase
    {
        private readonly IHttpClientFactory factory;
        private readonly IGraphTokenService graphTokenService;
        private readonly ILogger<GraphProxyController> logger;

        public GraphProxyController(
            IHttpClientFactory factory,
            IGraphTokenService graphTokenService,
            ILogger<GraphProxyController> logger
        )
        {
            this.factory = factory;
            this.graphTokenService = graphTokenService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<object>> Get(string path)
        {
            try
            {
                HttpClient client = factory.CreateClient();
                client.BaseAddress = new Uri("https://graph.microsoft.com");
                var accessToken = await graphTokenService.AccessTokenForCurrentUser(new string[] { "openid" });
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var uri = "https://graph.microsoft.com/" + path;
                logger.LogInformation("GET {uri}", uri);
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(JObject.Parse(content).ToObject<dynamic>());
                }
                else
                {
                    var msg = path + await response.Content.ReadAsStringAsync();
                    return StatusCode((int) response.StatusCode, msg);
                }
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: (int) HttpStatusCode.InternalServerError, title: "Server Error");
            }
        }
    }
}
