////using FluentAssertions;
////using LPRun;

////namespace OpenApiLINQPadDriverTests.Utils;
////internal static class RunnerResultExtensions
////{
////    public static async Task<Runner.Result> ShouldSucceedAsync(this Task<Runner.Result> resultAsync)
////    {
////        var result = await resultAsync;
////        result.ExitCode.Should().Be(0);
////        result.Error.Should().BeEmpty();

////        return result;
////    }
////}

//using FluentAssertions;
//using System.Text;

//namespace OpenApiLINQPadDriverTests.Utils;
//internal static class RunnerResultExtensions
//{
//    public static async Task<string> ShouldSucceedAsync(this Task<string> resultAsync)
//    {
//        var result = await resultAsync;
//        result.ExitCode.Should().Be(0);
//        result.Error.Should().BeEmpty();

//        return result;
//    }
//}

