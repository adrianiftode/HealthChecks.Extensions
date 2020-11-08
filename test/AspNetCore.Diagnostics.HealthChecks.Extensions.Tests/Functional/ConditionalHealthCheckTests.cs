using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions.Tests.Functional
{
    public class ConditionalHealthCheckTests
    {
        [Fact]
        public async Task Checks_When_Predicate_Returns_True()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("TheCheck", () => HealthCheckResult.Healthy(), new[] { "ThatCheck" })
                            .CheckOnlyWhen("TheCheck", true);
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = ResponseWriter.WriteResponse
                    });
                });

            var server = new TestServer(webHostBuilder);

            // Act
            var response = await server.CreateRequest("/health").GetAsync();

            // Assert
            response.Should().Be200Ok()
                .And.Satisfy(givenModelStructure: new
                {
                    entries = new[]
                    {
                        new
                        {
                            name = default(string),
                            status = default(HealthStatus),
                            tags = new string[] { }
                        }
                    }
                }, assertion: model =>
                {
                    model.entries.Should().Contain(entry => entry.name == "TheCheck");
                });
        }

        [Fact]
        public async Task Does_Not_Check_When_Predicate_Returns_False()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("TheCheck", () => HealthCheckResult.Healthy(), new[] { "ThatCheck" })
                        .CheckOnlyWhen("TheCheck", false);
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = ResponseWriter.WriteResponse
                    });
                });

            var server = new TestServer(webHostBuilder);

            // Act
            var response = await server.CreateRequest("/health").GetAsync();

            // Assert
            response.Should().Be200Ok()
                .And.Satisfy(givenModelStructure: new
                {
                    entries = new[]
                    {
                        new
                        {
                            name = default(string),
                            description = default(string),
                            status = default(HealthStatus),
                            tags = new string[] { }
                        }
                    }
                }, assertion: model =>
                {
                    model.entries.Should()
                        .NotBeNullOrEmpty()
                        .And.Contain(entry => entry.name == "TheCheck");
                    var result = model.entries[0];
                    result.tags.Should().Contain("NotChecked");
                    result.description.Should().Match("*check on `TheCheck` will not be evaluated*");
                });
        }

        [Fact]
        public async Task Checks_According_To_The_ConditionalHealthCheckPolicy()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("ThisPolicyShouldNotBeChecked", () => HealthCheckResult.Healthy())
                            .CheckOnlyWhen<CheckOrNotCheckPolicy>("ThisPolicyShouldNotBeChecked", default, conditionalHealthCheckPolicyArgs: false)
                        .AddCheck("ThisPolicyShouldBeChecked", () => HealthCheckResult.Healthy())
                            .CheckOnlyWhen<CheckOrNotCheckPolicy>("ThisPolicyShouldBeChecked", default, conditionalHealthCheckPolicyArgs: true)
                        .AddCheck("AlsoThisPolicyShouldNotBeChecked", () => HealthCheckResult.Healthy())
                            .CheckOnlyWhen<CheckOrNotCheckPolicy>("AlsoThisPolicyShouldNotBeChecked", default, conditionalHealthCheckPolicyArgs: false);
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = ResponseWriter.WriteResponse
                    });
                });

            var server = new TestServer(webHostBuilder);

            // Act
            var response = await server.CreateRequest("/health").GetAsync();

            // Assert
            response.Should().Satisfy(givenModelStructure: new
                {
                    entries = new[]
                    {
                        new
                        {
                            tags = new string[] { }
                        }
                    }
                }, assertion: model =>
                {
                    model.entries.Should().SatisfyRespectively(
                        firstPolicy => firstPolicy.tags.Should().Contain("NotChecked"),
                        secondPolicy => secondPolicy.tags.Should().NotContain("NotChecked"),
                        thirdPolicy => thirdPolicy.tags.Should().Contain("NotChecked"));
                });
        }

        internal class CheckOrNotCheckPolicy : IConditionalHealthCheckPolicy
        {
            private readonly bool _evaluate;

            public CheckOrNotCheckPolicy(bool evaluate) => _evaluate = evaluate;
            public Task<bool> Evaluate(HealthCheckContext context) => Task.FromResult(_evaluate);
        }
    }
}
