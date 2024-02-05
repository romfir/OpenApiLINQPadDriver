namespace OpenApiLINQPadDriverTests;

public class SimpleTests : BaseDriverApiTest
{
    [Theory] //todo test both global and local setter
    [MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    public async Task MultipleClientsFromFirstTagAndOperationName(JsonLibrary jsonLibrary, ClassStyle classStyle)
    {
        MapGet("arithmetic", "/sum", "sum", static (int x, int y) => x + y);
        MapGet("arithmetic", "/divide", "divide", static (int dividend, int divisor) => dividend / divisor);
        StartApi();

        await ExecuteScriptAsync(
            """
            var sum = await this.ArithmeticClient.SumAsync(x: 1, y: 2);
            sum.Should().Be(3, Reason());

            var asyncAction = () => this.ArithmeticClient.DivideAsync(dividend: 1, divisor: 0);

            await asyncAction.Should().ThrowAsync<ApiException>().WithMessage("*The HTTP status code of the response was not expected (500)*");
            
            //this.Dump();
            """, EndpointGrouping.MultipleClientsFromFirstTagAndOperationName, jsonLibrary, classStyle);
    }

    [Theory] //todo test both global and local setter
    [MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    public async Task SingleClientFromOperationIdOperationName(JsonLibrary jsonLibrary, ClassStyle classStyle)
    {
        MapGet("arithmetic", "/sum", "sum", static (int x, int y) => x + y);
        MapGet("arithmetic", "/divide", "divide", static (int dividend, int divisor) => dividend / divisor);
        StartApi();

        await ExecuteScriptAsync(
            """
            var sum = await this.SumAsync(x: 1, y: 2);
            sum.Should().Be(3, Reason());

            var division = await this.DivideAsync(dividend: 10, divisor: 5);
            division.Should().Be(2, Reason());
            """, EndpointGrouping.SingleClientFromOperationIdOperationName, jsonLibrary, classStyle);
    }

    //[Theory] //todo test both global and local setter
    //[MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    //public async Task ClientIsGeneratedWithoutServerInDefinition(JsonLibrary jsonLibrary, ClassStyle classStyle)
    //{
    //    MapGet("arithmetic", "/sum", "sum", static (int x, int y) => x + y);
    //    StartApi(removeServerFromApiDefinition: true);

    //    await ExecuteScriptAsync(
    //        """
    //        var sum = await this.SumAsync(x: 1, y: 2);
    //        sum.Should().Be(3, Reason());

    //        """, EndpointGrouping.SingleClientFromOperationIdOperationName, jsonLibrary, classStyle);
    //}



    //todo some form of physical cache based on swagger.json hash? Write to temp and read it or something

    //todo auth tests

    //todo defaults props are set (for both modes)

    //todo test with a lot of endpoints/clients

    //todo yml tests

    //https://github.com/romfir/OpenApiLINQPadDriver/issues/25
    //todo weird client names with dots/dashes/etc. (#25)
}
