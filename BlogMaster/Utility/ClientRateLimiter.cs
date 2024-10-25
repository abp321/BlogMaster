using BlogMaster.Extensions;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.RateLimiting;

namespace BlogMaster.Utility
{
    public class ClientRateLimiter(int requestLimitPerMinute)
    {
        private static readonly object s_lock = new();
        private static readonly HashSet<string> s_localAddresses = new(GetLocalAddresses());
        private static readonly ConcurrentDictionary<string, FixedWindowRateLimiter> s_limiters = [];

        private static IEnumerable<string> GetLocalAddresses()
        {
            yield return "127.0.0.1";
            yield return "::1";
            foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
            {
                yield return address.ToString();
            }
        }

        private FixedWindowRateLimiter CreateRateLimiter()
        {
            return new(new FixedWindowRateLimiterOptions
            {
                PermitLimit = requestLimitPerMinute,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        }

        public bool IsLimitReached(string ipAddress)
        {
            if (s_localAddresses.Contains(ipAddress)) return false;

            EnforceLimiterCapacity();
            var rateLimiter = s_limiters.GetOrAdd(ipAddress, _ => CreateRateLimiter());
            var lease = rateLimiter.AttemptAcquire(1);
            return !lease.IsAcquired;
        }

        public static bool IsLimitReached(HttpContext context)
        {
            string ipAddressValue = context.GetClientIp().ToString();
            if(s_limiters.TryGetValue(ipAddressValue, out var rateLimiter))
            {
                return !rateLimiter.AttemptAcquire(1).IsAcquired;
            }
            return false;
        }

        private static void EnforceLimiterCapacity()
        {
            if (s_limiters.Count < 100) return;

            lock(s_lock)
            {
                var firstKey = s_limiters.Keys.FirstOrDefault();
                if (firstKey is not null)
                {
                    if (s_limiters.TryRemove(firstKey, out FixedWindowRateLimiter? limiter))
                    {
                        limiter?.Dispose();
                    }
                }
            }
        }
    }
}
