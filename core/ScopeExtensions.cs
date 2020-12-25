using Microsoft.Extensions.DependencyInjection;

namespace Glow.Invoices.Api.Test
{
    public static class ScopeExtensions
    {
        public static T GetService<T>(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetService<T>();
        }

        public static T GetRequiredService<T>(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<T>();
        }
    }
}
