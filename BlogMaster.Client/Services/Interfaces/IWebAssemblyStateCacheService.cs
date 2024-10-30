namespace BlogMaster.Client.Services.Interfaces
{
    public interface IWebAssemblyStateCacheService
    {
        void SetItem<T>(string key, T item);
        bool TryGetItem<T>(string key, out T? item);
    }
}
