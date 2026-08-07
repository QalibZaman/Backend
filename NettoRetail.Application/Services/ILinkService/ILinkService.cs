namespace NettoRetail.Application.Services.ILinkService;

public interface ILinkService
{
    public string CreateBlockedAccountReactivationLink(string email, Guid accountId);
}
