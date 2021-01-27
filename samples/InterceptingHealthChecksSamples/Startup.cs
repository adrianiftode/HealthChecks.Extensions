using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InterceptingHealthChecksSamples.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace InterceptingHealthChecksSamples
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) => services.AddHealthChecks()

                // Check on Redis only when the environment is other than development
                //.AddRedis(Configuration["Redis:ConnectionString"])
                //    .Retry(Registrations.Redis, Times(2).Wait(new[]
                //            {
                //                TimeSpan.FromSeconds(1),
                //                TimeSpan.FromSeconds(2),
                //                TimeSpan.FromSeconds(3)
                //            }))

                // Express the condition as a predicate.
                // Due to this, the condition is evaluated with every request
                //
                // In this example when the `ADependency:Setting` config is changed,
                // then the health check on `A Dependency` is re-evaluated as usual.
                //
                // In the Redis example such a condition is evaluated once, at startup.
                .AddCheck("Check5", () => HealthCheckResult.Unhealthy())
                    .Retry("Check5", retries: 3)

                .AddCheck("Check6", () => HealthCheckResult.Unhealthy())
                    .Retry("Check6", waitAndRetryIntervals: new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) })

                .AddCheck("Check7", () => HealthCheckResult.Unhealthy())
                    .Retry("Check7", retries: 3, waitBetweenRetries: TimeSpan.FromSeconds(2))

                .AddCheck("Retry With Polly", () => HealthCheckResult.Unhealthy())
                    .Retry("Retry With Polly", async (healthCheck, sp, context, token) =>
                    {
                        var logger = sp.GetRequiredService<ILogger<Startup>>();
                        return await Policy
                            .HandleResult<HealthCheckResult>(r => r.Status == HealthStatus.Unhealthy)
                            .RetryAsync(3, (result, retry, _) =>
                            {
                                logger.LogDebug("Retried with Polly for {times} and received {resultStatus}.", retry,
                                    result.Result.Status);
                            })
                            .ExecuteAsync(() => healthCheck.CheckHealthAsync(context, token));
                    })

                //// Check on RabbitMQ only a specific feature flag is set.
                //// The condition is executed asynchronously with every health check request
                //// and it has access to the ServiceProvider and the HealthCheckContext.
                //.AddRabbitMQ()
                //    .CheckOnlyWhen(Registrations.RabbitMQ, async (sp, context, token) =>
                //                {
                //                    var featureFlags = sp.GetService<IFeatureFlags>();
                //                    return await featureFlags.IsSet("AFeatureDependingOnRabbitMQ");
                //                })

                //// Specify a typed policy to enable executing a health check
                //// This policy could have an exhaustive implementation and
                //// reused with other dependencies.
                //.AddCheck("Dependency1", () => HealthCheckResult.Healthy())
                //    .CheckOnlyWhen<FeatureFlagsPolicy>("Dependency1",
                //        conditionalHealthCheckPolicyCtorArgs: "A Flag for a Feature that depends on Dependency1")

                //// Specify a typed policy to check on multiple health checks
                //.AddCheck("Dependency2", () => HealthCheckResult.Healthy())
                //.AddCheck("Dependency3", () => HealthCheckResult.Healthy())
                //    .CheckOnlyWhen<FeatureFlagsPolicy>(new[] { "Dependency2", "Dependency3" },
                //        conditionalHealthCheckPolicyCtorArgs: "A Flag for a Feature that depends on both dependencies.")

                //// Customize health check responses when the health check registration is not evaluated
                //// By default the HealthStatus is HealthStatus.Healthy 

                //// A tag is also included in the conditional health check entry
                //// to mark the fact it was not checked
                ////
                //// By default this value the tag name is `NotChecked`, so this is also customizable
                //.AddCheck("CustomizedStatus", () => HealthCheckResult.Healthy())
                //    .CheckOnlyWhen("CustomizedStatus",
                //        conditionToRun: true,
                //        options: new ConditionalHealthCheckOptions
                //        {
                //            HealthStatusWhenNotChecked = HealthStatus.Degraded,
                //            NotCheckedTagName = "NotActive"
                //        });
                ;//services.AddSingleton<IFeatureFlags, FeatureFlags>();

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = ResponseWriter.WriteResponse
                });
            });
        }
    }

    // IFeatureFlags could be a service providing responses to questions like if a feature flag is set or not
    internal interface IFeatureFlags
    {
        Task<bool> IsSet(string featureName);
    }

    internal class FeatureFlags : IFeatureFlags
    {
        public Task<bool> IsSet(string featureName) => Task.FromResult(false);
    }

    // A ConditionalHealthCheckPolicy can then be created to build a parametrized policy by feature flags
    // So you setup each health check registration to be actually checked only when a feature flag is active
    // without writing the resolving code every time
    internal class FeatureFlagsPolicy : IConditionalHealthCheckPolicy
    {
        private readonly IFeatureFlags _featureFlags;
        private readonly ILogger<FeatureFlagsPolicy> _logger;
        private string FeatureName { get; }

        public FeatureFlagsPolicy(string featureName, IFeatureFlags featureFlags, ILogger<FeatureFlagsPolicy> logger)
        {
            _featureFlags = featureFlags;
            _logger = logger;
            FeatureName = featureName;
        }

        public Task<bool> Evaluate(HealthCheckContext context)
        {
            _logger.LogInformation("Checking if the `{flag}` is set, in order to evaluate " +
                                   "if health check `{name}` should be executed.", FeatureName, context.Registration.Name);
            return _featureFlags.IsSet(FeatureName);
        }
    }
}
