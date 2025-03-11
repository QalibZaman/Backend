using Microsoft.Extensions.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using Scriban;

namespace NettoRetail.Infrastructure.Services
{
    public class EmailTemplateService(IOptions<EmailTemplateOptions> options) : IEmailTemplateService
    {
        private readonly string _templateFolderPath = options.Value.TemplateFolderPath;

        public async Task<string> RenderEmailTemplateAsync(string templatePath, object model)
        {
            var fullPath = Path.Combine(_templateFolderPath, templatePath);

            var templateText = await File.ReadAllTextAsync(fullPath);
            var template = Template.Parse(templateText);
            return await template.RenderAsync(model);
        }
    }
}
