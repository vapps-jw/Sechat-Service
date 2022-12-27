namespace Sechat.Service.Middleware
{
    public static class CustomResponseHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomResponseHeaders(this IApplicationBuilder builder) => builder.UseMiddleware<CustomResponseHeadersMiddleware>();
    }

    public class CustomResponseHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomResponseHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Add("API-RES", "vapps-server-response");
            context.Response.Headers.Add("X-Developed-By", "JW");
            await _next(context);
        }
    }
}
