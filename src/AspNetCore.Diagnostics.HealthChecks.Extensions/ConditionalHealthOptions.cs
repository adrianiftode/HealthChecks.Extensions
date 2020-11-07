using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions
{
    public class ConditionalHealthOptions
    {
        private const string DefaultNotCheckedTagName = "NotChecked";
        private const HealthStatus DefaultHealthStatusWhenNotChecked = HealthStatus.Healthy;

        public string NotCheckedTagName { get; set; } = DefaultNotCheckedTagName;
        public HealthStatus HealthStatusWhenNotChecked { get; set; } = DefaultHealthStatusWhenNotChecked;

        internal static ConditionalHealthOptions DefaultFrom(ConditionalHealthOptions? options) => new ConditionalHealthOptions
        {
            NotCheckedTagName = options?.NotCheckedTagName ?? DefaultNotCheckedTagName,
            HealthStatusWhenNotChecked = options?.HealthStatusWhenNotChecked ?? DefaultHealthStatusWhenNotChecked
        };
    }
}
