namespace NettoRetail.Application.Interfaces;

public interface IInvitationTokenService
{
    string GenerateRawToken();
    string Hash(string rawToken);

}