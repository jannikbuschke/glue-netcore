using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Glow.Glue.AspNetCore
{

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;
        private readonly Stopwatch stopWatch = new Stopwatch();

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            stopWatch.Start();
            logger.LogDebug($"[Handling Request]: {typeof(TRequest).Name}");
            logger.LogTrace("Parameters {@values}", request);
            TResponse response = await next();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            logger.LogInformation("[Handled Request]: {requestName} ({timeMs} ms)", typeof(TRequest).Name, ts.TotalMilliseconds);
            logger.LogTrace("Response payload {@values}", response);
            return response;
        }
    }
}
