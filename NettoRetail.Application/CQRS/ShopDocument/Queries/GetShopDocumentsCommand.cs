using Azure.Storage.Sas;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.ShopDocument;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ShopDocument.Queries
{
    public record GetShopDocumentsCommand(int shopId) : IRequest<List<GetShopDocumentResponce>>;

    public class GetShopDocumentCommandHandler(
        IShopRepository shopRepository,
        IShopDocumentRepository shopDocumentRepository,
        IAzureBlobService azureBlobService) : IRequestHandler<GetShopDocumentsCommand, List<GetShopDocumentResponce>>
    {
        public async Task<List<GetShopDocumentResponce>> Handle(GetShopDocumentsCommand request, CancellationToken cancellationToken)
        {
            if (request.shopId <= 0) throw new BadRequestException("Shop Id cant be 0 or negative");
            var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.shopId), cancellationToken);
            if (!shopExists) throw new NotFoundException($"No shop with provided Id");
            SpecificationBuilder<ShopDocumentEntity> builder = new SpecificationBuilder<ShopDocumentEntity>();
            builder.AddCriteria(d => d.ShopId.Equals(request.shopId));
            IEnumerable<ShopDocumentEntity> shopDocuments = await shopDocumentRepository.GetAllBySpecAsync(builder.Build(), cancellationToken);
            List<GetShopDocumentResponce> responce = new List<GetShopDocumentResponce>();
            List<string> documentTypes = new List<string> { "Refund policy", "Shipping policy", "Terms and conditions", "Privacy policy" };
            foreach (var document in shopDocuments)
            {
                GetShopDocumentResponce resp = new GetShopDocumentResponce
                {
                    DocumentType = documentTypes[(int)document.DocumentType - 1],
                    FileName = document.FileName,
                    FileType = GetFileType(document.FileName),
                    FileUrl = azureBlobService.GenerateBlobSasUrlFromUrl(
                        document.BlobName,
                        BlobSasPermissions.Read,
                        TimeSpan.FromMinutes(15))
                };
                responce.Add(resp);
            }

            return responce;
        }

        public string GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".odt" => "application/vnd.oasis.opendocument.text",
                _ => "application/octet-stream"
            };
        }
    }
}
