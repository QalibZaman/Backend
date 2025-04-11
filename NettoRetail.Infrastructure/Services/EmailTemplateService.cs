using System.Text;
using NettoRetail.Infrastructure.Interfaces;
using Scriban;

namespace NettoRetail.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templateFolderPath;

        public EmailTemplateService()
        {
            _templateFolderPath = GetTemplateFolderPath();
        }

        public async Task<string> RenderEmailTemplateAsync(string templatePath, object model)
        {
            var fullPath = Path.Combine(_templateFolderPath, templatePath);

            var templateText = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
            var template = Template.Parse(templateText);
            return await template.RenderAsync(model);
        }

        private static string GetTemplateFolderPath()
        {
            string basePath = AppContext.BaseDirectory;

            string? solutionRoot = basePath;
            while (solutionRoot != null && !Directory.Exists(Path.Combine(solutionRoot, "NettoRetail.Infrastructure")))
            {
                solutionRoot = Directory.GetParent(solutionRoot)?.FullName;
            }

            string infraPath = Path.Combine(solutionRoot, "NettoRetail.Infrastructure", "EmailTemplates");

            return infraPath;
        }
    }
}
