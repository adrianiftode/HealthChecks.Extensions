using System.Threading.Tasks;
using ConditionalHealthChecksSamples.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConditionalHealthChecksSamples
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
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                // Check on Redis only when the connection string is set
                .AddRedis(Configuration["Redis:ConnectionString"])
                    .CheckOnlyWhen(Registrations.Redis, !string.IsNullOrEmpty(Configuration["Redis:ConnectionString"]))

                // Check on RabbitMQ only a specific feature flag is set
                .AddRabbitMQ()
                    .CheckOnlyWhen(Registrations.RabbitMQ, async (sp, context, token) =>
                                {
                                    var featureFlags = sp.GetService<IFeatureFlags>();
                                    return await featureFlags.IsSet("AFeatureDependingOnRabbitMQ");
                                })

                // Specify a typed policy to enable executing a health check
                .AddCheck("Dependency1", () => HealthCheckResult.Healthy())
                    .CheckOnlyWhen<FeatureFlagsPolicy>("Dependency1", conditionalHealthCheckPolicyArgs: "A Flag for a Feature that depends on Dependency1")

                // Specify a typed policy to check on multiple health checks
                .AddCheck("Dependency2", () => HealthCheckResult.Healthy())
                .AddCheck("Dependency3", () => HealthCheckResult.Healthy())
                    .CheckOnlyWhen<FeatureFlagsPolicy>(new [] { "Dependency2", "Dependency3" }, conditionalHealthCheckPolicyArgs: "A Flag for a Feature that depends on Dependency2 and Dependency3")

                // Customize health check responses when the health check registration is not evaluated
                // By default the HealthStatus is HealthStatus.Healthy 
                // A tag is also included in the conditional health check entry to mark the fact it was not checked
                // By default this value the tag name is NotChecked, so this is also customizable
                .AddCheck("CustomizedStatus", () => HealthCheckResult.Healthy())
                    .CheckOnlyWhen("CustomizedStatus", whenCondition:true, options: new ConditionalHealthOptions
                            {
                                HealthStatusWhenNotChecked = HealthStatus.Degraded,
                                NotCheckedTagName = "NotActive"
                            })
                ;

            services.AddSingleton<IFeatureFlags, FeatureFlags>();
        }

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
