namespace OpenApiLINQPadDriverTests;
public class AuthTests : BaseDriverApiTest
{
    [Theory] //todo test both global and local setter
    [MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    public async Task Setting_Headers_In_PrepareRequestFunction_For_MultipleClientsFromFirstTagAndOperationName(JsonLibrary jsonLibrary, ClassStyle classStyle)
    {
        MapGet("Header", "/replay", "Replay", static (HttpContext context) => context.Request.Headers);
        StartApi();

        await ExecuteScriptAsync(
            """
             var tokenValue = "fakeBearer";
             this.PrepareRequestFunction = (a, requestMessage, c) =>
             {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenValue);
             };

             var replayedHeaders = await this.HeaderClient.ReplayAsync();

             replayedHeaders.Should()
               .Contain(h => h.Key == "Authorization" && h.Value.Any(v => v == $"Bearer {tokenValue}"));
            // return 1;
            """, EndpointGrouping.MultipleClientsFromFirstTagAndOperationName, jsonLibrary, classStyle);
    }

    [Theory] //todo test both global and local setter
    [MemberData(nameof(EnumInlineData.Instance), MemberType = typeof(EnumInlineData))]
    public async Task Setting_Headers_In_PrepareRequestFunction_For_SingleClientFromOperationIdOperationName(JsonLibrary jsonLibrary, ClassStyle classStyle)
    {
        MapGet("Header", "/replay", "Replay", static (HttpContext context) => context.Request.Headers);
        StartApi();

        await ExecuteScriptAsync(
            """
             var tokenValue = "fakeBearer";
             this.PrepareRequestFunction = (a, requestMessage, c) =>
             {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenValue);
             };

             var replayedHeaders = await this.ReplayAsync();

             replayedHeaders.Should()
               .Contain(h => h.Key == "Authorization" && h.Value.Any(v => v == $"Bearer {tokenValue}"));

             //replayedHeaders.Dump();
             return replayedHeaders;
            """, EndpointGrouping.SingleClientFromOperationIdOperationName, jsonLibrary, classStyle);

    }
}
