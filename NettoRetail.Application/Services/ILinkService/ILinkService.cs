namespace NettoRetail.Application.Services.ILinkService;

public interface ILinkService
{
    public string CreateCustomerAccountVerificationLink(string email);
    string BuildNewsletterUnsubscribeUrl(Guid accountId);
}
