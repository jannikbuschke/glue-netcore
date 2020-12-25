using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;

namespace Glow.AzdoAuthentication
{
    public class AzdoClients
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ActiveUsersCache activeUsers;
        private readonly AzdoConfig config;

        public AzdoClients(
            IHttpContextAccessor httpContextAccessor,
            ActiveUsersCache activeUsers,
            AzdoConfig config
        )
        {
            this.httpContextAccessor = httpContextAccessor;
            this.activeUsers = activeUsers;
            this.config = config;
        }

        [Obsolete("use GetUserClient")]
        public Task<T> GetClient<T>() where T : VssHttpClientBase
        {
            return GetUserClient<T>();
        }

        public async Task<T> GetUserClient<T>() where T : VssHttpClientBase
        {
            ClaimsPrincipal user = httpContextAccessor.HttpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                Claim id = user.Claims.Single(v => v.Type == ClaimTypes.NameIdentifier);
                ActiveUser activeUser = await activeUsers.Get(id.Value);
                var credentials = new VssOAuthAccessTokenCredential(activeUser.AccessToken);
                var connection = new VssConnection(new Uri(config.OrganizationBaseUrl), credentials);
                return connection.GetClient<T>();
            }
            else
            {
                throw new Exception("User is no authenticated");
            }
        }

        public Task<T> GetAppClient<T>() where T : VssHttpClientBase
        {
            var connection = new VssConnection(
                new Uri(config.OrganizationBaseUrl),
                new VssBasicCredential("pat", config.Pat));

            return Task.FromResult(connection.GetClient<T>());
        }
    }
}
