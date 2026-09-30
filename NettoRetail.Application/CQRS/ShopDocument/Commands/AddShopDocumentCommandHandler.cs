using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.Application.Models.Response.ShopDocument;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ShopDocument.Commands
{
    public record AddShopDocumentCommand(AddShopDocumentRequest model, int ShopId) : IRequest<List<AddShopDocumentResponce>>;
    public class AddShopDocumentCommandHandler(
        IShopRepository shopRepository,
        IShopDocumentRepository shopDocumentRepository,
        IAzureBlobService azureBlobService,
        IMapper mapper)
        : IRequestHandler<AddShopDocumentCommand, List<AddShopDocumentResponce>>
    {
        private const string ContainerName = "dot-net";
        public async Task<List<AddShopDocumentResponce>> Handle(AddShopDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.ShopId <= 0) throw new BadRequestException("Shop Id cant be 0 or negative");
            var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken);
            if (!shopExists) throw new NotFoundException($"No shop with provided Id");

            await FilesExistsCheck(request.ShopId, request.model, cancellationToken);

            var documents = new[]
            {
                (request.model.RefundPolicy, ShopDocumentType.RefundPolicy),
                (request.model.ShippingPolicy, ShopDocumentType.ShippingPolicy),
                (request.model.TermsAndConditions, ShopDocumentType.TermsAndConditions),
                (request.model.PrivacyPolicy, ShopDocumentType.PrivacyPolicy)
            }
            .Where(x => x.Item1 is not null)
            .ToList();

            List<AddShopDocumentResponce> responces = new List<AddShopDocumentResponce>();
            foreach (var (file, documentType) in documents)
            {
                if (file is null)
                    continue;
                var blobName = await AddDoc(file);

                var document = new ShopDocumentEntity
                {
                    ShopId = request.ShopId,
                    DocumentType = documentType,
                    FileName = file!.FileName,
                    FileSize = file.Length,
                    BlobName = blobName,
                    CreatedAt = DateTime.UtcNow
                };
                var responce = mapper.Map<AddShopDocumentResponce>(document);
                await shopDocumentRepository.AddAsync(document, cancellationToken);
                responces.Add(responce);
            }

            return responces;
        }

        private async Task<string> AddDoc(IFormFile file)
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

        private async Task FilesExistsCheck(
            int shopId,
            AddShopDocumentRequest model,
            CancellationToken cancellationToken)
        {
            SpecificationBuilder<ShopDocumentEntity> builder = new SpecificationBuilder<ShopDocumentEntity>();
            builder.AddCriteria(x => x.ShopId.Equals(shopId));
            var documents = await shopDocumentRepository.GetAllBySpecAsync(builder.Build(), cancellationToken);
            List<string> existingDocuments = new List<string>();
            foreach (var document in documents)
            {
                switch ((int)document.DocumentType)
                {
                    case 1:
                        if (model.RefundPolicy is not null) existingDocuments.Add("Refund policy");
                        break;
                    case 2:
                        if (model.ShippingPolicy is not null) existingDocuments.Add("Shipping policy");
                        break;
                    case 3:
                        if (model.TermsAndConditions is not null) existingDocuments.Add("Terms and conditions");
                        break;
                    case 4:
                        if (model.PrivacyPolicy is not null) existingDocuments.Add("Privacy policy");
                        break;
                }
            }

            if (existingDocuments.Count > 0)
                throw new BadRequestException($"There are already following documents: {string.Join(", ", existingDocuments)}.");
        }
    }
}
