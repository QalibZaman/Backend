using MapsterMapper;
using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces.Services.UserService
{
    public class UserService(IMapper mapper) : IUserService
    {
        public Task UpdatePersonalInformationAsync(
            CustomerEntity customer,
            UpdatePersonalInformationRequest request,
            CancellationToken cancellationToken = default)
        {
            MapUser(customer, request);

            if (customer.Address is null)
            {
                customer.Address = new AddressEntity
                {
                    CustomerId = customer.Id
                };
            }

            MapAddress(customer.Address, request);

            return Task.CompletedTask;
        }

        public Task UpdatePersonalInformationAsync(
            StaffEntity staff,
            UpdatePersonalInformationRequest request,
            CancellationToken cancellationToken = default)
        {
            MapUser(staff, request);

            if (staff.Address is null)
            {
                staff.Address = new AddressEntity
                {
                    StaffId = staff.Id
                };
            }

            MapAddress(staff.Address, request);

            return Task.CompletedTask;
        }

        private void MapUser(
            BaseUserEntity user,
            UpdatePersonalInformationRequest request)
        {
            mapper.Map(request, user);
        }

        private void MapAddress(
            AddressEntity address,
            UpdatePersonalInformationRequest request)
        {
            mapper.Map(request, address);
        }
    }
}
