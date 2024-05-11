namespace MdEmail.Templates.Models;

public class Template : TemplateInfo
{
    public string? HtmlTemplate { get; set; }
    public string? TextTemplate { get; set; }
    public string? MdTemplate { get; set; }

    /// <summary>
    /// Throws exception if template is not valid
    /// </summary>
    public void EnsureValid()
    {
        if (MdTemplate is null && TextTemplate is null)
        {
            throw new InvalidOperationException(
                $"{nameof(Template)} not valid. When {nameof(MdTemplate)} is set, other template data will be ignored. " +
                $"When {nameof(MdTemplate)} is not set, at least {nameof(TextTemplate)} must be set. " +
                $"Setting {nameof(HtmlTemplate)} is optional but recommended when {nameof(MdTemplate)} is not set."
            );
        }
    }

    internal Template CloneTemplate()
    {
        return (Template)MemberwiseClone();
    }
}