namespace NettoRetail.Application.Models.Response;

public class OrderListResponse
{
    public required int Id { get; set; }
    public Guid? CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required DateTime DateCreated { get; set; }
    public required string OrderStatus { get; set; }
    public required string DeliveryType { get; set; }
    public required string Creator { get; set; }
    public required bool CanEditData { get; set; }
}
