using Microsoft.Data.Sqlite;

namespace MdEmail.Templates.Data.Sqlite.Tests.Integration;

internal class TestEnvironment : IDisposable
{
    // Setup env var TESTSRAMDISKDIR to point to ram disk if present
    // if not temp dir will be used

    private static readonly string _testDir;

    static TestEnvironment()
    {
        string? dir = Environment.GetEnvironmentVariable("TESTSRAMDISKDIR");

        if (dir is null)
        {
            _testDir = Directory.CreateTempSubdirectory("md_email").FullName;
        }
        else
        {
            _testDir = Path.Combine(dir, "md_email");
            Directory.CreateDirectory(_testDir);
        }
    }

    private readonly string _filePath;

    public string ConnectionString { get; }

    public TestEnvironment()
    {
        _filePath = Path.Combine(_testDir, $"mde_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}_test.sqlite");
        ConnectionString = $"Data Source={_filePath}";
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            SqliteConnection.ClearAllPools();
            File.Delete(_filePath);
        }
    }
}