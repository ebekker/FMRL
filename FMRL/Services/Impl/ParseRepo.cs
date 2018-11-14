using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FMRL.Services.Impl
{
    public class ParseRepo : IRepo
    {
        public const string Server = "https://parseapi.back4app.com";
        public const string AppId = "GibBELjUuUonfRpueteo6WNUgSfWLiJfo68f3f4g";
        public const string JsKey = "zcViyU7EqJvNKfaEUolODA3CTXI1EKX2oyR4HOcc";
        public const string WinKey = "J00OiEqvq7KlvTxVZ8jJqxFwHrISHUichicnfSHl";

        private HttpClient _http;

        public ParseRepo(HttpClient http)
        {
            _http = http;
        }

        public HttpClient InitHttp()
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("X-Parse-Application-Id", AppId);
            // Win/.NET Key is not listed in the CORS allowed Headers
            //_http.DefaultRequestHeaders.Add("X-Parse-Windows-Key", WinKey);
            _http.DefaultRequestHeaders.Add("X-Parse-Javascript-Key", JsKey);
            return _http;
        }

        public async Task Write(string type, string name, object value)
        {
            InitHttp();
            switch (type)
            {
                case "messages":
                    var url = $"{Server}/classes/Message";
                    var map = Json.Deserialize<Dictionary<string, object>>(Json.Serialize(value));
                    map["keyHash"] = name;
                    map.Remove("created");
                    var json = Json.Serialize(map);
                    var resp = await InitHttp().PostJsonAsync<ParseResponse<object>>(url, map);
                    if (resp.Code != 0 || !string.IsNullOrEmpty(resp.Error))
                        throw new Exception($"Parse Write Failed: {resp.Code}:{resp.Error}");
                    return;

                default:
                    throw new Exception("unknown type");
            }
        }

        public Task<T> Write<T>(string type, string name, object value)
        {
            throw new NotImplementedException();
        }

        public async Task<T> Read<T>(string type, string name)
        {
            switch (type)
            {
                case "messages":
                    var url = $"{Server}/functions/get";
                    Console.WriteLine("Reading from: " + name);
                    var resp = await InitHttp().PostJsonAsync<ParseResponse<T>>(url, new { key_hash = name });
                    if (resp.Code != 0 || !string.IsNullOrEmpty(resp.Error))
                        throw new Exception($"Parse Call Failed: {resp.Code}:{resp.Error}");
                    return resp.Result;

                default:
                    throw new Exception("unknown type");
            }
        }

        public async Task Delete(string type, string name)
        {
            switch (type)
            {
                case "messages":
                    var url = $"{Server}/functions/del";
                    var resp = await InitHttp().PostJsonAsync<ParseResponse<object>>(url, new { key_hash = name });
                    if (resp.Code != 0 || !string.IsNullOrEmpty(resp.Error))
                        throw new Exception($"Parse Call Failed: {resp.Code}:{resp.Error}");
                    return;

                default:
                    throw new Exception("unknown type");
            }
        }

        public class ParseResponse<T>
        {
            public int Code { get; set; }

            public string Error { get; set; }

            public T Result { get; set; }

        }
    }
}
