using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using Xunit;

namespace HealthChecks.Extensions.Tests
{
    public class ApiTests
    {
        [Fact]
        public void Api()
        {
            Action act = () =>
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test0", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test0", true);

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test1", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test1", () => true);

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test7", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test7",
                        async (sp, context, token) => await Task.Factory.StartNew(() => true, token));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test7", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen<SomePolicy>("Test7");

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test9", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test9", new SomeParametrizedPolicy("Some variation"));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test10", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test10", () => new SomeParametrizedPolicy("Some variation"));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test14", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test14",
                        async (sp, context, token) =>
                            await Task.FromResult(new SomeParametrizedPolicy(variation: "Some variation")));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test15", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test15", true, new ConditionalHealthOptions { HealthStatusWhenNotChecked = HealthStatus.Degraded });

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test16", () => HealthCheckResult.Unhealthy())
                    .AddCheck("Test17", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen(new [] { "Test16", "Test17" }, (sp, context, token) => Task.FromResult(true) );

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test18", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen(Registrations.Redis, true);
            };

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
