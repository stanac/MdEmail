using Dapper;
using MdEmail.Templates.Models;
using Microsoft.Data.Sqlite;

namespace MdEmail.Templates.Data.Sqlite;

public class SqliteTemplateRepository : ITemplateRepository
{
    private readonly string _connectionString;
    private readonly string _dbTableName;
    private readonly bool _useWal;

    public SqliteTemplateRepository(string connectionString)
        : this (connectionString, "mdEmailTemplates")
    {
    }

    public SqliteTemplateRepository(string connectionString, string dbTableName, bool useWal = true)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
        if (string.IsNullOrWhiteSpace(dbTableName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbTableName));

        _connectionString = connectionString;
        _dbTableName = dbTableName;
        _useWal = useWal;

        Initialize();
    }

    public Task<IReadOnlyList<string>> ListTenantsAsync()
    {
        return Task.FromResult(ListTenants());
    }

    public Task<IReadOnlyList<TemplateInfo>> ListTemplatesAsync(string tenantKey)
    {
        return Task.FromResult(ListTemplates(tenantKey));
    }

    public Task<Template?> GetAsync(string tenantKey, string templateKey, CancellationToken cancellationToken)
    {
        return GetAsync(tenantKey, templateKey);
    }

    public Task<Template?> GetAsync(string tenantKey, string templateKey)
    {
        return Task.FromResult(Get(tenantKey, templateKey));
    }

    public Task DeleteAsync(string tenantKey, string templateKey)
    {
        Delete(tenantKey, templateKey);
        return Task.CompletedTask;
    }

    public Task UpsertAsync(Template template)
    {
        Upsert(template);
        return Task.CompletedTask;
    }

    private IReadOnlyList<string> ListTenants()
    {
        string sql = $"SELECT DISTINCT TenantKey FROM {_dbTableName}";

        using SqliteConnection c = CreateAndOpenConnection();

        return c.Query<string>(sql).ToList();
    }

    private IReadOnlyList<TemplateInfo> ListTemplates(string tenantKey)
    {
        string sql = $@"
            SELECT TemplateKey, TenantKey, Renderer, Subject, CreatedBy, CreatedDate, LastEditedBy, LastEditedDate
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey
        ";

        object p = new { tenantKey };

        using SqliteConnection c = CreateAndOpenConnection();

        List<TemplateInfoDbModel> dbModels = c.Query<TemplateInfoDbModel>(sql, p).ToList();

        return dbModels
            .Select(x => x.ToTemplateInfo())
            .ToList();
    }

    private Template? Get(string tenantKey, string templateKey)
    {
        string sql = $@"
            SELECT *
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey AND TemplateKey = @templateKey
        ";

        object p = new { templateKey, tenantKey };

        using SqliteConnection c = CreateAndOpenConnection();

        TemplateDbModel? result = c.Query<TemplateDbModel>(sql, p).FirstOrDefault();

        return result?.ToTemplate();
    }

    private void Delete(string tenantKey, string templateKey)
    {
        string sql = $@"
            DELETE
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey AND TemplateKey = @templateKey
        ";

        object p = new { templateKey, tenantKey };

        using SqliteConnection c = CreateAndOpenConnection();
        c.Execute(sql, p);
    }

    private void Upsert(Template template)
    {
        string sqlDelete = $@"
            DELETE
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey AND TemplateKey = @templateKey
        ";

        object deleteParams = new { templateKey = template.TemplateKey, tenantKey = template.TenantKey };
        
        string sqlInsert = $@"
            INSERT INTO {_dbTableName} (TemplateKey, TenantKey, Renderer, Subject, CreatedBy, CreatedDate, LastEditedBy, LastEditedDate, HtmlTemplate, TextTemplate, MdTemplate)
            VALUES (@TemplateKey, @TenantKey, @Renderer, @Subject, @CreatedBy, @CreatedDate, @LastEditedBy, @LastEditedDate, @HtmlTemplate, @TextTemplate, @MdTemplate)
        ";

        using SqliteConnection c = CreateAndOpenConnection();
        SqliteTransaction trans = c.BeginTransaction();

        c.Execute(sqlDelete, deleteParams, trans);
        c.Execute(sqlInsert, new TemplateDbModel(template));

        trans.Commit();
    }

    private void Initialize()
    {
        string createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {_dbTableName} (
                TenantKey      TEXT NOT NULL,
                TemplateKey    TEXT NOT NULL,
                Renderer       TEXT NOT NULL,
                Subject        TEXT NOT NULL,
                CreatedBy      TEXT NULL,
                CreatedDate    INTEGER NULL,
                LastEditedBy   TEXT NULL,
                LastEditedDate INTEGER NULL,
                HtmlTemplate   TEXT NULL,
                TextTemplate   TEXT NULL,
                MdTemplate     TEXT NULL,
                PRIMARY KEY (TenantKey, TemplateKey)
            )
        ";

        string createIndexSql = $@"
            CREATE INDEX IF NOT EXISTS IX_{_dbTableName}_TenantKey ON {_dbTableName}(TenantKey)
        ";

        using SqliteConnection c = CreateAndOpenConnection();

        c.Execute(createTableSql);
        c.Execute(createIndexSql);
    }

    private SqliteConnection CreateAndOpenConnection()
    {
        SqliteConnection c = new SqliteConnection(_connectionString);
        c.Open();

        if (_useWal)
        {
            c.Execute("PRAGMA journal_mode=WAL;");
        }

        return c;
    }
}