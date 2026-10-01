using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.ShopDocument;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.ShopDocument.Commands
{
    public record EditShopDocumentCommand(EditShopDocumentRequest model, int shopId) : IRequest;
    public class EditShopDocumentsCommandHandler(
        IShopRepository shopRepository,
        IShopDocumentRepository shopDocumentRepository,
        IAzureBlobService azureBlobService) : IRequestHandler<EditShopDocumentCommand>
    {
        private const string ContainerName = "dot-net";
        public async Task Handle(EditShopDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.shopId <= 0) throw new BadRequestException("Shop Id cant be 0 or negative");
            var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.shopId), cancellationToken);
            if (!shopExists) throw new NotFoundException($"No shop with provided Id");
            var editedDocumentTypes = new[]
            {
                (request.model.RefundPolicy, ShopDocumentType.RefundPolicy),
                (request.model.ShippingPolicy, ShopDocumentType.ShippingPolicy),
                (request.model.TermsAndConditions, ShopDocumentType.TermsAndConditions),
                (request.model.PrivacyPolicy, ShopDocumentType.PrivacyPolicy)
            }.Where(x => x.Item1 is not null).ToList();

            foreach (var (file, documentType) in editedDocumentTypes)
            {
                if (file is null) continue;
                var document = await shopDocumentRepository.GetAsync(new GetShopDocumentByShopIdandDocumentType(request.shopId, (int)documentType), cancellationToken);

                if (document is not null)
                {
                    await azureBlobService.DeleteBlobAsync(document.BlobName);
                    document.BlobName = await AddDoc(file);
                    document.FileName = file.FileName;
                    document.FileSize = file.Length;
                    document.CreatedAt = DateTime.UtcNow;
                    await shopDocumentRepository.UpdateAsync(document, cancellationToken);
                }
                else
                {
                    document = new ShopDocumentEntity
                    {
                        BlobName = await AddDoc(file),
                        FileName = file.FileName,
                        FileSize = file.Length,
                        CreatedAt = DateTime.UtcNow
                    };
                    await shopDocumentRepository.AddAsync(document, cancellationToken);
                }

                return;
            }
        }

        public async Task<string> AddDoc(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            await using var stream = file.OpenReadStream();

            return await azureBlobService.UploadAsync(
                fileName,
                stream,
                ContainerName,
                BlobContentCategoryEnum.Document,
                overwrite: true,
                contentType: file.ContentType);
        }
    }
}
