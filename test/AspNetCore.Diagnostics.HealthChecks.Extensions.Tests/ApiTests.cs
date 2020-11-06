using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions.Tests
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
                    .AddCheck("Test2", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test2", async token => await Task.FromResult(true));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test3", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test3", sp => true);

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test4", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test4", async token => await Task.FromResult(true));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test5", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test5", (sp, context) => true);

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test6", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test6", async (sp, token) => await Task.FromResult(true));

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
                    .AddCheck("Test11", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test11",
                        async token => await Task.FromResult(new SomeParametrizedPolicy("Some variation")));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test12", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test12",
                        async (sp, token) =>
                            await Task.FromResult(new SomeParametrizedPolicy(variation: "Some variation")));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test13", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test13", sp => new SomeParametrizedPolicy(variation: "Some variation"));

                serviceCollection.AddHealthChecks()
                    .AddCheck("Test14", () => HealthCheckResult.Unhealthy())
                        .CheckOnlyWhen("Test14",
                        async (sp, context, token) =>
                            await Task.FromResult(new SomeParametrizedPolicy(variation: "Some variation")));
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
