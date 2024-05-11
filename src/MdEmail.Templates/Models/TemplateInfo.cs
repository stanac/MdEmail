namespace MdEmail.Templates.Models;

public class TemplateInfo
{
    public required string TemplateKey { get; set; }
    public required string TenantKey { get; set; }
    public required string Renderer { get; set; }
    public required string Subject { get; set; }

    public string? CreateBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? LastEditedBy { get; set; }
    public DateTimeOffset? LastEditedDate { get; set; }

    internal TemplateInfo CloneTemplateInfo()
    {
        return new TemplateInfo
        {
            TemplateKey = TemplateKey,
            TenantKey = TenantKey,
            Renderer = Renderer,
            Subject = Subject,

            CreateBy = CreateBy,
            CreatedDate = CreatedDate,
            LastEditedBy = LastEditedBy,
            LastEditedDate = LastEditedDate,
        };
    }
}