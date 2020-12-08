using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing.Middleware
{
    public class JaegerHttpMiddleware
    {
        private readonly RequestDelegate _next;

        public JaegerHttpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IOpenTracingContext openTracingContext)
        {
            var tracer = openTracingContext.Tracer;
            OpenTracing.IScope scope = null;
            var span = tracer.ActiveSpan;
            var method = context.Request.Method;
            if (span is null)
            {
                var spanBuilder = tracer.BuildSpan($"HTTP {method}");
                scope = spanBuilder.StartActive(true);
                span = scope.Span;
            }

            span.Log($"Processing HTTP {method}: {context.Request.Path}");
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                span.SetTag(OpenTracing.Tag.Tags.Error, true);
                span.Log(ex.Message);
                throw;
            }
            finally
            {
                scope?.Dispose();
            }
        }
    }
}
