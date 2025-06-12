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
    public async Task Handle(UpdateContentBySlugCommand request, CancellationToken cancellationToken)
    {
        var accountSpecification = new GetAccountByIdSpecification(request.UpdateAuthor);
        var updateAuthor = await accountRepository.GetAsync(accountSpecification, cancellationToken)
            ?? throw new BadRequestException("Couldn't find account record with given user identifier");

        var slugTuple = ContentSlugParser.ParseContentSlug(request.Slug);

        var spec = new GetContentByShopSpecification(slugTuple!.Value.ShopId);
        spec.And(new GetContentBySectionTypeSpecification(slugTuple!.Value.SectionType));

        var contentBlock = await contentRepo.GetAsync(spec, cancellationToken);

        if (contentBlock == null)
        {
            var sanitizer = new HtmlSanitizer();
            request.Model.Content = sanitizer.Sanitize(request.Model.Content);

            var contentBlockToCreate = request.Model.Adapt<ContentEntity>();

            contentBlockToCreate.LastUpdated = DateTime.UtcNow;
            contentBlockToCreate.UpdatedBy = updateAuthor.FullName;
            contentBlockToCreate.SectionType = slugTuple!.Value.SectionType;
            contentBlockToCreate.ShopId = slugTuple!.Value.ShopId;

            await contentRepo.AddAsync(contentBlockToCreate, cancellationToken);
            return;
        }

        request.Model.Adapt(contentBlock);

        contentBlock.LastUpdated = DateTime.UtcNow;
        contentBlock.UpdatedBy = updateAuthor.FullName;

        await contentRepo.UpdateAsync(contentBlock, cancellationToken);
    }
}
