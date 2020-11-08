using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public interface IConditionalHealthCheckPolicy
    {
        Task<bool> Evaluate(HealthCheckContext context);
    }
}