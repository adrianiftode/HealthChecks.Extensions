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

            Assert.True(true);
        }
    }

    public class SomePolicy : IConditionalHealthCheckPolicy
    {
        public Task<bool> Evaluate(HealthCheckContext context) => Task.FromResult(true);
    }
}
