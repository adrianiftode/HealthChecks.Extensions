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
    /// <summary>
    /// IHealthChecksBuilder extensions methods used to register health checks extensions
    /// </summary>
    public static class ConditionalHealthCheckRegistration
    {
        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by an asynchronous predicate which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The condition is expressed by a predicate which receives as input <see cref="IServiceProvider"/>, <see cref="HealthCheckContext"/> and a <see cref="CancellationToken"/>
        /// and it returns a <see cref="Task"/> representing the asynchronous operation returning the predicate result.
        /// </remarks>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="predicate">The predicate describing when to run the health check. If it returns true, then the health check is executed, otherwise is not.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder,
            string name,
            Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<bool>> predicate,
            ConditionalHealthCheckOptions? options = null)
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

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a immediately evaluated condition.
        /// </summary>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="conditionToRun">True if the health check should be evaluated, or false if not.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder,
            string name,
            bool conditionToRun,
            ConditionalHealthCheckOptions? options = null)
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(conditionToRun), options);

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a predicate which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        ///  The condition is expressed by a predicate that returns a boolean value based on which is decided the health check is executed or not.
        /// </remarks>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="predicate">The predicate describing when to run the health check. If it returns true, then the health check is executed, otherwise is not. </param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder,
            string name, Func<bool> predicate,
            ConditionalHealthCheckOptions? options = null)
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(predicate()), options);

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The health check policy instance is provided by a function that receives as input <see cref="IServiceProvider"/>, <see cref="HealthCheckContext"/> and a <see cref="CancellationToken"/>
        /// and it returns a <see cref="Task"/> representing the asynchronous operation that returns the health check policy instance.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="policyProvider">The function that returns the instance based on which the health check execution is decided to run on not.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        /// <exception cref="InvalidOperationException">When the policy provider returns a null instance.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string name,
            Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<T>> policyProvider,
            ConditionalHealthCheckOptions? options = null)
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

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The instance of this type is created with every request and its dependencies are resolved by <see cref="IServiceProvider"/>.
        /// If the constructor has more arguments then these can be passed via the <paramref name="conditionalHealthCheckPolicyCtorArgs"/> parameter.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="conditionalHealthCheckPolicyCtorArgs">Additional arguments required by the constructor of the T type implementing <see cref="IConditionalHealthCheckPolicy"/></param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string name,
            ConditionalHealthCheckOptions? options = null,
            params object[] conditionalHealthCheckPolicyCtorArgs)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (sp, _, __) => Task.FromResult(ActivatorUtilities.CreateInstance<T>(sp, conditionalHealthCheckPolicyCtorArgs)), options);

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which given instance's method will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The <see cref="IConditionalHealthCheckPolicy"/> instance is provided as an argument of this method.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="policy">The conditional health check policy instance.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string name,
            T policy,
            ConditionalHealthCheckOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(policy), options);

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which type's instance will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The <see cref="IConditionalHealthCheckPolicy"/> instance is provided by a function via the <paramref name="policyProvider"/> parameter.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="name">
        /// The name of the existing health check, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health check must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="policyProvider">The function that returns the instance based on which the health check execution is decided to run on not.
        /// </param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string name,
            Func<T> policyProvider,
            ConditionalHealthCheckOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(name, (_, __, ___) => Task.FromResult(policyProvider()), options);

        /// <summary>
        /// Enables multiple existing health checks to be evaluated only when a specific condition occurs,
        /// that is described by an asynchronous predicate which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The condition is expressed by a predicate which receives as input <see cref="IServiceProvider"/>, <see cref="HealthCheckContext"/> and a <see cref="CancellationToken"/>
        /// and it returns a <see cref="Task"/> representing the asynchronous operation returning the predicate result.
        /// </remarks>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="predicate"></param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder,
            string[] names,
            Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<bool>> predicate,
            ConditionalHealthCheckOptions? options = null)
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

        /// <summary>
        /// Enables multiple existing health checks to be evaluated only when a specific condition occurs,
        /// that is described by a immediately evaluated condition.
        /// </summary>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="conditionToRun">True if the health check should be evaluated, or false if not.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder,
            string[] names,
            bool conditionToRun,
            ConditionalHealthCheckOptions? options = null)
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(conditionToRun), options);

        /// <summary>
        /// Enables multiple existing health checks to be evaluated only when a specific condition occurs,
        /// that is described by predicate which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        ///  The condition is expressed by a predicate that returns a boolean value based on which is decided the health check is executed or not.
        /// </remarks>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="predicate">The predicate describing when to run the health check. If it returns true, then the health check is executed, otherwise is not. </param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen(this IHealthChecksBuilder builder,
            string[] names,
            Func<bool> predicate,
            ConditionalHealthCheckOptions? options = null)
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(predicate()), options);

        /// <summary>
        /// Enables multiple existing health checks to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The health check policy instance is provided by a function that receives as input <see cref="IServiceProvider"/>, <see cref="HealthCheckContext"/> and a <see cref="CancellationToken"/>
        /// and it returns a <see cref="Task"/> representing the asynchronous operation that returns the health check policy instance.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="policyProvider">The function that returns the instance based on which the health check execution is decided to run on not.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string[] names,
            Func<IServiceProvider, HealthCheckContext, CancellationToken, Task<T>> policyProvider,
            ConditionalHealthCheckOptions? options = null)
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

        /// <summary>
        /// Enables multiple existing health checks to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The instance of this type is created with every request and its dependencies are resolved by <see cref="IServiceProvider"/>.
        /// If the constructor has more arguments then these can be passed via the <paramref name="conditionalHealthCheckPolicyCtorArgs"/> parameter.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="conditionalHealthCheckPolicyCtorArgs"></param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string[] names,
            ConditionalHealthCheckOptions? options = null,
            params object[] conditionalHealthCheckPolicyCtorArgs)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(names, (sp, _, __) => Task.FromResult(ActivatorUtilities.CreateInstance<T>(sp, conditionalHealthCheckPolicyCtorArgs)), options);

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which type's instance will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The <see cref="IConditionalHealthCheckPolicy"/> instance is provided as an argument of this method.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="policy">The conditional health check policy instance.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string[] names,
            T policy,
            ConditionalHealthCheckOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(policy), options);

        /// <summary>
        /// Enables an existing health check to be evaluated only when a specific condition occurs,
        /// that is described by a type implementing <see cref="IConditionalHealthCheckPolicy"/>
        /// and which type's instance will be executed on every health check request.
        /// </summary>
        /// <remarks>
        /// The <see cref="IConditionalHealthCheckPolicy"/> instance is provided by a function via the <paramref name="policyProvider"/> parameter.
        /// </remarks>
        /// <typeparam name="T">The type implementing <see cref="IConditionalHealthCheckPolicy"/>.</typeparam>
        /// <param name="builder">The builder used to register health checks.</param>
        /// <param name="names">
        /// The list of names of the existing health checks, ie. "Redis", "redis", "S3", "My Health Check".
        /// The health checks must be previously registered using the other <see cref="IHealthChecksBuilder" /> extensions, like AddRedis, AddRabbitMQ, AddCheck, etc.
        /// You can use the static <see cref="Registrations"/> class that contains a list of well-known names,
        /// otherwise specify the name of the previously added health check.
        /// </param>
        /// <param name="policyProvider">The function that returns the instance based on which the health check execution is decided to run on not.</param>
        /// <param name="options">A list of options to override the default behavior. See <see cref="ConditionalHealthCheckOptions"/> for extra details.</param>
        /// <returns>The same builder used to register health checks.</returns>
        /// <exception cref="ArgumentException">When the name of the health check is not provided, ie. is null or is an empty string.</exception>
        /// <exception cref="InvalidOperationException">When the health check identified by the previously given name is not registered yet.</exception>
        public static IHealthChecksBuilder CheckOnlyWhen<T>(this IHealthChecksBuilder builder,
            string[] names,
            Func<T> policyProvider,
            ConditionalHealthCheckOptions? options = null)
            where T : IConditionalHealthCheckPolicy
            => builder.CheckOnlyWhen(names, (_, __, ___) => Task.FromResult(policyProvider()), options);
    }
}
