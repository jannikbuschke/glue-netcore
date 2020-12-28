using System;
using System.Threading.Tasks;
using Glow.Core.Authentication;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Glow.Core.AzdoAuthentication
{
    // glow/profile uses IGraphTokenService to get some scopes
    // in azdo apps we use this to mock it (workaround)
    public class MockedGraphTokenService : IGraphTokenService
    {
        public Task<string> AccessTokenForApp()
        {
            throw new NotImplementedException();
        }

        public Task<string> AccessTokenForCurrentUser(string[] scope)
        {
            throw new NotImplementedException();
        }

        public Task<GraphServiceClient> GetClientForUser(string[] scopes, bool useBetaEndpoint = false)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResult> TokenForCurrentUser(string[] scope)
        {
            throw new NotImplementedException();
        }
    }
}
