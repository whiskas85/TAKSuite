using Microsoft.JSInterop;
using TAKSuite.Data.Models;
using TAKSuite.Data.ModelsTak;
public static class DocumentationHelper
{
    public static async Task OpenAttachment(Documentation attach, IJSRuntime JS)
    {
        var bytes = File.ReadAllBytes(attach.Path);
        if (bytes != null)
        {
            string base64String = Convert.ToBase64String(bytes);
            string title = attach.Name;  // Titolo per il PDF
            if (attach.Type == "image/jpeg" || attach.Type == "image/png")
            {
                string imageDataUrl = $"data:image/jpeg;base64,{base64String}";
                await JS.InvokeVoidAsync("openNewTab", imageDataUrl, title);
            }
            else if (attach.Type == "application/pdf")
            {
                // Crea un blob URL per il PDF
                var pdfDataUrl = $"data:application/pdf;base64,{base64String}";
                await JS.InvokeVoidAsync("downloadFile", pdfDataUrl, title);
            }
        }
    }



    public static async Task OpenAttachment(AtakAttachment attach, IJSRuntime JS)
    {
        var bytes = attach.FileBytes;
        if (bytes != null)
        {
            string base64String = Convert.ToBase64String(bytes);
            string title = attach.Name;  // Titolo per il PDF
            if (attach.MediaType == "image/jpeg" || attach.MediaType == "image/png")
            {
                string imageDataUrl = $"data:image/jpeg;base64,{base64String}";
                await JS.InvokeVoidAsync("openNewTab", imageDataUrl, title);
            }
            else if (attach.MediaType == "application/pdf")
            {
                // Crea un blob URL per il PDF
                var pdfDataUrl = $"data:application/pdf;base64,{base64String}";
                await JS.InvokeVoidAsync("downloadFile", pdfDataUrl, title);
            }
        }

    }
}

