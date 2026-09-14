namespace NettoRetail.Application.Interfaces;

public interface ILinkService
{
    string BuildNewsletterUnsubscribeUrl(Guid accountId);
    public string CreateBlockedAccountReactivationLink(string email, Guid accountId);
}