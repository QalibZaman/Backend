namespace NettoRetail.Application.Interfaces;

public interface ILinkService
{
    string BuildNewsletterUnsubscribeUrl(Guid accountId);
}