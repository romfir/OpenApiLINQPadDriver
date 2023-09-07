using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace OpenApiLINQPadDriver.Extensions;

public static class StringExtensions
{
    public static bool? ToBoolSafe(this string? s, NumberStyles style = NumberStyles.Integer | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    {
        var longValue = s.ToLongSafe(style, provider);

        return longValue.HasValue
            ? longValue.Value != 0
            : bool.TryParse(s, out var parsedValue)
                ? parsedValue
                : null;
    }

    public static long? ToLongSafe(this string? s, NumberStyles style = NumberStyles.Integer | NumberStyles.AllowThousands, IFormatProvider? provider = null) 
        => long.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

    public static readonly IFormatProvider DefaultFormatProvider = CultureInfo.InvariantCulture;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IFormatProvider ResolveFormatProvider(this IFormatProvider? provider) =>
        provider ?? DefaultFormatProvider;
}
