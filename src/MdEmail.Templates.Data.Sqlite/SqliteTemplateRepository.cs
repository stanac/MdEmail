using MdEmail.Templates.Models;

namespace MdEmail.Templates.Data.Sqlite;

public class SqliteTemplateRepository : ITemplateRepository
{
    private readonly string _connectionString;

    public SqliteTemplateRepository(string connectionString)
    {
        _connectionString = connectionString;

        // TODO: initialize
    }

    public Task<IReadOnlyList<string>> ListTenantsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TemplateInfo>> ListTemplatesAsync(string tenantKey)
    {
        throw new NotImplementedException();
    }

    public Task<Template?> GetAsync(string tenantKey, string templateKey) => GetAsync(tenantKey, templateKey, CancellationToken.None);

    public Task<Template?> GetAsync(string tenantKey, string templateKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string tenantKey, string templateKey)
    {
        throw new NotImplementedException();
    }

    public Task UpsertAsync(Template template)
    {
        throw new NotImplementedException();
    }
}