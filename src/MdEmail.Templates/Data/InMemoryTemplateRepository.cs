using System.Collections.Concurrent;
using MdEmail.Templates.Models;

namespace MdEmail.Templates.Data;

public class InMemoryTemplateRepository : ITemplateRepository
{
    private const string KeySeparator = "|1cba9bebac3a4a1aa5351f835af6ed9c|";

    private ConcurrentDictionary<string, Template> _data = new();

    public Task<IReadOnlyList<string>> ListTenantsAsync()
    {
        IReadOnlyList<string> data = _data.Keys
            .Select(x => x.Split(KeySeparator)[0])
            .Distinct()
            .ToList();

        return Task.FromResult(data);
    }

    public Task<IReadOnlyList<TemplateInfo>> ListTemplatesAsync(string tenantKey)
    {
        string startsWith = tenantKey + KeySeparator;

        IReadOnlyList<TemplateInfo> data = _data
            .Where(x => x.Key.StartsWith(startsWith))
            .Select(x => x.Value.CloneTemplateInfo())
            .ToList();

        return Task.FromResult(data);
    }

    public Task<Template?> GetAsync(string tenantKey, string templateKey)
    {
        string key = tenantKey + KeySeparator + templateKey;

        _data.TryGetValue(key, out Template? template);

        return Task.FromResult(template?.CloneTemplate());
    }

    public Task<Template?> GetAsync(string tenantKey, string templateKey, CancellationToken cancellationToken) => GetAsync(tenantKey, templateKey);

    public Task DeleteAsync(string tenantKey, string templateKey)
    {
        string key = tenantKey + KeySeparator + templateKey;

        _data.Remove(key, out _);

        return Task.CompletedTask;
    }

    public Task UpsertAsync(Template template)
    {
        string key = template.TenantKey + KeySeparator + template.TenantKey;

        _data[key] = template.CloneTemplate();

        return Task.CompletedTask;
    }
}