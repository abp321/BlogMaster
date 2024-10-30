using BlogMaster.Database;
using BlogMaster.Extensions;

namespace BlogMaster.Middleware
{
    public class CustomMiddleware(RequestDelegate next, BlogDbContext dbContext)
    {
        private const string redirectPath = "/redirects.html";
        private const string limitCheckApi = "/api/blogs/limited";

        public Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value ?? string.Empty;
            if (path == redirectPath || path == limitCheckApi) return next(context);
            string currentIp = context.GetClientIp().ToString();

            if (!path.Contains('.') && !path.Contains("api"))
            {
                Scripts.LogVisitor(currentIp, dbContext);
            }

            return next(context);
        }
    }
}
