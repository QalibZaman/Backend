using Mapster;
using MediatR;
using NettoRetail.Application.Requests.TeaserSection;
using NettoRetail.Application.Responses.TeaserSection;
using NettoRetail.Application.Services.IImageUploadService;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;

namespace NettoRetail.Application.CQRS.TeaserSection.Commands;

public record UpdateTeaserSectionCommand(
    TeaserSectionRequest Model,
    int ShopId,
    string UpdatedBy
) : IRequest<TeaserSectionResponse>;

public class UpdateTeaserSectionCommandHandler(
    ITeaserSectionRepository teaserSectionRepository,
    IImageUploadService imageUploadService,
    IAzureBlobService azureBlobService) : IRequestHandler<UpdateTeaserSectionCommand, TeaserSectionResponse>
{
    public async Task<TeaserSectionResponse> Handle(UpdateTeaserSectionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetTeaserSectionByShopIdSpecification(request.ShopId);
        var teaser = await teaserSectionRepository.GetAsync(spec, cancellationToken);

        var isNew = teaser is null;

        teaser ??= new TeaserSectionEntity { ShopId = request.ShopId };

        request.Model.Adapt(teaser);

        await UploadTeaserImageAsync(request, teaser, cancellationToken);

        if (isNew)
            await teaserSectionRepository.AddAsync(teaser, cancellationToken);
        else
            await teaserSectionRepository.UpdateAsync(teaser, cancellationToken);

        return teaser.Adapt<TeaserSectionResponse>();
    }

    private async Task UploadTeaserImageAsync(UpdateTeaserSectionCommand request, TeaserSectionEntity entity, CancellationToken cancellationToken)
    {
        if (request.Model.TeaserImage is null)
            return;

        if (entity.TeaserImageUrl != null)
            await azureBlobService.DeleteByUrlAsync(entity.TeaserImageUrl);

        entity.TeaserImageUrl = await imageUploadService.UploadImageAsync(
            request.Model.TeaserImage,
            $"teaser-section-images/{request.ShopId}",
            cancellationToken);
    }
}
