using System.Diagnostics.CodeAnalysis;
using MdEmail.Templates.Models;

namespace MdEmail.Templates.Data.Sqlite;

internal class TemplateDbModel : TemplateInfoDbModel
{
    public string? HtmlTemplate { get; set; }
    public string? TextTemplate { get; set; }
    public string? MdTemplate { get; set; }

    public TemplateDbModel()
    {
    }

    [SetsRequiredMembers]
    public TemplateDbModel(Template template)
        : base(template)
    {
        HtmlTemplate = template.HtmlTemplate;
        TextTemplate = template.TextTemplate;
        MdTemplate = template.MdTemplate;
    }

    public Template ToTemplate()
    {
        Template model = new Template
        {
            Renderer = Renderer,
            Subject = Subject,
            TemplateKey = TemplateKey,
            TenantKey = TenantKey,
            LastEditedBy = LastEditedBy,
            CreatedBy = CreatedBy,
            HtmlTemplate = HtmlTemplate,
            TextTemplate = TextTemplate,
            MdTemplate = MdTemplate
        };

        if (CreatedDate.HasValue) model.CreatedDate = DateTimeOffset.FromUnixTimeSeconds(CreatedDate.Value);

        if (LastEditedDate.HasValue) model.LastEditedDate = DateTimeOffset.FromUnixTimeSeconds(LastEditedDate.Value);

        return model;
    }
}