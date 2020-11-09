# AspNetCore.Diagnostics.HealthChecks.Extensions

A set of ASP.NET Health checks extensions for AspNetCore.Diagnostics.HealthChecks

## Conditional checks

### Examples
```csharp
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
            .CheckOnlyWhen<FeatureFlagsPolicy>("Dependency1", 
                conditionalHealthCheckPolicyArgs: "A Flag for a Feature that depends on Dependency1")

        // Specify a typed policy to check on multiple health checks
        .AddCheck("Dependency2", () => HealthCheckResult.Healthy())
        .AddCheck("Dependency3", () => HealthCheckResult.Healthy())
            .CheckOnlyWhen<FeatureFlagsPolicy>(new [] { "Dependency2", "Dependency3" }, 
                conditionalHealthCheckPolicyArgs: "A Flag for a Feature that depends on Dependency2 and Dependency3")

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
}
```
