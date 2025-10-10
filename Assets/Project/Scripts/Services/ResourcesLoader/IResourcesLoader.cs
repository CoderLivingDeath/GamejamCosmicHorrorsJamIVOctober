public interface IResourcesLoader
{
    object Load(string path);
    T LoadAsJson<T>(string path) where T : class;
}
