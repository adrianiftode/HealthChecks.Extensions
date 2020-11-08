using System.Threading.Tasks;
using ConditionalHealthChecksSamples.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

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
                    .CheckOnlyWhen<FeatureFlagsPolicy>("Dependency1", conditionalHealthCheckPolicyArgs: "A Feature Depending On Dependency1")

                // Specify a typed policy to check on multiple health checks
                .AddCheck("Dependency2", () => HealthCheckResult.Healthy())
                .AddCheck("Dependency3", () => HealthCheckResult.Healthy())
                    .CheckOnlyWhen<FeatureFlagsPolicy>(new [] { "Dependency2", "Dependency3" }, conditionalHealthCheckPolicyArgs: "A Feature Depending on Dependency2 and Dependency3")
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

    internal interface IFeatureFlags
    {
        Task<bool> IsSet(string featureName);
    }

    internal class FeatureFlags : IFeatureFlags
    {
        public Task<bool> IsSet(string featureName) => Task.FromResult(false);
    }

    internal class FeatureFlagsPolicy : IConditionalHealthCheckPolicy
    {
        private readonly IFeatureFlags _featureFlags;
        private string FeatureName { get; }

        public FeatureFlagsPolicy(string featureName, IFeatureFlags featureFlags)
        {
            _featureFlags = featureFlags;
            FeatureName = featureName;
        }
        public Task<bool> Evaluate(HealthCheckContext context) => _featureFlags.IsSet(FeatureName);
    }
}
