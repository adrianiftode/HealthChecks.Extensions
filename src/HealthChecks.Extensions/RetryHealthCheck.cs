using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Extensions
{
    internal class RetryHealthCheck : IHealthCheck
    {
        private readonly RetryHealthCheckOptions _options;
        private readonly ILogger<RetryHealthCheck>? _logger;
        private readonly Func<IHealthCheck, HealthCheckContext, CancellationToken, Task<HealthCheckResult>> _retryStrategy;
        private readonly Func<IHealthCheck> _healthCheckFactory;

        public RetryHealthCheck(
            Func<IHealthCheck> healthCheckFactory,
            Func<IHealthCheck, HealthCheckContext, CancellationToken, Task<HealthCheckResult>> retryStrategy,
            RetryHealthCheckOptions? options,
            ILogger<RetryHealthCheck>? logger)
        {
            _healthCheckFactory = healthCheckFactory;
            _options = RetryHealthCheckOptions.DefaultFrom(options);
            _logger = logger;
            _retryStrategy = retryStrategy ?? throw new ArgumentNullException(nameof(retryStrategy));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            context.Registration.Tags.Remove(_options.RetriedTagName);
            _logger?.LogDebug("HealthCheck `{0}` will be retried.", context.Registration.Name);

            context.Registration.Tags.Add(_options.RetriedTagName);

            var healthCheck = _healthCheckFactory();
            var healthCheckExecutionRecorder = new HealthCheckExecutionRecorder(healthCheck);

            var result = await _retryStrategy(healthCheckExecutionRecorder, context, cancellationToken);

            if (!healthCheckExecutionRecorder.HealthCheckCalled)
            {
                throw new InvalidOperationException($"The health check `{context.Registration.Name}` was not executed.");
            }

            return result;
        }

        private class HealthCheckExecutionRecorder : IHealthCheck
        {
            private readonly IHealthCheck _healthCheck;

            public HealthCheckExecutionRecorder(IHealthCheck healthCheck)
            {
                _healthCheck = healthCheck;
            }

            public bool HealthCheckCalled { get; private set; }

            public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                HealthCheckCalled = true;
                return _healthCheck.CheckHealthAsync(context, cancellationToken);
            }
        }
    }

    public interface IRetryPolicy
    {
        Task<HealthCheckResult> Execute(IHealthCheck healthCheck, HealthCheckContext context, CancellationToken cancellationToken = default);
    }

    internal class DefaultRetryPolicy : IRetryPolicy
    {
        private readonly ILogger<DefaultRetryPolicy> _logger;
        private readonly TimeSpan[] _waitAndRetryIntervals;

        public DefaultRetryPolicy(TimeSpan[] waitAndRetryIntervals, ILogger<DefaultRetryPolicy> logger)
        {
            _waitAndRetryIntervals = waitAndRetryIntervals;
            _logger = logger;
        }

        public async Task<HealthCheckResult> Execute(IHealthCheck healthCheck, HealthCheckContext context, CancellationToken token = default)
        {
            HealthCheckResult result;
            var retriesIndex = 0;
            var retries = _waitAndRetryIntervals.Length;
            do
            {
                _logger.LogDebug("Retrying health check `{name}` with attempt number {times} from total of {total}.", context.Registration.Name,
                    retriesIndex + 1, retries);

                result = await healthCheck.CheckHealthAsync(context, token);

                var wait = _waitAndRetryIntervals[retriesIndex];

                _logger.LogDebug("Retried health check `{name}` with result {resultStatus}, retrying after {wait}.", context.Registration.Name,
                    result.Status, wait);
                await Task.Delay(wait, token);

                retriesIndex++;
            } while (result.Status == HealthStatus.Healthy || retriesIndex < retries);

            return result;
        }
    }
}