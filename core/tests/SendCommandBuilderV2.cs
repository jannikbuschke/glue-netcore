using System;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Glow.Tests
{
    [Obsolete("use SendBuilder")]
    public class SendCommandBuilderV2<R, Startup> : SendBuilder<R, Startup> where Startup : class
    {
        public SendCommandBuilderV2(WebApplicationFactory<Startup> factory, IRequest<R> request)
            : base(factory, request) { }
    }
}
