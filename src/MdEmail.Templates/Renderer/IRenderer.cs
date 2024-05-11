namespace MdEmail.Templates.Renderer;

public interface IRenderer
{
    /// <summary>
    /// If set to true, template email sender will use <see cref="Render"/> method instead of <see cref="RenderAsync"/> method
    /// </summary>
    bool UseSyncRendering { get; }

    /// <summary>
    /// Renders template to string.
    /// </summary>
    /// <param name="template">Template for rendering</param>
    /// <param name="data">Template data</param>
    /// <returns>Rendered email body</returns>
    string Render(string template, IDictionary<string, object> data);

    /// <summary>
    /// Renders template asynchronously
    /// </summary>
    /// <param name="template">Template for rendering</param>
    /// <param name="data">Template data</param>
    /// <param name="cancellationToken">Cancellation</param>
    /// <returns>Rendered email body</returns>
    Task<string> RenderAsync(string template, IDictionary<string, object> data, CancellationToken cancellationToken);
}