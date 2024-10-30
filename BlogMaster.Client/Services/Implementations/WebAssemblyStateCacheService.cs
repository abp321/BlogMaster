using BlogMaster.Client.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlogMaster.Client.Services.Implementations
{
    public class WebAssemblyStateCacheService(IMemoryCache cache) : IWebAssemblyStateCacheService
    {
        private readonly bool _isWebAssembly = OperatingSystem.IsBrowser();

        public void SetItem<T>(string key, T item)
        {
            if (_isWebAssembly)
            {
                cache.Set(key, item);
            }
        }

        public bool TryGetItem<T>(string key, out T? item)
        {
            if (_isWebAssembly && cache.TryGetValue(key, out item))
            {
                return true;
            }

            item = default;
            return false;
        }
    }
}
