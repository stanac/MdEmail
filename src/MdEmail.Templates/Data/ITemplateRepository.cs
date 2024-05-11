using MdEmail.Templates.Models;

namespace MdEmail.Templates.Data;

public interface ITemplateRepository
{
    Task<IReadOnlyList<string>> ListTenantsAsync();
    Task<IReadOnlyList<TemplateInfo>> ListTemplatesAsync(string tenantKey);
    Task<Template?> GetAsync(string tenantKey, string templateKey);
    Task<Template?> GetAsync(string tenantKey, string templateKey, CancellationToken cancellationToken);
    Task DeleteAsync(string tenantKey, string templateKey);
    Task UpsertAsync(Template template);
}