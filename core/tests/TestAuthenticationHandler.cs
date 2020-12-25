using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Glow.Authentication.Aad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Glow.Tests
{
    public class AdditionalClaims
    {
        public IEnumerable<Claim> Claims { get; set; }
    }

    public static class ServiceExtensions
    {
        public static void AddTestAuthentication(this IServiceCollection services)
        {
            AddTestAuthentication(services, "test", "test", "test@email.com", new List<Claim>());
        }

        public static void AddTestAuthentication(
            this IServiceCollection services,
            string userId,
            string userName,
            string email
        )
        {
            AddTestAuthentication(services, userId, userName, email, new List<Claim>());
        }

        public static void AddTestAuthentication(
          this IServiceCollection services,
          string userId,
          string userName,
          string email,
          IEnumerable<Claim> additionalClaims
        )
        {
            IEnumerable<Claim> claims = additionalClaims.Concat(new[] {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email),
            });
            AddTestAuthentication(services, claims);
        }

        public static void AddTestAuthentication(
            this IServiceCollection services,
            IEnumerable<Claim> additionalClaims
        )
        {
            services.AddSingleton(new AdditionalClaims
            {
                Claims = additionalClaims
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = FakeAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FakeAuthenticationDefaults.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("TestAuthentication", null);
        }
    }

    public static class FakeAuthenticationDefaults
    {
        public const string AuthenticationScheme = "TestAuthentication";
    }

    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IEnumerable<Claim> claims;
        private readonly IHttpContextAccessor httpContextAccessor;

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AdditionalClaims additionalClaims,
            IHttpContextAccessor httpContextAccessor
        )
        : base(options, logger, encoder, clock)
        {
            claims = additionalClaims.Claims;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            StringValues userIds = httpContextAccessor.HttpContext.Request.Headers["x-userid"];
            var userId = userIds.FirstOrDefault();
            StringValues userNames = httpContextAccessor.HttpContext.Request.Headers["x-username"];
            var userName = userIds.FirstOrDefault();
            IEnumerable<Claim> newClaims = claims
                .Select(v => userId != null && v.Type == ClaimsPrincipalExtensions.ObjectId ? new Claim("oid", userId) : v)
                .Select(v => userId != null && v.Type == "oid" ? new Claim("oid", userId) : v)
                .Select(v => userId != null && v.Type == ClaimTypes.NameIdentifier ? new Claim(ClaimTypes.NameIdentifier, userId) : v)
                .Select(v => userName != null && v.Type == ClaimTypes.Name ? new Claim(ClaimTypes.Name, userName) : v);

            var identity = new ClaimsIdentity(newClaims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
