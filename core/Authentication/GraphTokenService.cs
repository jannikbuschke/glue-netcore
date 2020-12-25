using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Glow.Authentication.Aad;
using Glow.Glue.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Glow.Core.Authentication
{
    public class GraphTokenService : IGraphTokenService
    {
        private readonly IHttpContextAccessor accessor;
        private readonly TokenService tokenService;
        private readonly IOptions<AzureAdOptions> options;
        private readonly ILogger<GraphTokenService> logger;

        public GraphTokenService(
            IHttpContextAccessor accessor,
            TokenService tokenService,
            IOptions<AzureAdOptions> options,
            ILogger<GraphTokenService> logger
        )

        {
            this.accessor = accessor;
            this.tokenService = tokenService;
            this.options = options;
            this.logger = logger;
        }

        public Task<string> AccessTokenForApp()
        {
            throw new System.NotImplementedException();
        }

        public async Task<AuthenticationResult> TokenForCurrentUser(string[] scopes)
        {
            if (accessor.HttpContext.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
            {
                var token = value.FirstOrDefault();

                AzureAdOptions o = options.Value;

                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId: o.ClientId)
                    .WithClientSecret(o.ClientSecret)
                    .WithTenantId(o.TenantId)
                    .Build();

                AuthenticationResult t = await app.AcquireTokenOnBehalfOf(scopes, new UserAssertion(token.Split(" ")[1])).ExecuteAsync();

                return t;
            }
            else
            {
                ClaimsPrincipal user = accessor.HttpContext.User;
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    throw new ForbiddenException("Not authenticated");
                }
                try
                {
                    AuthenticationResult result = await tokenService.GetAccessTokenAsync(user, scopes);
                    return result;
                }
                catch (MsalUiRequiredException e)
                {
                    if (e.Classification == UiRequiredExceptionClassification.ConsentRequired)
                    {
                        var message = "We could not yet fullfill you request. Please first allow the app to act on your behalf, then try again.";
                        logger.LogError("Missing consent {@scopes}", scopes);
                        throw new MissingConsentException(message, scopes[0]);
                    }
                    throw e;
                }
            }

        }

        public async Task<GraphServiceClient> GetClientForUser(string[] scopes, bool useBetaEndpoint = false)
        {
            var token = await AccessTokenForCurrentUser(scopes);
            var client = new GraphServiceClient(
                useBetaEndpoint ? "https://graph.microsoft.com/beta/" : "https://graph.microsoft.com/v1.0/",
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        return Task.FromResult(requestMessage);
                    }
                ));
            return client;
        }

        public async Task<string> AccessTokenForCurrentUser(string[] scopes)
        {
            AuthenticationResult result = await TokenForCurrentUser(scopes);
            return result.AccessToken;
        }
    }
}
