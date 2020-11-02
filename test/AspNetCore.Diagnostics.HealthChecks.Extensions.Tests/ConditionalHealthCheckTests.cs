using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions.Tests
{
    public class ConditionalHealthCheckTests
    {
        [Fact]
        public async Task Api()
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
                    .CheckOnlyWhen("Test2", sp => true);

            serviceCollection.AddHealthChecks()
               .AddCheck("Test3", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen("Test3", async sp => await Task.FromResult(true));

            serviceCollection.AddHealthChecks()
                .AddCheck("Test4", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("Test4", (sp, context) => true);

            serviceCollection.AddHealthChecks()
                .AddCheck("Test5", () => HealthCheckResult.Unhealthy())
                    .CheckOnlyWhen("Test5", async (sp, context) => await Task.FromResult(true));

            serviceCollection.AddHealthChecks()
               .AddCheck("Test6", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen<SomePolicy>("Test6");

            serviceCollection.AddHealthChecks()
               .AddCheck("Test7", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen("Test7", new SomeParametrizedPolicy("Some variation"));

            serviceCollection.AddHealthChecks()
               .AddCheck("Test8", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen("Test8", () => new SomeParametrizedPolicy("Some variation"));

            serviceCollection.AddHealthChecks()
               .AddCheck("Test9", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen("Test9", async sp => await Task.FromResult(new SomeParametrizedPolicy(variation: "Some variation")));

            serviceCollection.AddHealthChecks()
               .AddCheck("Test10", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen("Test10", sp => new SomeParametrizedPolicy(variation: "Some variation"));

            serviceCollection.AddHealthChecks()
               .AddCheck("Test11", () => HealthCheckResult.Unhealthy())
                   .CheckOnlyWhen("Test11", async (sp, context) => await Task.FromResult(new SomeParametrizedPolicy(variation: "Some variation")));

            Assert.True(true);
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
