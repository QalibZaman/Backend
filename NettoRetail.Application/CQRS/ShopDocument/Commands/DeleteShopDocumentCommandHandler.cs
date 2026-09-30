using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.ShopDocument;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ShopDocument.Commands
{
    public record DeleteShopDocumentCommand(DeleteShopDocumentRequest model, int shopId) : IRequest;
    public class DeleteShopDocumentCommandHandler(
        IShopRepository shopRepository,
        IShopDocumentRepository shopDocumentRepository,
        IAzureBlobService azureBlobService) : IRequestHandler<DeleteShopDocumentCommand>
    {
        public async Task Handle(DeleteShopDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.shopId <= 0) throw new BadRequestException("Shop Id cant be 0 or negative");
            var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.shopId), cancellationToken);
            if (!shopExists) throw new NotFoundException($"No shop with provided Id");
            var deletedDocumentTypes = new[]
            {
                (request.model.DeleteRefundPolicy, ShopDocumentType.RefundPolicy),
                (request.model.DeleteShippingPolicy, ShopDocumentType.ShippingPolicy),
                (request.model.DeleteTermsAndConditions, ShopDocumentType.TermsAndConditions),
                (request.model.DeletePrivacyPolicy, ShopDocumentType.PrivacyPolicy)
            }.Where(x => x.Item1 is not false).ToList();

            foreach (var (item1, documentType) in deletedDocumentTypes)
            {
                SpecificationBuilder<ShopDocumentEntity> builder = new SpecificationBuilder<ShopDocumentEntity>();
                if (item1 is false) continue;
                ShopDocumentEntity entity = await shopDocumentRepository.GetAsync(new GetShopDocumentByShopIdandDocumentType(request.shopId, (int)documentType), cancellationToken);
                if (entity is null) continue;
                await azureBlobService.DeleteBlobAsync(entity.BlobName);
                builder.AddCriteria(x => x.DocumentType.Equals(documentType));
                await shopDocumentRepository.DeleteAsync(builder.Build(), cancellationToken);
            }

            return;
        }
    }
}
