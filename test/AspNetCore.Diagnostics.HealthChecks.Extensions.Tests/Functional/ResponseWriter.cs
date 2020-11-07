using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions.Tests.Functional
{
    internal static class ResponseWriter
    {
        internal static async Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };

            await using var stream = new MemoryStream();

            var healthResponse = new
            {
                status = result.Status.ToString(),
                totalDuration = result.TotalDuration.ToString(),
                entries = result.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    tags = e.Value.Tags,
                    description = e.Value.Description,
                    data = e.Value.Data?.Count > 0 ? e.Value.Data : null,
                    exception = ExtractSerializableExceptionData(e.Value.Exception)
                }).ToList()
            };

            await JsonSerializer.SerializeAsync(stream, healthResponse, healthResponse.GetType(), options);
            var json = Encoding.UTF8.GetString(stream.ToArray());

            await context.Response.WriteAsync(json);

            static object ExtractSerializableExceptionData(Exception exception)
            {
                if (exception == null)
                {
                    return null;
                }

                return new
                {
                    type = exception.GetType().ToString(),
                    message = exception.Message,
                    stackTrace = exception.StackTrace,
                    source = exception.Source,
                    data = exception.Data?.Count > 0 ? exception.Data : null,
                    innerException = exception.InnerException != null ? ExtractSerializableExceptionData(exception.InnerException) : null
                };
            };
        }
    }
}
