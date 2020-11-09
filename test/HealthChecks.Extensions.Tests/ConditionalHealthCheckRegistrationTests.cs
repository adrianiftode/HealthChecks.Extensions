using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.Extensions.Tests
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
                .CheckOnlyWhen("TheCheck", conditionToRun: false);

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
                .CheckOnlyWhen("TheCheck", conditionToRun: true);

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
                .CheckOnlyWhen("TheCheck", conditionToRun: false);

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
            var services = new ServiceCollection();

            // Arrange
            Action act = () =>
            {
                services.AddHealthChecks()
                    .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                    .CheckOnlyWhen("A Name Other Than TheCheck", conditionToRun: false);

                var (_, __) = Resolve(services);
            };

            // Act
            act.Should().ThrowExactly<InvalidOperationException>()
                .WithMessage("*registration named `A Name Other Than TheCheck` was not found*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Throws_ArgumentException_When_The_Health_Check_Name_Is_Null_Or_Empty(string name)
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen(name, conditionToRun: false);

            // Assert
            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public void Throws_ArgumentException_When_The_Predicate_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen("TheCheck", (Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<bool>>)null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Throws_ArgumentException_When_The_Health_Check_Names_Array_Is_Empty()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen(new string[] { }, conditionToRun: false);

            // Assert
            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("*cannot be null*empty*");
        }

        [Fact]
        public void Throws_ArgumentException_When_The_Health_Check_Names_Array_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            string[] names = null;

            // Act
            Action act = () => services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen(names!, conditionToRun: false);

            // Assert
            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("*cannot be null*empty*");
        }

        [Fact]
        public void Throws_ArgumentException_For_Policy_When_The_Health_Check_Names_Array_Is_Empty()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen<SomePolicy>(new string[] { });

            // Assert
            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("*cannot be null*empty*");
        }

        [Fact]
        public async Task Throws_InvalidOperationException_When_Policy_Provider_Returns_A_Null_Policy()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCheck("TheCheck", () => HealthCheckResult.Healthy())
                .CheckOnlyWhen("TheCheck", () => (SomePolicy)null);

            // Act
            Func<Task> act = async () =>
            {

                var (policy, registration) = Resolve(services);

                await policy.CheckHealthAsync(new HealthCheckContext { Registration = registration });
            };

            // Assert
            (await act.Should().ThrowExactlyAsync<InvalidOperationException>())
                .WithMessage("A policy of type `SomePolicy` could not be retrieved as it was null.");
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