using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Models.Order;

public class SearchFieldModel
{
    public required string Field { get; set; }
    public required string SearchTerm { get; set; }

    public enum SearchCriteria
    {
        None,
        CustomerEmail,
        OrderId
    }
}