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
        Task Write(string type, string name, object value);
        Task<T> Write<T>(string type, string name, object value);
        Task<T> Read<T>(string type, string name);
        Task Delete(string type, string name);
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
}
