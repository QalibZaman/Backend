using Ganss.Xss;
using MapsterMapper;
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

public class UpdateContentBySlugCommandHandler(
    IContentRepository contentRepo,
    IStaffRepository accountRepository,
    IMapper mapper)
    : IRequestHandler<UpdateContentBySlugCommand>
{
    private static readonly HtmlSanitizer sanitizer = new();

    public async Task Handle(UpdateContentBySlugCommand request, CancellationToken cancellationToken)
    {
        var accountSpecification = new GetStaffByIdSpecification(request.UpdateAuthor);
        var updateAuthor = await accountRepository.GetAsync(accountSpecification, cancellationToken)
            ?? throw new BadRequestException("Couldn't find account record with given user identifier");

        var slug = ContentSlugParser.ParseContentSlug(request.Slug);
        if (!string.IsNullOrEmpty(request.Model.Content))
        {
            request.Model.Content = sanitizer.Sanitize(request.Model.Content);
        }

        var spec = new GetContentByShopAndSectionSpecification(slug.ShopId, slug.SectionType);

        var contentBlock = await contentRepo.GetAsync(spec, cancellationToken);

        if (contentBlock == null)
        {
            contentBlock = mapper.Map<ContentEntity>(request.Model);
            contentBlock.SectionType = slug.SectionType;
            contentBlock.ShopId = slug.ShopId;

            ApplyAuditFields(contentBlock, updateAuthor.FullName);
            await contentRepo.AddAsync(contentBlock, cancellationToken);
            return;
        }

        mapper.Map(request.Model, contentBlock);

        ApplyAuditFields(contentBlock, updateAuthor.FullName);

        await contentRepo.UpdateAsync(contentBlock, cancellationToken);
    }

    private static void ApplyAuditFields(ContentEntity contentEntity, string updatedBy)
    {
        contentEntity.LastUpdated = DateTime.UtcNow;
        contentEntity.UpdatedBy = updatedBy;
    }
}
