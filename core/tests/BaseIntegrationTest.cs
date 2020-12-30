using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Glow.Clocks;
using Glow.Glue.AspNetCore.Tests;
using Glow.Tests;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Glow.Core.Tests
{
    public interface IFixture<Startup> : IDisposable where Startup : class
    {
        WebApplicationFactory<Startup> Factory { get; }
    }

    public abstract class BaseIntegrationTest<Startup> : IDisposable where Startup : class
    {
        protected readonly HttpClient client;
        protected Faker Faker { get; set; } = new Faker();
        public BaseIntegrationTest(WebApplicationFactory<Startup> factory)
        {
            Factory = factory;
            client = Factory.CreateClient();
        }

        private FakeClock clock;
        protected FakeClock Clock
        {
            get
            {
                if (clock == null)
                {
                    clock = GetRequiredService<FakeClock>();
                }
                return clock;
            }
        }

        public WebApplicationFactory<Startup> Factory { get; }

        public void Dispose()
        {
            //client.Dispose();
            //Factory.Dispose();
        }

        protected T GetRequiredService<T>()
        {
            return Factory.Server.Host.Services.GetRequiredService<T>();
        }

        protected Task<T> SendRequest<T>(IRequest<T> request)
        {
            IMediator m = GetRequiredService<IMediator>();
            return m.Send(request);
        }

        protected SendCommandBuilderV2<TResponse, Startup> Send<TResponse>(IRequest<TResponse> request)
        {
            return new SendCommandBuilderV2<TResponse, Startup>(Factory, request);
        }

        protected QueryBuilder<Startup> Query(string url)
        {
            return new QueryBuilder<Startup>(Factory, url);
        }

        protected GetBuilder<Startup> Get()
        {
            return new GetBuilder<Startup>(Factory);
        }
    }
}
