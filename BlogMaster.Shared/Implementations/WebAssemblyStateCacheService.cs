using System.Collections.Concurrent;
using BlogMaster.Shared.Interfaces;

namespace BlogMaster.Shared.Implementations
{
    public class WebAssemblyStateCacheService<T> : IWebAssemblyStateCacheService<T>
    {
        private static readonly ConcurrentDictionary<string, T> _webAssemblyCache = new();
        private readonly bool _isWebAssembly;

        public WebAssemblyStateCacheService()
        {
            _isWebAssembly = OperatingSystem.IsBrowser();
        }

        public void SetItem(string key, T item)
        {
            if (_isWebAssembly)
            {
                _webAssemblyCache[key] = item!;
            }
        }

        public bool TryGetItem(string key, out T? item)
        {
            if (_isWebAssembly && _webAssemblyCache.TryGetValue(key, out item))
            {
                return true;
            }

            item = default;
            return false;
        }
    }
}
