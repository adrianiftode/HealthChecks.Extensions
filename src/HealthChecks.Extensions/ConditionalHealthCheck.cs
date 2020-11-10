using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("HealthChecks.Extensions.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace HealthChecks.Extensions
{
    internal class ConditionalHealthCheck : IHealthCheck
    {
        private readonly Func<HealthCheckContext, CancellationToken, Task<bool>> _predicate;
        private readonly ConditionalHealthCheckOptions _options;
        private readonly ILogger<ConditionalHealthCheck>? _logger;

        public ConditionalHealthCheck(Func<IHealthCheck> healthCheckFactory,
            Func<HealthCheckContext, CancellationToken, Task<bool>> predicate,
            ConditionalHealthCheckOptions? options,
            ILogger<ConditionalHealthCheck>? logger)
        {
            HealthCheckFactory = healthCheckFactory ?? throw new ArgumentNullException(nameof(healthCheckFactory));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _options = ConditionalHealthCheckOptions.DefaultFrom(options);
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            context.Registration.Tags.Remove(_options.NotCheckedTagName);

            if (!await _predicate(context, cancellationToken))
            {
                _logger?.LogDebug("HealthCheck `{0}` will not be executed as its checking condition is not met.", context.Registration.Name);

                context.Registration.Tags.Add(_options.NotCheckedTagName);

                return new HealthCheckResult(_options.HealthStatusWhenNotChecked, $"Health check on `{context.Registration.Name}` will not be evaluated " +
                    "as its checking condition is not met. This does not mean your dependency is healthy, " +
                    "but the health check operation on this dependency is not configured to be executed yet.");
            }

            return await HealthCheckFactory().CheckHealthAsync(context, cancellationToken);
        }

        internal Func<IHealthCheck> HealthCheckFactory { get; set; }
    }
}
