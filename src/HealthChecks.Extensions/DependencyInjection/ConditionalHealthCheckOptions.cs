using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Options to override the default behavior of the conditional health checks.
    /// </summary>
    public class ConditionalHealthCheckOptions
    {
        private const HealthStatus DefaultHealthStatusWhenNotChecked = HealthStatus.Healthy;

        /// <summary>
        /// 
        /// </summary>
        public static readonly string DefaultNotCheckedTagName = "NotChecked";

        /// <summary>
        /// When a health check is not evaluated, then the health check tags list will be augmented with this tag name.
        /// The default value is "NotChecked" <see cref="DefaultNotCheckedTagName"/>
        /// </summary>
        public string NotCheckedTagName { get; set; } = DefaultNotCheckedTagName;

        /// <summary>
        /// When a health check is not evaluated, then the health check result remains Healthy.
        /// Use this to return the desired status. 
        /// </summary>
        public HealthStatus HealthStatusWhenNotChecked { get; set; } = DefaultHealthStatusWhenNotChecked;

        internal static ConditionalHealthCheckOptions DefaultFrom(ConditionalHealthCheckOptions? options) => new ConditionalHealthCheckOptions
        {
            NotCheckedTagName = options?.NotCheckedTagName ?? DefaultNotCheckedTagName,
            HealthStatusWhenNotChecked = options?.HealthStatusWhenNotChecked ?? DefaultHealthStatusWhenNotChecked
        };
    }
}
