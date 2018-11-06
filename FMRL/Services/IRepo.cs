using Microsoft.AspNetCore.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FMRL.Services
{
    public interface IRepo
    {
        Task Write(string path, object value);
        Task<T> Write<T>(string path, object value);
        Task<T> Read<T>(string path);
        Task Delete(string path);
    }

    public class FirebaseRepo : IRepo
    {
        private static readonly Uri BaseUrl = new Uri("https://keypearmx.firebaseio.com/fmrl/");

        private HttpClient _http;

        public FirebaseRepo(HttpClient http)
        {
            _http = http;
        }

        public async Task Write(string path, object value)
        {
            var url = new Uri(BaseUrl, path);
            await _http.PutJsonAsync(url.ToString(), value);
        }

        public async Task<T> Write<T>(string path, object value)
        {
            var url = new Uri(BaseUrl, path);
            return await _http.PutJsonAsync<T>(url.ToString(), value);
        }

        public async Task<T> Read<T>(string path)
        {
            var url = new Uri(BaseUrl, path);
            return await _http.GetJsonAsync<T>(url.ToString());
        }

        public async Task Delete(string path)
        {
            var url = new Uri(BaseUrl, path);
            await _http.DeleteAsync(url);
        }
    }
}
