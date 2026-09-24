using Microsoft.JSInterop;

namespace CarMaintenanceDiary.Client.Common;

public static class JSRuntimeExtensions
{
    public static ValueTask OpenInNewTab(this IJSRuntime js, string url)
        => js.InvokeVoidAsync("open", url, "_blank");
}