using HealthChecks.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConditionalHealthCheckRegistration
    {
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<bool>> predicate, ConditionalHealthOptions? options = null)
        {
            builder.Services.Configure<HealthCheckServiceOptions>(healthCheckOptions =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Health check registration name cannot be null or empty.", nameof(name));
                }

                var registration = healthCheckOptions.Registrations.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

                if (registration == null)
                {
                    throw new InvalidOperationException($"A health check registration named `{name}` was not found in the health registrations list, " +
                                                        "so its conditional check cannot be configured. " +
                                                        $"The registration must be added before configuring the conditional predicate, so `{nameof(CheckOnlyWhen)}` must be called after the AddHealthCheck methods. " +
                                                        $"The exiting registrations are: \n" +
                                                        $"{string.Join("\n", healthCheckOptions.Registrations.Select(c => "        `" + c.Name + "`"))}");
                }

                var factory = registration.Factory;
                registration.Factory = sp => new ConditionalHealthCheck(
                       () => factory(sp),
                       (context, token) => predicate(sp, context, token),
                       options,
                       sp.GetService<ILogger<ConditionalHealthCheck>>()
                   );
            });

            return builder;
        }

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, bool whenCondition, ConditionalHealthOptions? options = null)
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(whenCondition), options);

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<bool> predicate, ConditionalHealthOptions? options = null)
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(predicate()), options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<T>> policyProvider, ConditionalHealthOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, async (sp, context, token) =>
            {
                var policy = await policyProvider(sp, context, token);

                if (policy == null)
                {
                    throw new InvalidOperationException($"A policy of type `{name}` could not be retrieved.");
                }

                return await policy.Evaluate(context);
            }, options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, ConditionalHealthOptions? options = null, params object[] conditionalHealthCheckPolicyArgs)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (sp, _, __) => Task.FromResult(ActivatorUtilities.CreateInstance<T>(sp, conditionalHealthCheckPolicyArgs)), options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, T policy, ConditionalHealthOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(policy), options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<T> policyProvider, ConditionalHealthOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(policyProvider()), options);

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string[] names, Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<bool>> predicate, ConditionalHealthOptions? options = null)
        {
            if (names == null || names.Length == 0)
            {
                throw new ArgumentException(nameof(names));
            }

            foreach (var name in names)
            {
                builder.CheckOnlyWhen(name, predicate, options);
            }

            return builder;
        }

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string[] names, bool whenCondition, ConditionalHealthOptions? options = null)
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(whenCondition), options);

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string[] names, Func<bool> predicate, ConditionalHealthOptions? options = null)
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(predicate()), options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string[] names, Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<T>> policyProvider, ConditionalHealthOptions? options = null)
            where T : IConditionalHealthCheckPolicy
        {
            if (names == null || names.Length == 0)
            {
                throw new ArgumentException(nameof(names));
            }

            foreach (var name in names)
            {
                builder.CheckOnlyWhen(name, policyProvider, options);
            }

            return builder;
        }

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string[] names, ConditionalHealthOptions? options = null, params object[] conditionalHealthCheckPolicyArgs)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(names, (sp, _, __) => Task.FromResult(ActivatorUtilities.CreateInstance<T>(sp, conditionalHealthCheckPolicyArgs)), options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string[] names, T policy, ConditionalHealthOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(policy), options);

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string[] names, Func<T> policyProvider, ConditionalHealthOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(policyProvider()), options);
    }
}
