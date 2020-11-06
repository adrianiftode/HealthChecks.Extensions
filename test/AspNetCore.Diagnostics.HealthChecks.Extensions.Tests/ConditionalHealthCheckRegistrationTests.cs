using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions.Tests
{

    public class ConditionalHealthCheckRegistrationTests
    {
        [Fact]
        public async Task Disables_Health_Check_Execution_When_Configured()
        {
            // Arrange
            var executed = false;
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCheck("TheCheck", () =>
                {
                    executed = true;
                    return HealthCheckResult.Healthy();
                })
                .CheckOnlyWhen("TheCheck", whenCondition: false);

            var (check, registration) = Resolve(services);

            // Act
            _ = await check.CheckHealthAsync(new HealthCheckContext
            {
                Registration = registration
            });

            // Assert
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Enables_Health_Check_Execution_When_Configured()
        {
            // Arrange
            var executed = false;
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCheck("TheCheck", () =>
                {
                    executed = true;
                    return HealthCheckResult.Healthy();
                })
                .CheckOnlyWhen("TheCheck", whenCondition:true);

            var (check, registration) = Resolve(services);

            // Act
            _ = await check.CheckHealthAsync(new HealthCheckContext
            {
                Registration = registration
            });

            // Assert
            executed.Should().BeTrue();
        }

        [Fact]
        public void Original_Health_Check_Is_Original_By_ConditionalHealthCheck()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen("TheCheck", whenCondition: false);

            // Act
            var (check, registration) = Resolve(services);

            // Assert
            registration.Name.Should().Be("TheCheck");
            check.GetType().Should().Be(typeof(ConditionalHealthCheck));
        }

        [Fact]
        public void Throws_InvalidOperationException_When_The_Original_Health_Check_Is_Not_Found()
        {
            // Arrange
            Action act = () =>
            {
                var services = new ServiceCollection();
                services.AddHealthChecks()
                    .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                    .CheckOnlyWhen("A Name Other Than TheCheck", whenCondition: false);

                var (_, __) = Resolve(services);
            };

            act.Should().ThrowExactly<InvalidOperationException>()
                .WithMessage("*registration named `A Name Other Than TheCheck` is not found*");
        }

        private static (IHealthCheck check, HealthCheckRegistration registration) Resolve(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);
            return (check, registration);
        }
    }
}