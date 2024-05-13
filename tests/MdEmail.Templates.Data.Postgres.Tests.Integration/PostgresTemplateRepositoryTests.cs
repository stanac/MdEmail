using FluentAssertions;
using MdEmail.Templates.Models;

namespace MdEmail.Templates.Data.Postgres.Tests.Integration;

public class PostgresTemplateRepositoryTests : IDisposable
{
    private readonly TestEnvironment _env = new();
    private readonly PostgresTemplateRepository _sut;

    public PostgresTemplateRepositoryTests()
    {
        _sut = new PostgresTemplateRepository(_env.ConnectionString);
    }
    
    [Fact]
    public async Task EmptyRepo_ListTenants_ReturnsEmpty()
    {
        IReadOnlyList<string> tenants = await _sut.ListTenantsAsync();

        tenants.Should().BeEmpty();
    }

    [Fact]
    public async Task EmptyRepo_ListTemplates_ReturnsEmpty()
    {
        IReadOnlyList<TemplateInfo> templates = await _sut.ListTemplatesAsync("tenant1");
        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task EmptyRepo_Get_ReturnsNull()
    {
        Template? template = await _sut.GetAsync("tenant", "template");
        template.Should().BeNull();
    }

    [Fact]
    public async Task AddTemplate_GetTemplate_ReturnsTemplate()
    {
        Template template1 = TestTemplate("1", "2");

        await _sut.UpsertAsync(template1);

        Template? template2 = await _sut.GetAsync(template1.TenantKey, template1.TemplateKey);

        template2.Should().NotBeNull();
        template2.Should().BeEquivalentTo(template1);
    }

    [Fact]
    public async Task Add_Delete_Get_ReturnsNull()
    {
        AssertCount(0);

        Template template1 = TestTemplate("1", "2");
        await _sut.UpsertAsync(template1);

        AssertCount(1);

        await _sut.DeleteAsync(template1.TenantKey, template1.TemplateKey);

        Template? template2 = await _sut.GetAsync(template1.TenantKey, template1.TemplateKey, CancellationToken.None);
        template2.Should().BeNull();

        AssertCount(0);
    }

    [Fact]
    public async Task UpsertExisting_UpdatesTemplate()
    {
        Template template1 = TestTemplate("1", "2");

        await _sut.UpsertAsync(template1);

        Template template2 = new Template
        {
            Renderer = template1.Renderer,
            TemplateKey = template1.TemplateKey,
            TenantKey = template1.TenantKey,
            CreatedDate = template1.CreatedDate,
            CreatedBy = template1.CreatedBy,
            Subject = template1.Subject,
            LastEditedDate = template1.LastEditedDate,
            LastEditedBy = template1.LastEditedBy,
            HtmlTemplate = "new html",
            MdTemplate = "new md",
            TextTemplate = "new txt"
        };

        await _sut.UpsertAsync(template2);

        Template? template3 = await _sut.GetAsync(template1.TenantKey, template1.TemplateKey);

        template3.Should().NotBeNull();

        template3!.MdTemplate.Should().Be("new md");
        template3.HtmlTemplate.Should().Be("new html");
        template3.TextTemplate.Should().Be("new txt");
    }

    [Fact]
    public async Task InsertTwoDifferentTenants_ListTenants_ReturnsTenants()
    {
        const string tenant1 = "t1";
        const string tenant2 = "t2";

        var template1 = TestTemplate("t1", tenant1);
        var template2 = TestTemplate("t1", tenant2);

        await _sut.UpsertAsync(template1);
        await _sut.UpsertAsync(template2);

        IReadOnlyList<string> tenants = await _sut.ListTenantsAsync();

        tenants.Should().HaveCount(2);
        tenants.Should().Contain(tenant1);
        tenants.Should().Contain(tenant2);
    }

    private void AssertCount(int countExpected)
    {
        IReadOnlyList<string> tenants = _sut.ListTenantsAsync().GetAwaiter().GetResult();

        if (countExpected == 0 && !tenants.Any())
        {
            return;
        }

        int count = 0;

        foreach (string tenant in tenants)
        {
            count += _sut.ListTemplatesAsync(tenant).GetAwaiter().GetResult().Count;
        }

        count.Should().Be(countExpected);
    }

    private static Template TestTemplate(string key, string tenant)
    {
        static DateTimeOffset RoundTime(DateTimeOffset dto)
        {
            return DateTimeOffset.FromUnixTimeSeconds(dto.ToUnixTimeSeconds());
        }

        return new Template
        {
            Renderer = "1",
            Subject = "subject",
            TemplateKey = key,
            TenantKey = tenant,
            CreatedDate = RoundTime(DateTimeOffset.UtcNow.AddMinutes(-3)),
            LastEditedDate = RoundTime(DateTimeOffset.UtcNow),
            LastEditedBy = Guid.NewGuid().ToString(),
            CreatedBy = Guid.NewGuid().ToString(),
            HtmlTemplate = Guid.NewGuid().ToString(),
            MdTemplate = Guid.NewGuid().ToString(),
            TextTemplate = Guid.NewGuid().ToString()
        };
    }

    public void Dispose()
    {
        _env.Dispose();
    }
}