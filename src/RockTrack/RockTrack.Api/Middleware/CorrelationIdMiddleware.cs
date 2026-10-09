using Serilog.Context;

namespace RockTrack.Api.Middleware
{
    // Gives every request a correlation ID: reuses a valid X-Correlation-Id header, otherwise generates one.
    // The ID is echoed in the response header and added to every log entry written during the request.
    public sealed class CorrelationIdMiddleware(RequestDelegate next)
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string ItemKey = "CorrelationId";
        private const int MaxLength = 64;

        public async Task InvokeAsync(HttpContext context)
        {
            var incoming = context.Request.Headers[HeaderName].ToString();
            var correlationId = IsValid(incoming) ? incoming : Guid.NewGuid().ToString("N");

            context.Items[ItemKey] = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next(context);
            }
        }

        // Letters, digits, '-', '_' and '.' only, at most 64 characters.
        // Invalid values are replaced, not rejected: stops log injection and oversized values without failing the request.
        private static bool IsValid(string value) =>
            value.Length is > 0 and <= MaxLength &&
            value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
    }
}
