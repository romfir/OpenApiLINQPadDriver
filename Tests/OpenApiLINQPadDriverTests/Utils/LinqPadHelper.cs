namespace OpenApiLINQPadDriverTests.Utils;
internal static class LinqPadHelper
{
    private static readonly string LocalLinqPadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LINQPad");
    public static void ThrowIfDriverExists(string driverName)
    {
        var nugetDriverPath = Path.Combine(LocalLinqPadPath, "NuGet.Drivers", driverName);
        var folderDriverPath = Path.Combine(LocalLinqPadPath, "Drivers", "DataContext", "NetCore", driverName);

        if (Directory.Exists(nugetDriverPath))
            throw new InvalidOperationException($"Driver already exists inside \"{nugetDriverPath}\", please remove it via LINQPad");

        if (Directory.Exists(folderDriverPath))
            throw new InvalidOperationException($"Driver already exists inside \"{folderDriverPath}\", please remove it via LINQPad");
    }
}
