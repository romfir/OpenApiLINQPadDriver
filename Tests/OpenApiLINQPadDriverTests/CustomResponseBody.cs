namespace OpenApiLINQPadDriverTests;
public class CustomResponseBody : BaseDriverApiTest
{
    [Theory]
    [MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    public async Task MultipleClientsFromFirstTagAndOperationName(JsonLibrary jsonLibrary, ClassStyle classStyle)
    {
        MapGet("CustomResponseAndRequest", "/First", "first", static ([FromBody] CustomRequest request) => new CustomResponse(request.Foo, request.Bar));
        StartApi();

        var requestConstructor = GetRequestCtor(classStyle);
        var responseConstructor = GetResponseCtor(classStyle);

        await ExecuteScriptAsync(
            $"""
            {nameof(CustomRequest)} request = {requestConstructor};
            
            var response = await this.CustomResponseAndRequestClient.FirstAsync(request);
            response.Should().BeEquivalentTo({responseConstructor});
            
            """, EndpointGrouping.MultipleClientsFromFirstTagAndOperationName, jsonLibrary, classStyle);
    }

    [Theory]
    [MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    public async Task SingleClientFromOperationIdOperationName(JsonLibrary jsonLibrary, ClassStyle classStyle)
    {
        MapGet("CustomResponseAndRequest", "/First", "first", static ([FromBody] CustomRequest request) => new CustomResponse(request.Foo, request.Bar));
        StartApi();

        var requestConstructor = GetRequestCtor(classStyle);
        var responseConstructor = GetResponseCtor(classStyle);

        await ExecuteScriptAsync(
            $"""
             {nameof(CustomRequest)} request = {requestConstructor};

             var response = await this.FirstAsync(request);
             response.Should().BeEquivalentTo({responseConstructor});

             """, EndpointGrouping.SingleClientFromOperationIdOperationName, jsonLibrary, classStyle);
    }

    private record CustomResponse(string Foo, int Bar);
    private record CustomRequest(string Foo, int Bar);

    private static string GetResponseCtor(ClassStyle classStyle) => classStyle switch
    {
        ClassStyle.Poco or ClassStyle.Inpc or ClassStyle.Prism =>
            """
            new CustomResponse()
            {
                Foo = "string",
                Bar = 1
            }
            """,
        ClassStyle.Record => "new CustomResponse(1, \"string\")",
        _ => throw new InvalidOperationException()
    };

    private static string GetRequestCtor(ClassStyle classStyle) => classStyle switch
    {
        ClassStyle.Poco or ClassStyle.Inpc or ClassStyle.Prism =>
            """
            new CustomRequest()
            {
                Foo = "string",
                Bar = 1
            }
            """,
        ClassStyle.Record => "new CustomRequest(1, \"string\")",
        _ => throw new InvalidOperationException()
    };
}
