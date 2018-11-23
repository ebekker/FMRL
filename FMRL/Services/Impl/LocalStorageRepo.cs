using Blazor.Extensions.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMRL.Services.Impl
{
    /// <summary>
    /// A repo implementation that stores all data in the browser's localStorage,
    /// useful for testing or offline development.
    /// </summary>
    public class LocalStorageRepo : IRepo
    {
        LocalStorage _storage;

        public LocalStorageRepo(LocalStorage storage)
        {
            _storage = storage;
        }

        public async Task Write(string type, string name, object value)
        {
            var path = $"{type}/{name}";
            await _storage.SetItem(path, value);
        }

        public async Task<T> Write<T>(string type, string name, object value)
        {
            var path = $"{type}/{name}";
            await _storage.SetItem(path, value);
            return await _storage.GetItem<T>(path);
        }

        public async Task<T> Read<T>(string type, string name)
        {
            var path = $"{type}/{name}";
            return await _storage.GetItem<T>(path);
        }

        public async Task Delete(string type, string name)
        {
            var path = $"{type}/{name}";
            await _storage.RemoveItem(path);
        }
    }
}
