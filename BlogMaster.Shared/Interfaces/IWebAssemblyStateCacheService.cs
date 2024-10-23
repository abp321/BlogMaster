namespace BlogMaster.Shared.Interfaces
{
    public interface IWebAssemblyStateCacheService<T>
    {
        void SetItem(string key, T item);
        bool TryGetItem(string key, out T? item);
    }
}
