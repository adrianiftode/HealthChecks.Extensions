using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.Extensions.Tests
{
    public class ApiTests
    {
        [Fact]
        public void Api()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck0", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck0", true);

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck1", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck1", () => true);

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck7", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck7",
                    async (sp, context, token) => await Task.Factory.StartNew(() => true, token));

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck8", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen<SomePolicy>("HealthCheck8");

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck9", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck9", new SomeParametrizedPolicy("Some variation"));

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck10", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck10", () => new SomeParametrizedPolicy("Some variation"));

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck14", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck14",
                    async (sp, context, token) =>
                        await Task.FromResult(new SomeParametrizedPolicy(variation: "Some variation")));

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck15", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("HealthCheck15", true, new ConditionalHealthCheckOptions { HealthStatusWhenNotChecked = HealthStatus.Degraded });

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck16", () => HealthCheckResult.Unhealthy())
                .AddCheck("HealthCheck17", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen(new[] { "HealthCheck16", "HealthCheck17" }, (sp, context, token) => Task.FromResult(true));

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck18", () => HealthCheckResult.Unhealthy())
                .AddCheck("HealthCheck19", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen(new[] { "HealthCheck18", "HealthCheck19" }, conditionToRun: true);

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck20", () => HealthCheckResult.Unhealthy())
                .AddCheck("HealthCheck21", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen(new[] { "HealthCheck20", "HealthCheck21" }, () => true);

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck20", () => HealthCheckResult.Unhealthy())
                .AddCheck("HealthCheck21", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen<SomePolicy>(new[] { "HealthCheck20", "HealthCheck21" });

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck20", () => HealthCheckResult.Unhealthy())
                .AddCheck("HealthCheck21", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen(new[] { "HealthCheck20", "HealthCheck21" }, new SomePolicy());

            serviceCollection.AddHealthChecks()
                .AddCheck("HealthCheck20", () => HealthCheckResult.Unhealthy())
                .AddCheck("HealthCheck21", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen(new[] { "HealthCheck20", "HealthCheck21" }, () => new SomePolicy());

            serviceCollection.AddHealthChecks()
                .AddCheck(Registrations.Redis, () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen(Registrations.Redis, true);

            Action act = () => _ = serviceCollection.BuildServiceProvider()
                .GetService<IOptions<HealthCheckServiceOptions>>()
                .Value;

            act.Should().NotThrow();
        }
    }

    public class SomePolicy : IConditionalHealthCheckPolicy
    {
        public Task<bool> Evaluate(HealthCheckContext context) => Task.FromResult(true);
    }

    public class SomeParametrizedPolicy : IConditionalHealthCheckPolicy
    {
        private readonly string _variation;
        public SomeParametrizedPolicy(string variation) => _variation = variation;
        public Task<bool> Evaluate(HealthCheckContext context) => Task.FromResult(true);
    }
}
