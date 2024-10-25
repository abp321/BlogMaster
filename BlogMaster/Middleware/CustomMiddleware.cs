using BlogMaster.Extensions;
using BlogMaster.Utility;

namespace BlogMaster.Middleware
{
    public class CustomMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        private const string redirectPath = "/redirects.html";
        private const string limitCheckApi = "/api/blogs/limited";
        private static readonly ClientRateLimiter rateLimiter = new(15);
        private static readonly SimpleTaskSceduler s_scheduler = new();

        public Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value ?? string.Empty;
            if (path == redirectPath || path == limitCheckApi) return next(context);
            string currentIp = context.GetClientIp().ToString();

            if (!path.Contains('.') && !path.Contains("api"))
            {
                s_scheduler.Schedule(() => Scripts.LogVisitor(currentIp, serviceProvider));
            }

            if (rateLimiter.IsLimitReached(currentIp))
            {
                context.Response.Redirect(redirectPath);
                return Task.Delay(1000);
            }

            return next(context);
        }
    }
}
