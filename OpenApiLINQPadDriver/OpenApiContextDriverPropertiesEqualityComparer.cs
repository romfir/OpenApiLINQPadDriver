using System.Collections.Generic;

namespace OpenApiLINQPadDriver;
internal sealed class OpenApiContextDriverPropertiesEqualityComparer : IEqualityComparer<OpenApiContextDriverProperties>
{
    public static readonly OpenApiContextDriverPropertiesEqualityComparer Default = new();
    
    public bool Equals(OpenApiContextDriverProperties? x, OpenApiContextDriverProperties? y)
    {
        if (ReferenceEquals(x, y)) 
            return true;

        if (x is null || y is null) 
            return false;

        return x.OpenApiDocumentUri == y.OpenApiDocumentUri
               && x.ApiUri == y.ApiUri
               && x.EndpointGrouping == y.EndpointGrouping
               && x.JsonLibrary == y.JsonLibrary
               && x.ClassStyle == y.ClassStyle
               && x.GenerateSyncMethods == y.GenerateSyncMethods;
    }

    public int GetHashCode(OpenApiContextDriverProperties obj)
        => obj.GetHashCode();
}
