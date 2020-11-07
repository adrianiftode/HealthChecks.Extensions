using AspNetCore.Diagnostics.HealthChecks.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConditionalHealthCheckRegistration
    {
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<bool>> predicate)
        {
            builder.Services.Configure<HealthCheckServiceOptions>(options =>
            {
                var registration = options.Registrations.FirstOrDefault(c => c.Name == name);

                if (registration == null)
                {
                    throw new InvalidOperationException($"A health check registration named `{name}` is not found in the health registrations list, so its conditional check cannot be configured. The registration must be added before configuring the conditional predicate.");
                }

                var factory = registration.Factory;
                registration.Factory = sp => new ConditionalHealthCheck(
                       () => factory(sp),
                       (context, token) => predicate(sp, context, token), null,
                       sp.GetService<ILogger<ConditionalHealthCheck>>()
                   );
            });

            return builder;
        }

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, bool whenCondition)
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(whenCondition));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<bool> predicate)
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(predicate()));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<CancellationToken, Task<bool>> predicate)
            => builder.CheckOnlyWhen(name, (_, __, token) => predicate(token));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, bool> predicate)
            => builder.CheckOnlyWhen(name, (sp, _, __) => Task.FromResult(predicate(sp)));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, CancellationToken, Task<bool>> predicate)
           => builder.CheckOnlyWhen(name, (sp, _, token) => predicate(sp, token));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, bool> predicate)
            => builder.CheckOnlyWhen(name, (sp, context, _) => Task.FromResult(predicate(sp, context)));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<T>> policyProvider)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, async (sp, context, token) =>
            {
                var policy = await policyProvider(sp, context, token);

                if (policy == null)
                {
                    throw new InvalidOperationException($"A policy of type `{name}` is not found in the health registrations list, so its conditional check cannot be configured. The registration must be added before configuring the conditional predicate.");
                }

                return await policy.Evaluate(context);
            });

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, params object[] args)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (sp, _, __) => Task.FromResult(ActivatorUtilities.CreateInstance<T>(sp, args)));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, T policy)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(policy));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<T> policyProvider)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(policyProvider()));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<CancellationToken, Task<T>> policyProvider)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, token) => policyProvider(token));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, T> policyProvider)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (sp, __, ___) => Task.FromResult(policyProvider(sp)));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, CancellationToken, Task<T>> policyProvider)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (sp, __, token) => policyProvider(sp, token));
    }

    public interface IConditionalHealthCheckPolicy
    {
        Task<bool> Evaluate(HealthCheckContext context);
    }
}
