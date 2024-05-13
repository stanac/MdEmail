using Dapper;
using Npgsql;

namespace MdEmail.Templates.Data.Postgres.Tests.Integration;

internal class TestEnvironment : IDisposable
{
    // Set env vars PG_MASTER_DB and PG_TEST_DB_TEMPLATE
    // Examples:
    // - PG_MASTER_DB: Server=127.0.0.1;Port=5432;Database=postgres;User Id=user;Password=password;
    // - PG_TEST_DB_TEMPLATE: Server=127.0.0.1;Port=5432;Database={0};User Id=user;Password=password;

    private static readonly string _masterDbConnectionString;
    private static readonly string _testDbConnectionStringTemplate;

    static TestEnvironment()
    {
        _masterDbConnectionString = Environment.GetEnvironmentVariable("PG_MASTER_DB") ?? throw new InvalidOperationException("`PG_MASTER_DB` var not set in env");
        _testDbConnectionStringTemplate = Environment.GetEnvironmentVariable("PG_TEST_DB_TEMPLATE") ?? throw new InvalidOperationException("`PG_TEST_DB_TEMPLATE` var not set in env");
    }

    private readonly string _dbName;
    
    public string ConnectionString { get; }

    public TestEnvironment()
    {
        _dbName = $"mde_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}";
        ConnectionString = string.Format(_testDbConnectionStringTemplate, _dbName);

        using NpgsqlConnection c = new NpgsqlConnection(_masterDbConnectionString);

        string sql = $"CREATE DATABASE {_dbName}";

        c.Execute(sql);
    }

    public void Dispose()
    {
        string sql = $"DROP DATABASE IF EXISTS {_dbName} WITH (FORCE)";

        using NpgsqlConnection c = new NpgsqlConnection(_masterDbConnectionString);

        c.Execute(sql);
    }
}