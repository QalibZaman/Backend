namespace NettoRetail.Application.Services.IPasswordService
{
    public interface IPasswordService
    {
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
        Task<bool> ValidatePasswordAsync(Guid userId, string password, CancellationToken cancellationToken);
    }
}
