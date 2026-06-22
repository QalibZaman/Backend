using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Requests.TeaserSection;
using NettoRetail.Application.Responses.TeaserSection;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.TeaserSection.Commands;

public record CreateTeaserSectionCommand(CreateTeaserSectionRequest Model, int ShopId) : IRequest<TeaserSectionResponse>;
public class CreateTeaserSectionCommandHandler(
    IAzureBlobService azureBlobService,
    IBlobNameGenerator blobNameGenerator,
    IImageUploadService imageUploadService,
    IImageValidatorService imageValidatorService,
    ITeaserSectionRepository teaserSectionRepository,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateTeaserSectionCommand, TeaserSectionResponse>
{
    public async Task<TeaserSectionResponse> Handle(CreateTeaserSectionCommand request, CancellationToken cancellationToken)
    {
        var containerName = azureBlobStorageOptions.Value.ContainerName;
        var teaserSection = mapper.Map<TeaserSectionEntity>(request.Model);
        teaserSection.ShopId = request.ShopId;
        using var transaction = unitOfWork.BeginTransaction();
        await teaserSectionRepository.AddAsync(teaserSection, cancellationToken);
        if (request.Model.TeaserImage is { Length: > 0 } file)
        {
            await UpdateTeaserImageUrlAsync(request, file, teaserSection, containerName, cancellationToken);
            await teaserSectionRepository.UpdateAsync(teaserSection, cancellationToken);
        }

        transaction.Commit();
        return mapper.Map<TeaserSectionResponse>(teaserSection);
    }

    private async Task UpdateTeaserImageUrlAsync(CreateTeaserSectionCommand request, IFormFile file, TeaserSectionEntity teaserSection, string containerName, CancellationToken cancellationToken)
    {
        await imageValidatorService.ValidateImage(request.Model.TeaserImage);

        await imageValidatorService.ValidateBannerImageAsync(request.Model.TeaserImage, cancellationToken);

        teaserSection.TeaserImageUrl = await imageUploadService.UploadImageAsync(
            request.Model.TeaserImage,
            $"teaser-section-images/{request.ShopId}",
            cancellationToken);
    }
}