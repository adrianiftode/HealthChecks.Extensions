using AspNetCore.Diagnostics.HealthChecks.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConditionalHealthChecksExtensions
    {
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, Task<bool>> predicate)
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
                       context => predicate(sp, context),
                       sp.GetService<ILogger<ConditionalHealthCheck>>()
                   );
            });

            return builder;
        }

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, HealthCheckContext, bool> predicate)
            => builder.CheckOnlyWhen(name, (sp, context) => Task.FromResult(predicate(sp, context)));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<Task<bool>> predicate)
            => builder.CheckOnlyWhen(name, (_, __) => predicate());

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<bool> predicate)
           => builder.CheckOnlyWhen(name, (_, __) => Task.FromResult(predicate()));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, Task<bool>> predicate)
           => builder.CheckOnlyWhen(name, (sp, _) => predicate(sp));

        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<IServiceProvider, bool> predicate)
            => builder.CheckOnlyWhen(name, (sp, _) => Task.FromResult(predicate(sp)));

        // no overload for HC as it introduces ambiguity with IServiceProvider overloads, so it will introduce a burden degree on client side
        //public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<HealthCheckContext, Task<bool>> predicate)
        //  => builder.CheckOnlyWhen(name, (_, hc) => predicate(hc));

        //public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder, string name, Func<HealthCheckContext, bool> predicate)
        //   => builder.CheckOnlyWhen(name, (_, hc) => Task.FromResult(predicate(hc)));

        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder, string name)
            where T : IConditionalHealthCheckPolicy
           => builder.CheckOnlyWhen(name, async (sp, context) =>
           {
               var policy = sp.GetService<T>();

               if (policy == null)
               {
                   throw new InvalidOperationException($"A policy of type `{name}` is not found in the health registrations list, so its conditional check cannot be configured. The registration must be added before configuring the conditional predicate.");
               }

               return await policy.Evaluate(context);
           });
    }

    public interface IConditionalHealthCheckPolicy
    {
        Task<bool> Evaluate(HealthCheckContext context);
    }
}
