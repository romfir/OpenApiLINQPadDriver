using System.Collections.Generic;
using NSwag;

namespace OpenApiLINQPadDriver.Extensions;

internal static class OpenApiDocumentExtensions
{
    public static void SetServer(this OpenApiDocument document, string url)
        => document.SetProperty(d => d.Servers, new List<OpenApiServer>
        {
            new()
            {
                Description = "Auto generated server",
                Url = url,
            }
        });
}
