using MediatR;
using Mapster;
using NettoRetail.Domain.Entities;
using NettoRetail.Application.Requests.TeaserSection;
using NettoRetail.Application.Responses.TeaserSection;
using NettoRetail.Application.Services.ImageUploadService;

namespace NettoRetail.Application.CQRS.TeaserSection.Commands;

public record UpdateTeaserSectionCommand(
    TeaserSectionRequest Model,
    int ShopId,
    string UpdatedBy
) : IRequest<TeaserSectionResponse>;

public class UpdateTeaserSectionCommandHandler(
    ITeaserSectionRepository teaserSectionRepository,
    IImageUploadService imageUploadService
) : IRequestHandler<UpdateTeaserSectionCommand, TeaserSectionResponse>
{
    public async Task<TeaserSectionResponse> Handle(UpdateTeaserSectionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetTeaserSectionByShopIdSpecification(request.ShopId);
        var teaser = await teaserSectionRepository.GetAsync(spec, cancellationToken);

        if (request.Model.TeaserImage != null)
        {
            var imageUrl = await imageUploadService.UploadImageAsync(
                request.Model.TeaserImage,
                $"teaser-section-images/{request.ShopId}",
                cancellationToken);

            request.Model.TeaserImageUrl = imageUrl;
        }

        if (teaser == null)
        {
            teaser = request.Model.Adapt<TeaserSectionEntity>();
            teaser.ShopId = request.ShopId;

            await teaserSectionRepository.AddAsync(teaser, cancellationToken);
        }
        else
        {
            request.Model.Adapt(teaser);

            await teaserSectionRepository.UpdateAsync(teaser, cancellationToken);
        }

        return teaser.Adapt<TeaserSectionResponse>();
    }
}
