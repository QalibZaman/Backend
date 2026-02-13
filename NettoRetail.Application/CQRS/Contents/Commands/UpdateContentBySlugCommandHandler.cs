using Ganss.Xss;
using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Contents.Commands;

public record UpdateContentBySlugCommand(string Slug, Guid UpdateAuthor, UpdateContentRequest Model)
    : IRequest;

public class UpdateContentBySlugCommandHandler(IContentRepository contentRepo,
    IAccountRepository accountRepository)
    : IRequestHandler<UpdateContentBySlugCommand>
{
    private static readonly HtmlSanitizer Sanitizer = new();

    public async Task Handle(UpdateContentBySlugCommand request, CancellationToken cancellationToken)
    {
        var accountSpecification = new GetAccountByIdSpecification(request.UpdateAuthor);
        var updateAuthor = await accountRepository.GetAsync(accountSpecification, cancellationToken)
            ?? throw new BadRequestException("Couldn't find account record with given user identifier");

        var slug = ContentSlugParser.ParseContentSlug(request.Slug);
        request.Model.Content = SanitizeContent(request.Model.Content);

        var spec = new GetContentByShopAndSectionSpecification(slug.ShopId, slug.SectionType);

        var contentBlock = await contentRepo.GetAsync(spec, cancellationToken);
        var sanitizer = new HtmlSanitizer();
        request.Model.Content = sanitizer.Sanitize(request.Model.Content);

        if (contentBlock == null)
        {
            var contentBlockToCreate = request.Model.Adapt<ContentEntity>();

            ApplyAuditFields(contentBlockToCreate, updateAuthor.FullName);
            contentBlockToCreate.SectionType = slug.SectionType;
            contentBlockToCreate.ShopId = slug.ShopId;

            await contentRepo.AddAsync(contentBlockToCreate, cancellationToken);
            return;
        }

        request.Model.Adapt(contentBlock);

        ApplyAuditFields(contentBlock, updateAuthor.FullName);

        await contentRepo.UpdateAsync(contentBlock, cancellationToken);
    }

    private static string SanitizeContent(string content)
    {
        return Sanitizer.Sanitize(content);
    }

    private static void ApplyAuditFields(ContentEntity contentEntity, string updatedBy)
    {
        contentEntity.LastUpdated = DateTime.UtcNow;
        contentEntity.UpdatedBy = updatedBy;
    }
}
