namespace OpenApiLINQPadDriverTests.Utils;
internal static class EnumInlineData
{
    public static IEnumerable<object[]> Data
    {
        get
        {
            foreach (var jsonLibrary in Enum.GetValues<JsonLibrary>())
            {
                foreach (var classStyle in Enum.GetValues<ClassStyle>())
                {
                    yield return new object[] { jsonLibrary, classStyle };
                }
            }
        }
    }
}
