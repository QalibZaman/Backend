namespace NettoRetail.Infrastructure.Interfaces;

public interface IEmailTemplateService
{
    Task<string> RenderEmailTemplateAsync(string templatePath, object model);
}
