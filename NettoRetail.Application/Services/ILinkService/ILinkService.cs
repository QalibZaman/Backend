namespace NettoRetail.Application.Services.ILinkService;

public interface ILinkService
{
    public string CreateCustomerAccountVerificationLink(string email, int shopId);
    public string CreateCustomerResetPasswordLink(string email, int shopId);
    string BuildNewsletterUnsubscribeUrl(Guid accountId);
}
