using System.Collections.Generic;
using System.Linq;

namespace OpenApiLINQPadDriver.Extensions;
internal static class EnumerableExtensions
{
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, bool shouldAppend, T element)
        => shouldAppend ? enumerable.Append(element) : enumerable;
}
