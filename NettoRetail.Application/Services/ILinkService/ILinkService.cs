namespace NettoRetail.Application.Services.ILinkService;

public interface ILinkService
{
    public Task<string> CreateCustomerAccountVerificationLink(string email);
}
