using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ShopDocument
{
    public class GetShopDocumentByShopIdandDocumentType(int shopId, int documentType)
            : Specification<ShopDocumentEntity>(
            document => document.ShopId == shopId &&
            (int)document.DocumentType == documentType);
}
