using Dapper;
using MdEmail.Templates.Models;
using Npgsql;

namespace MdEmail.Templates.Data.Postgres;

public class PostgresTemplateRepository : ITemplateRepository
{
    private readonly string _connectionString;
    private readonly string _dbTableName;

    public PostgresTemplateRepository(string connectionString)
        : this(connectionString, "mdEmailTemplates")
    {
    }

    public PostgresTemplateRepository(string connectionString, string dbTableName)
    {
        _connectionString = connectionString;
        _dbTableName = dbTableName;

        Initialize();
    }

    public async Task<IReadOnlyList<string>> ListTenantsAsync()
    {
        string sql = $@"
            SELECT DISTINCT TenantKey
            FROM {_dbTableName}
        ";

        await using NpgsqlConnection c = await CreateAndOpenConnection();

        return (await c.QueryAsync<string>(sql)).ToList();
    }

    public async Task<IReadOnlyList<TemplateInfo>> ListTemplatesAsync(string tenantKey)
    {
        if (string.IsNullOrWhiteSpace(tenantKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tenantKey));

        string sql = $@"
            SELECT TemplateKey, TenantKey, Renderer, Subject, CreatedBy, CreatedDate, LastEditedBy, LastEditedDate
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey
        ";

        object p = new { tenantKey };

        await using NpgsqlConnection c = await CreateAndOpenConnection();

        List<TemplateInfoDbModel> data = (await c.QueryAsync<TemplateInfoDbModel>(sql, p)).ToList();

        return data.Select(x => x.ToTemplateInfo()).ToList();
    }

    public Task<Template?> GetAsync(string tenantKey, string templateKey) => GetAsync(tenantKey, templateKey, CancellationToken.None);

    public async Task<Template?> GetAsync(string tenantKey, string templateKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tenantKey));
        if (string.IsNullOrWhiteSpace(templateKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateKey));

        await using NpgsqlConnection c = await CreateAndOpenConnection();

        string sql = $@"
            SELECT *
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey AND TemplateKey = @templateKey
        ";

        object p = new { templateKey, tenantKey };

        TemplateDbModel? data = await c.QueryFirstOrDefaultAsync<TemplateDbModel>(sql, p);

        return data?.ToTemplate();
    }

    public async Task DeleteAsync(string tenantKey, string templateKey)
    {
        if (string.IsNullOrWhiteSpace(tenantKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tenantKey));
        if (string.IsNullOrWhiteSpace(templateKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateKey));

        await using NpgsqlConnection c = await CreateAndOpenConnection();
        
        string sql = $@"
            DELETE
            FROM {_dbTableName}
            WHERE TenantKey = @tenantKey AND TemplateKey = @templateKey
        ";

        object p = new { templateKey, tenantKey };

        await c.ExecuteAsync(sql, p);
    }

    public async Task UpsertAsync(Template template)
    {
        string sql = $@"
            INSERT INTO {_dbTableName} (TemplateKey, TenantKey, Renderer, Subject, CreatedBy, CreatedDate, LastEditedBy, LastEditedDate, HtmlTemplate, TextTemplate, MdTemplate)
            VALUES (@TemplateKey, @TenantKey, @Renderer, @Subject, @CreatedBy, @CreatedDate, @LastEditedBy, @LastEditedDate, @HtmlTemplate, @TextTemplate, @MdTemplate)
            ON CONFLICT (TemplateKey, TenantKey)
            DO UPDATE SET
                Renderer = @Renderer,
                Subject = @Subject, 
                CreatedBy = @CreatedBy, 
                CreatedDate = @CreatedDate, 
                LastEditedBy = @LastEditedBy, 
                LastEditedDate = @LastEditedDate, 
                HtmlTemplate = @HtmlTemplate, 
                TextTemplate = @TextTemplate,
                MdTemplate = @MdTemplate
        ";

        var dbModel = new TemplateDbModel(template);

        await using NpgsqlConnection c = await CreateAndOpenConnection();

        await c.ExecuteAsync(sql, dbModel);
    }

    private void Initialize()
    {
        string createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {_dbTableName} (
                TenantKey      VARCHAR(250) NOT NULL,
                TemplateKey    VARCHAR(250) NOT NULL,
                Renderer       VARCHAR(250) NOT NULL,
                Subject        VARCHAR(250) NOT NULL,
                CreatedBy      VARCHAR(250) NULL,
                CreatedDate    BIGINT NULL,
                LastEditedBy   VARCHAR(250) NULL,
                LastEditedDate BIGINT NULL,
                HtmlTemplate   TEXT NULL,
                TextTemplate   TEXT NULL,
                MdTemplate     TEXT NULL,
                PRIMARY KEY (TenantKey, TemplateKey)
            )
        ";

        string createIndexSql = $@"
            CREATE INDEX IF NOT EXISTS IX_{_dbTableName}_TenantKey ON {_dbTableName}(TenantKey)
        ";

        NpgsqlConnection c = new NpgsqlConnection(_connectionString);
        c.Open();

        c.Execute(createTableSql);
        c.Execute(createIndexSql);
    }

    private async Task<NpgsqlConnection> CreateAndOpenConnection()
    {
        NpgsqlConnection c = new NpgsqlConnection(_connectionString);
        await c.OpenAsync();

        return c;
    }
}