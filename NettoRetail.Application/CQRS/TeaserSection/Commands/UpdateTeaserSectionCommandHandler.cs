using MapsterMapper;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Requests.TeaserSection;
using NettoRetail.Application.Responses.TeaserSection;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.DataAccess.Specifications.TeaserSection;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;

namespace NettoRetail.Application.CQRS.TeaserSection.Commands;

public record UpdateTeaserSectionCommand(
    TeaserSectionRequest Model,
    int ShopId,
    string UpdatedBy
) : IRequest<TeaserSectionResponse>;

public class UpdateTeaserSectionCommandHandler(
    ITeaserSectionRepository teaserSectionRepository,
    IImageUploadService imageUploadService,
    IAzureBlobService azureBlobService,
    IImageValidatorService imageValidatorService,
    IMapper mapper) : IRequestHandler<UpdateTeaserSectionCommand, TeaserSectionResponse>
{
    public async Task<TeaserSectionResponse> Handle(UpdateTeaserSectionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetTeaserSectionByShopIdSpecification(request.ShopId);
        var teaser = await teaserSectionRepository.GetAsync(spec, cancellationToken);

        var isNew = teaser is null;

        teaser ??= new TeaserSectionEntity { ShopId = request.ShopId };

        mapper.Map(request.Model, teaser);

        await ValidateUrlUniquenessAsync(teaser.Link, teaser.Id, cancellationToken);

        await UploadTeaserImageAsync(request, teaser, cancellationToken);

        if (isNew)
        {
            await teaserSectionRepository.AddAsync(teaser, cancellationToken);
        }
        else
        {
            await teaserSectionRepository.UpdateAsync(teaser, cancellationToken);
        }

        return mapper.Map<TeaserSectionResponse>(teaser);
    }

    private async Task UploadTeaserImageAsync(UpdateTeaserSectionCommand request, TeaserSectionEntity entity, CancellationToken cancellationToken)
    {
        if (request.Model.TeaserImage is null)
            return;

        await imageValidatorService.ValidateImage(request.Model.TeaserImage);

        if (entity.TeaserImageUrl != null)
            await azureBlobService.DeleteByUrlAsync(entity.TeaserImageUrl);

        await imageValidatorService.ValidateBannerImageAsync(request.Model.TeaserImage, cancellationToken);

        entity.TeaserImageUrl = await imageUploadService.UploadImageAsync(
            request.Model.TeaserImage,
            $"teaser-section-images/{request.ShopId}",
            cancellationToken);
    }

    private async Task ValidateUrlUniquenessAsync(string? url, Guid id, CancellationToken cancellationToken)
    {
        if (url.IsNullOrEmpty())
            return;

        var spec = new GetTeaserSectionByUrlExcludingCurrentSpecification(url, id);
        if (await teaserSectionRepository.ExistsAsync(spec, cancellationToken))
            throw new ConflictException("Teaser section with such URL already exists");
    }
}
