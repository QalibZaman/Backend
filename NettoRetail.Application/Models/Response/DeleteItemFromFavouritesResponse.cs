using Mapster;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Models.Response;

public class DeleteItemFromFavouritesResponse
{
    public List<InventoryItemCardResponse> Items { get; set; } = new List<InventoryItemCardResponse>();

    public static DeleteItemFromFavouritesResponse AdaptToResponse(IEnumerable<FavouriteListItemEntity> favourites)
    {
        return new DeleteItemFromFavouritesResponse()
        {
            Items = favourites
            .Where(f => f.InventoryItem != null)
            .Select(f => f.InventoryItem.Adapt<InventoryItemCardResponse>())
            .ToList()
        };
    }
}
