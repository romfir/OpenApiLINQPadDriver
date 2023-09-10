using System;
using System.Threading.Tasks;
using NSwag;

namespace OpenApiLINQPadDriver;
internal static class OpenApiDocumentHelper
{
    public static Task<OpenApiDocument> GetFromUriAsync(Uri uri)
        => uri.Scheme == Uri.UriSchemeFile
            ? OpenApiDocument.FromFileAsync(uri.LocalPath)
            : OpenApiDocument.FromUrlAsync(uri.ToString());

    public static OpenApiDocument GetFromUri(Uri uri)
        => AsyncHelper.RunSync(() => GetFromUriAsync(uri));
}
