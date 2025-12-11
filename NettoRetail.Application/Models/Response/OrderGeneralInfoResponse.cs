namespace NettoRetail.Application.Models.Response;

public class OrderGeneralInfoResponse
{
    public required int OrderId { get; set; }
    public required Guid CustomerId { get; set; }
    public required string CustomerEmail { get; set; }
    public required string OrderStatus { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string DeliveryType { get; set; }
    public required string Creator { get; set; }
    public DateTime DateCreated { get; set; }
}