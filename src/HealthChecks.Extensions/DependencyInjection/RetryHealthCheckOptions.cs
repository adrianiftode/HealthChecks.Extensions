using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Options to override the default behavior of the retry health checks.
    /// </summary>
    public class RetryHealthCheckOptions
    {
        public static readonly string DefaultRetriedTagName = "Retried";

        /// <summary>
        /// When a health check is not evaluated, then the health check tags list will be augmented with this tag name.
        /// The default value is "Retried" <see cref="DefaultRetriedTagName"/>
        /// </summary>
        public string RetriedTagName { get; set; } = DefaultRetriedTagName;

        internal static RetryHealthCheckOptions DefaultFrom(RetryHealthCheckOptions? options) => new RetryHealthCheckOptions
        {
            RetriedTagName = options?.RetriedTagName ?? DefaultRetriedTagName
        };
    }
}
