using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces.Services.UserService
{
    public interface IUserService
    {
        Task UpdatePersonalInformationAsync(
            CustomerEntity customer,
            UpdatePersonalInformationRequest request,
            CancellationToken cancellationToken = default);

        Task UpdatePersonalInformationAsync(
            StaffEntity staff,
            UpdatePersonalInformationRequest request,
            CancellationToken cancellationToken = default);
    }
}
