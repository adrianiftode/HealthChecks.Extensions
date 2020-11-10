using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Describes conditional health check policies that will answer to the question if a health check should be evaluated or not.
    /// </summary>
    public interface IConditionalHealthCheckPolicy
    {
        /// <summary>
        /// Decides if a health check should be evaluated or not.
        /// </summary>
        /// <param name="context">The context of the subjected health check.</param>
        /// <returns>An asynchronous operation that can return the evaluation result as a boolean value.</returns>
        Task<bool> Evaluate(HealthCheckContext context);
    }
}