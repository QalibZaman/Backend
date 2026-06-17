using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Utilities;

namespace NettoRetail.Application.Models.Order;

public class GetFilteredOrdersModel
{
    public List<Guid>? CustomerIds { get; set; }
    public List<string>? CustomerFirstNames { get; set; }
    public List<string>? CustomerLastNames { get; set; }
    public List<string>? CustomerEmails { get; set; }
    public List<int>? OrderIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? OrderStatuses { get; set; }
    public List<string>? DeliveryTypes { get; set; }
    public List<string>? Creators { get; set; }

    [ModelBinder(BinderType = typeof(SortingOptionsModelBinder))]
    public List<SortingOption>? SortingOptions { get; set; }
}
