namespace NettoRetail.Application.Models.Request.Address;

public class UpdateAddressRequest : CreateAddressRequest
{
    public bool IsDefault { get; set; }
}
