namespace BlogHybrid.API.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private const string API_KEY_HEADER_NAME = "X-API-Key";

        public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip API key check for Swagger endpoints
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Check if API key exists in header
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "API Key is missing" });
                return;
            }

            // Validate API key
            var apiKey = _configuration["ApiSettings:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid API Key" });
                return;
            }

            await _next(context);
        }
    }

    // Extension method for easy registration
    public static class ApiKeyAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyAuthMiddleware>();
        }
    }
}