using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions
{
    public class ConditionalHealthOptions
    {
        private const string DefaultNotCheckedTagName = "NotChecked";
        private const HealthStatus DefaultHealthStatus = HealthStatus.Healthy;

        public string NotCheckedTagName { get; set; } = DefaultNotCheckedTagName;
        public HealthStatus HealthStatus { get; set; } = DefaultHealthStatus;

        internal static ConditionalHealthOptions DefaultFrom(ConditionalHealthOptions? options) => new ConditionalHealthOptions
        {
            NotCheckedTagName = options?.NotCheckedTagName ?? DefaultNotCheckedTagName,
            HealthStatus = options?.HealthStatus ?? DefaultHealthStatus
        };
    }
}
