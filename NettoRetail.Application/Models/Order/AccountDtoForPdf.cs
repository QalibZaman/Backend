namespace NettoRetail.Application.Models.Order;

public class AccountDtoForPdf
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}