using System.Net;

namespace BlogMaster.Extensions
{
    public static class HttpContextExtensions
    {
        public static IPAddress GetClientIp(this HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress;
            if(clientIp is not null) return clientIp;

            if (IPAddress.TryParse(context.Request.Headers["X-Forwarded-For"].FirstOrDefault(), out IPAddress? parsedIp))
            {
                return parsedIp;
            }
            return IPAddress.None;
        }

        public static string GetHostBaseAddress(this HttpContext context)
        {
            var request = context.Request;
            return string.Concat(request.Scheme, "://", request.Host.Value, request.PathBase.Value);
        }

        public static string GetRequestUrl(this HttpContext context)
        {
            var request = context.Request;
            return string.Concat(request.Scheme, "://", request.Host.Value, request.Path.Value, request.QueryString.Value).TrimEnd('/');
        }

        public static ValueTask WriteJavascript(this HttpContext context, string customJs)
        {
            context.Response.ContentType = "text/html";
            string htmlContent = $"<script>{customJs}</script>";
            Memory<byte> contentBytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
            return context.Response.Body.WriteAsync(contentBytes);
        }
    }
}
