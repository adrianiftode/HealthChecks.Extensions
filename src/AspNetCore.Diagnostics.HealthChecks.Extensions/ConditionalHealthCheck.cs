using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions
{
    public class ConditionalHealthCheck : IHealthCheck
    {
        private const string NotChecked = "NotChecked";
        private readonly Func<HealthCheckContext, CancellationToken, Task<bool>> _predicate;
        private readonly ILogger<ConditionalHealthCheck>? _logger;

        public ConditionalHealthCheck(Func<IHealthCheck> healthCheckFactory,
            Func<HealthCheckContext, CancellationToken, Task<bool>> predicate,
            ILogger<ConditionalHealthCheck>? logger)
        {
            HealthCheckFactory = healthCheckFactory ?? throw new ArgumentNullException(nameof(healthCheckFactory));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            context.Registration.Tags.Remove(NotChecked);

            if (!await _predicate(context, cancellationToken))
            {
                _logger?.LogDebug("HealthCheck `{0}` will not be executed as its checking condition is not met.", context.Registration.Name);

                context.Registration.Tags.Add(NotChecked);

                return new HealthCheckResult(HealthStatus.Healthy, $"Health check on `{context.Registration.Name}` will not be evaluated " +
                    "as its checking condition is not met. This does not mean your dependency is healthy, " +
                    "but the health check operation on this dependency is not configured to be executed yet.");
            }

            return await HealthCheckFactory().CheckHealthAsync(context, cancellationToken);
        }

        internal Func<IHealthCheck> HealthCheckFactory { get; set; }
    }
}
