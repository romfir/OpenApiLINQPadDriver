using System;
using System.Threading.Tasks;
using NSwag;
using OpenApiLINQPadDriver.Enums;

namespace OpenApiLINQPadDriver;
internal static class OpenApiDocumentHelper
{
    public static Task<OpenApiDocument> GetFromUriAsync(Uri uri, OpenApiFormat openApiFormat)
    {
        var isLocalFile = uri.Scheme == Uri.UriSchemeFile;
        return openApiFormat switch
        {
            OpenApiFormat.Json => isLocalFile
                ? OpenApiDocument.FromFileAsync(uri.LocalPath)
                : OpenApiDocument.FromUrlAsync(uri.ToString()),
            OpenApiFormat.Yaml => isLocalFile
                ? OpenApiYamlDocument.FromFileAsync(uri.LocalPath)
                : OpenApiYamlDocument.FromUrlAsync(uri.ToString()),
            _ => throw new InvalidOperationException()
        };
    }

    public static OpenApiDocument GetFromUri(Uri uri, OpenApiFormat openApiFormat)
        => AsyncHelper.RunSync(() => GetFromUriAsync(uri, openApiFormat));
}
