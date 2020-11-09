# HealthChecks.Extensions

A set of ASP.NET Health Checks extensions

[![Build status](https://ci.appveyor.com/api/projects/status/94f8gktyknvmmu6t/branch/main?svg=true)](https://ci.appveyor.com/project/adrianiftode/healthchecks-extensions/branch/main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=HealthChecks.Extensions&metric=alert_status)](https://sonarcloud.io/dashboard?id=HealthChecks.Extensions)

## Conditional checks

### Examples
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        // Check on Redis only when the connection string is set.
        .AddRedis(Configuration["Redis:ConnectionString"])
            .CheckOnlyWhen(Registrations.Redis, !string.IsNullOrEmpty(Configuration["Redis:ConnectionString"]))

        // Due to the fact the condition is expressed as a predicate, then this is evaluated with every request
        // thus in this example when the `ADependency:Setting` config is changed, then the result is re-evaluated
        // In the Redis example the condition is evaluated once, at startup.
        .AddCheck("A Dependency", () => HealthCheckResult.Healthy())
            .CheckOnlyWhen("A Dependency", () => !string.IsNullOrEmpty(Configuration["ADependency:Setting"]))

        // Check on RabbitMQ only a specific feature flag is set.
        // The condition is executed asynchronously with every health check request
        // and it has access to the ServiceProvider and the HealthCheckContext.
        .AddRabbitMQ()
            .CheckOnlyWhen(Registrations.RabbitMQ, async (sp, context, token) =>
                        {
                            var featureFlags = sp.GetService<IFeatureFlags>();
                            return await featureFlags.IsSet("AFeatureDependingOnRabbitMQ");
                        })

        // Specify a typed policy to enable executing a health check
        .AddCheck("Dependency1", () => HealthCheckResult.Healthy())
            .CheckOnlyWhen<FeatureFlagsPolicy>("Dependency1",
                conditionalHealthCheckPolicyArgs: "A Flag for a Feature that depends on Dependency1")

        // Specify a typed policy to check on multiple health checks
        .AddCheck("Dependency2", () => HealthCheckResult.Healthy())
        .AddCheck("Dependency3", () => HealthCheckResult.Healthy())
            .CheckOnlyWhen<FeatureFlagsPolicy>(new[] { "Dependency2", "Dependency3" },
                conditionalHealthCheckPolicyArgs: "A Flag for a Feature that depends on Dependency2 and Dependency3")

        // Customize health check responses when the health check registration is not evaluated
        // By default the HealthStatus is HealthStatus.Healthy 
        // A tag is also included in the conditional health check entry to mark the fact it was not checked
        // By default this value the tag name is NotChecked, so this is also customizable
        .AddCheck("CustomizedStatus", () => HealthCheckResult.Healthy())
            .CheckOnlyWhen("CustomizedStatus", conditionToRun: true, options: new ConditionalHealthCheckOptions
            {
                HealthStatusWhenNotChecked = HealthStatus.Degraded,
                NotCheckedTagName = "NotActive"
            })
        ;
}
```
