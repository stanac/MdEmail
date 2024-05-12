using System.Diagnostics.CodeAnalysis;
using MdEmail.Templates.Models;

namespace MdEmail.Templates.Data.Sqlite;

internal class TemplateInfoDbModel
{
    public required string TemplateKey { get; set; }
    public required string TenantKey { get; set; }
    public required string Renderer { get; set; }
    public required string Subject { get; set; }

    public string? CreatedBy { get; set; }
    public long? CreatedDate { get; set; }
    public string? LastEditedBy { get; set; }
    public long? LastEditedDate { get; set; }

    public TemplateInfoDbModel()
    {
    }

    [SetsRequiredMembers]
    public TemplateInfoDbModel(TemplateInfo templateInfo)
    {
        TemplateKey = templateInfo.TemplateKey;
        TenantKey = templateInfo.TenantKey;
        Renderer = templateInfo.Renderer;
        Subject = templateInfo.Subject;
        CreatedBy = templateInfo.CreatedBy;
        CreatedDate = templateInfo.CreatedDate?.ToUnixTimeSeconds();
        LastEditedBy = templateInfo.LastEditedBy;
        LastEditedDate = templateInfo.LastEditedDate?.ToUnixTimeSeconds();
    }

    public TemplateInfo ToTemplateInfo()
    {
        TemplateInfo model = new TemplateInfo
        {
            Renderer = Renderer,
            Subject = Subject,
            TemplateKey = TemplateKey,
            TenantKey = TenantKey,
            LastEditedBy = LastEditedBy,
            CreatedBy = CreatedBy
        };

        if (CreatedDate.HasValue) model.CreatedDate = DateTimeOffset.FromUnixTimeSeconds(CreatedDate.Value);

        if (LastEditedDate.HasValue) model.LastEditedDate = DateTimeOffset.FromUnixTimeSeconds(LastEditedDate.Value);

        return model;
    }
}