using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

    public class HttpRequestWithStatusException : HttpRequestException
    {
        public HttpRequestWithStatusException(string message, Exception inner, HttpStatusCode code)
            : base(message, inner)
        {
            StatusCode = code;
        }

        public HttpStatusCode StatusCode { get; }
    }

    public class FirebaseRepo : IRepo
    {
        private static readonly Uri BaseUrl = new Uri("https://fmrl-a.firebaseio.com/");

        private HttpClient _http;

        public FirebaseRepo(HttpClient http)
        {
            _http = http;
        }

        public async Task Write(string path, object value)
        {
            var url = new Uri(BaseUrl, path);
            var requJson = Json.Serialize(value);
            var content = new StringContent(requJson, Encoding.UTF8, "application/json");
            var resp = await _http.PutAsync(url, content);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<T> Write<T>(string path, object value)
        {
            var url = new Uri(BaseUrl, path);
            var requJson = Json.Serialize(value);
            var content = new StringContent(requJson, Encoding.UTF8, "application/json");
            var resp = await _http.PutAsync(url, content);
            resp.EnsureSuccessStatusCode();

            var respJson = await resp.Content.ReadAsStringAsync();
            return Json.Deserialize<T>(respJson);
        }

        public async Task<T> Read<T>(string path)
        {
            var url = new Uri(BaseUrl, path);
            var resp = await _http.GetAsync(url);
            try
            {
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new HttpRequestWithStatusException(ex.Message, ex.InnerException,
                    resp.StatusCode);
            }
            return await _http.GetJsonAsync<T>(url.ToString());
        }

        public async Task Delete(string path)
        {
            var url = new Uri(BaseUrl, path);
            await _http.DeleteAsync(url);
        }
    }
}
