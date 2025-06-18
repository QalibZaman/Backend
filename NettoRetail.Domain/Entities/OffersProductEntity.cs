namespace NettoRetail.Domain.Entities;

public class OffersProductEntity
{
    public int Id { get; set; }
    public required int OfferId { get; set; }
    public OfferEntity? Offer { get; set; }
    public required int ProductId { get; set; }
    public ProductDetailsEntity? Product { get; set; }
    public required int DisplayOrder { get; set; }
}
