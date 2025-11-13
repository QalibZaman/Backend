using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Models.Order;

public class SearchFieldModel
{
    public required string Field { get; set; }
    public required string SearchTerm { get; set; }

    public static ICollection<SearchOrderResponse> AdaptSearchOrderResponse(IEnumerable<OrderEntity> entities)
    {
        List<SearchOrderResponse> responseList = new List<SearchOrderResponse>(capacity: entities.Count());
        foreach (var entity in entities)
        {
            var response = new SearchOrderResponse
            {
                Id = entity.Id,
                CreationDate = entity.CreationDate,
                FullName = entity.Customer.Account.FullName,
                Email = entity.Customer.Account.Email,
                OrderStatus = entity.OrderStatus,
                Creator = entity.Creator
            };
            responseList.Add(response);
        }

        return responseList;
    }
}