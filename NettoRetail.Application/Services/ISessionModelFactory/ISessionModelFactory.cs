using NettoRetail.Application.Models.Entities;
using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Application.Services.ISessionModelFactory;

public interface ISessionModelFactory
{
    public Task<SessionModel> CreateSessionModel(string userAgentString, string ips, string jwt, string requestToken, Location? location, DateTime expiresAt, CancellationToken cancellationToken);
}
