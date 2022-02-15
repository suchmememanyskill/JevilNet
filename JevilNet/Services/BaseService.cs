using JevilNet.Services.Models;
using Newtonsoft.Json;

namespace JevilNet.Services;

public abstract class BaseService<T> where T: new()
{
    protected T storage = new();
    public abstract string StoragePath();
    
    public async Task Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StoragePath()));
        await File.WriteAllTextAsync(StoragePath(), JsonConvert.SerializeObject(storage));
    }

    public void Load()
    {
        if (File.Exists(StoragePath()))
        {
            storage = JsonConvert.DeserializeObject<T>(File.ReadAllText(StoragePath()))!;
        }
    }
}