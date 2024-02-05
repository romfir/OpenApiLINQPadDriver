namespace OpenApiLINQPadDriverTests.Utils;
internal class EnumInlineData : TheoryData<JsonLibrary, ClassStyle>
{
    public static EnumInlineData Instance = new();

    private EnumInlineData()
    {
        foreach (var jsonLibrary in Enum.GetValues<JsonLibrary>())
        {
            foreach (var classStyle in Enum.GetValues<ClassStyle>())
            {
               Add(jsonLibrary, classStyle);
            }
        }
    }
}
