using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class CreateOrderModel
{
    public Guid? CustomerId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string DeliveryType { get; set; }
    public required IDictionary<Guid, int> Products { get; set; }
}