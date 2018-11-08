using Microsoft.JSInterop;

namespace BlazorX.WebCrypto
{
    public static class RandomSource
    {
        public static byte[] GetRandomValues(int length)
        {
            return ((IJSInProcessRuntime)JSRuntime.Current).Invoke<byte[]>(
                "_blazorWebCrypto.getRandomValues", length);
        }
    }
}
