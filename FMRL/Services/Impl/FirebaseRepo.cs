using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FMRL.Services.Impl
{
    public class FirebaseRepo : IRepo
    {
        private static readonly Uri BaseUrl = new Uri("https://fmrl-a.firebaseio.com/");

        private HttpClient _http;

        public FirebaseRepo(HttpClient http)
        {
            _http = http;
        }

        public async Task Write(string type, string name, object value)
        {
            var path = $"/{type}/{name}.json";
            var url = new Uri(BaseUrl, path);
            var requJson = Json.Serialize(value);
            var content = new StringContent(requJson, Encoding.UTF8, "application/json");
            var resp = await _http.PutAsync(url, content);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<T> Write<T>(string type, string name, object value)
        {
            var path = $"/{type}/{name}.json";
            var url = new Uri(BaseUrl, path);
            var requJson = Json.Serialize(value);
            var content = new StringContent(requJson, Encoding.UTF8, "application/json");
            var resp = await _http.PutAsync(url, content);
            resp.EnsureSuccessStatusCode();

            var respJson = await resp.Content.ReadAsStringAsync();
            return Json.Deserialize<T>(respJson);
        }

        public async Task<T> Read<T>(string type, string name)
        {
            var path = $"/{type}/{name}.json";
            var url = new Uri(BaseUrl, path);
            var resp = await _http.GetAsync(url);
            try
            {
                resp.EnsureSuccessStatusCode();
                return Json.Deserialize<T>(await resp.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new HttpRequestWithStatusException(ex.Message, ex.InnerException,
                    resp.StatusCode);
            }
        }

        public async Task Delete(string type, string name)
        {
            var path = $"/{type}/{name}.json";
            var url = new Uri(BaseUrl, path);
            await _http.DeleteAsync(url);
        }
    }
}
