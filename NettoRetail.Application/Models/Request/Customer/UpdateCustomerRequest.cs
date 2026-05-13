namespace NettoRetail.Application.Models.Request.Customer;

public class UpdateCustomerRequest : CreateCustomerRequest
{
    public bool? IsActive { get; set; }
}
