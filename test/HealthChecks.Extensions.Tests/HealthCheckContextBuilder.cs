using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Extensions.Tests
{
    public class HealthCheckContextBuilder
    {
        private IHealthCheck _instance;
        private string _name = "TheCheck";
        private HealthStatus? _healthStatus = HealthStatus.Healthy;
        private List<string> _tags = new List<string>();

        public HealthCheckContextBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public HealthCheckContextBuilder WithInstance(IHealthCheck instance)
        {
            _instance = instance;
            return this;
        }

        public HealthCheckContextBuilder WithHealthStatus(HealthStatus? healthStatus)
        {
            _healthStatus = healthStatus;
            return this;
        }

        public HealthCheckContextBuilder WithTag(string tag)
        {
            (_tags ?? new List<string>()).Add(tag);
            return this;
        }

        public HealthCheckContextBuilder WithTags(List<string> tags)
        {
            _tags = tags;
            return this;
        }

        public HealthCheckContext Build() =>
            new HealthCheckContext
            {
                Registration =
                    new HealthCheckRegistration(_name, _instance, _healthStatus, _tags)
            };
    }
}