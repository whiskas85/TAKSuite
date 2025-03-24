using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace TAKSuite.Helpers
{
    public class JsInteropHelper
    {
        private readonly IJSRuntime _jsRuntime;

        public JsInteropHelper(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        public async Task OpenInNewTab(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore JSRuntime: {ex.Message}");
                }
            }
        }
    }
}
