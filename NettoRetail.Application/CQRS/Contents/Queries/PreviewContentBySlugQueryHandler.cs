using MediatR;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public record PreviewContentBySlugQuery(string Slug, UpdateContentRequest Model) : IRequest<ContentPreviewResponse>;

public class PreviewContentBySlugQueryHandler
    : IRequestHandler<PreviewContentBySlugQuery, ContentPreviewResponse>
{
    public Task<ContentPreviewResponse> Handle(PreviewContentBySlugQuery request, CancellationToken cancellationToken)
    {
        var (shopId, sectionType) = ContentSlugParser.ParseContentSlug(request.Slug);
        var sanitizedContent = ContentBlockRules.Sanitize(request.Model.Content);

        return Task.FromResult(
            ContentResponseFactory.CreatePreview(
                shopId,
                sectionType,
                sanitizedContent,
                ContentBlockRules.ShouldBeVisible(request.Model.IsVisible, sanitizedContent)));
    }
}
