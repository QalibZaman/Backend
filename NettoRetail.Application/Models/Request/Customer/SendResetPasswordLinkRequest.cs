namespace NettoRetail.Application.Models.Request.Customer;

public class SendResetPasswordLinkRequest
{
    public string Email { get; set; }
    public int ShopId { get; set; }
}
