# HealthChecks.Extensions

A set of ASP.NET Health Checks extensions

[![Build status](https://ci.appveyor.com/api/projects/status/94f8gktyknvmmu6t/branch/main?svg=true)](https://ci.appveyor.com/project/adrianiftode/healthchecks-extensions/branch/main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=HealthChecks.Extensions&metric=alert_status)](https://sonarcloud.io/dashboard?id=HealthChecks.Extensions)
[![NuGet](https://img.shields.io/nuget/v/HealthChecks.Extensions.svg)](https://www.nuget.org/packages/HealthChecks.Extensions)

```ps
PM> Install-Package HealthChecks.Extensions
```
```ps
> dotnet add package HealthChecks.Extensions
```
```xml
<PackageReference Include="HealthChecks.Extensions" Version="1.0.0" />
```
## Conditional Health Checks

ASP.NET Core offers Health Checks Middleware, but is not always desirable to run them in every context.

For example, in environments like the development one, some dependencies might not be available at all, but others could be.

Other dependencies might be used later during the application lifetime, maybe when their configuration is finished, or when some feature flag is enabled.
`Conditional Health Checks` leverages the possibility to decide when a health check should be actually checked.

### Examples
```csharp
public void ConfigureServices(IServiceCollection services)
{
  services.AddHealthChecks()

    // Check on Redis only when the environment is other than development
    .AddRedis(Configuration["Redis:ConnectionString"])
        .CheckOnlyWhen(Registrations.Redis, !Environment.IsDevelopment())

    // Express the condition as a predicate.
    // Due to this, the condition is evaluated with every request
    //
    // In this example when the `ADependency:Setting` config is changed,
    // then the health check on `A Dependency` is re-evaluated as usual.
    //
    // In the Redis example such a condition is evaluated once, at startup.
    .AddCheck("A Dependency", () => HealthCheckResult.Healthy())
        .CheckOnlyWhen("A Dependency",
               () => !string.IsNullOrEmpty(Configuration["ADependency:Setting"]))

    // Check on RabbitMQ only a specific feature flag is set.
    // The condition is executed asynchronously with every health check request
    // and it has access to the ServiceProvider and the HealthCheckContext.
    .AddRabbitMQ()
        .CheckOnlyWhen(Registrations.RabbitMQ, 
                    async (sp, context, token) =>
                    {
                        var featureFlags = sp.GetService<IFeatureFlags>();
                        return await featureFlags.IsSet("AFeatureDependingOnRabbitMQ");
                    })

    // Specify a typed policy to enable executing a health check
    // This policy could have an exhaustive implementation and
    // reused with other dependencies.
    .AddCheck("Dependency1", () => HealthCheckResult.Healthy())
        .CheckOnlyWhen<FeatureFlagsPolicy>("Dependency1",
            conditionalHealthCheckPolicyCtorArgs: "A Flag for a Feature that depends on Dependency1")

    // Specify a typed policy to check on multiple health checks
    .AddCheck("Dependency2", () => HealthCheckResult.Healthy())
    .AddCheck("Dependency3", () => HealthCheckResult.Healthy())
        .CheckOnlyWhen<FeatureFlagsPolicy>(new[] { "Dependency2", "Dependency3" },
            conditionalHealthCheckPolicyCtorArgs: "A Flag for a Feature that depends on both dependencies.")

    // Customize health check responses when the health check registration is not evaluated
    // By default the HealthStatus is HealthStatus.Healthy 

    // A tag is also included in the conditional health check entry
    // to mark the fact it was not checked
    //
    // By default this value the tag name is `NotChecked`, so this is also customizable
    .AddCheck("CustomizedStatus", () => HealthCheckResult.Healthy())
        .CheckOnlyWhen("CustomizedStatus",
            conditionToRun: true,
            options: new ConditionalHealthCheckOptions
            {
                HealthStatusWhenNotChecked = HealthStatus.Degraded,
                NotCheckedTagName = "NotActive"
            });
}
```
