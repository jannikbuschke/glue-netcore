using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Glow.TokenCache;
using Glow.Glue.AspNetCore;
using Glow.Invoices.Api.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Glow.AzdoAuthentication
{

    public class ActiveUsersCache
    {
        private readonly Dictionary<string, ActiveUser> tokens = new Dictionary<string, ActiveUser>();
        private readonly IServiceProvider services;
        private bool initialized = false;

        public ActiveUsersCache(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task Set(string userId, ActiveUser user)
        {
            using IServiceScope scope = services.CreateScope();
            ITokenDataContext ctx = scope.GetRequiredService<ITokenDataContext>();
            UserToken existingToken = await ctx.GlowTokenCache.SingleOrDefaultAsync(v => v.UserId == userId);
            if (existingToken == null)
            {
                ctx.GlowTokenCache.Add(new UserToken
                {
                    UserId = userId,
                    AccessToken = user.AccessToken
                });
            }
            else
            {
                existingToken.AccessToken = user.AccessToken;
            }
            await ctx.SaveChangesAsync();
            tokens[userId] = user;
        }

        public async Task<ActiveUser> Get(string userId)
        {
            if (!initialized)
            {
                await Initialize();
            }
            if (!tokens.ContainsKey(userId))
            {
                throw new ForbiddenException($"No token for user '{userId}' found");
            }
            return tokens[userId];
        }

        private async Task Initialize()
        {
            using IServiceScope scope = services.CreateScope();
            ITokenDataContext ctx = scope.GetRequiredService<ITokenDataContext>();
            List<UserToken> storedTokens = await ctx.GlowTokenCache.ToListAsync();
            foreach (UserToken token in storedTokens)
            {
                tokens[token.UserId] = new ActiveUser(token.UserId, "", "", token.AccessToken);
            }
            initialized = true;
        }
    }
}
