using System.Text;
using NettoRetail.Infrastructure.Interfaces;
using Scriban;

namespace NettoRetail.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public async Task<string> RenderEmailTemplateAsync(string templateFileName, object model)
        {
            var assembly = typeof(EmailTemplateService).Assembly;
            var resourcePath = $"NettoRetail.Infrastructure.EmailTemplates.{templateFileName}";

            using var stream = assembly.GetManifestResourceStream(resourcePath)
                ?? throw new FileNotFoundException($"Email template '{templateFileName}' not found in embedded resources.");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var templateText = await reader.ReadToEndAsync();

            var template = Template.Parse(templateText);
            return await template.RenderAsync(model);
        }
    }
}
