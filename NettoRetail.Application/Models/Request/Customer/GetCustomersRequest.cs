namespace NettoRetail.Application.Models.Request.Customer;

public class GetCustomersRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }

}
