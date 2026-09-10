using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces.Services.UserService;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.PersonalInformation
{
    public class UpdatePersonalInformationCommandHandler(
     ICustomerRepository customerRepository,
     IStaffRepository staffRepository,
     IUserService userService,
     IHttpContextAccessor httpContextAccessor)
     : IRequestHandler<UpdatePersonalInformationCommand>
    {
        public async Task Handle(
            UpdatePersonalInformationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            var customer = await customerRepository.GetWithRelatedDataAsync(
                new GetCustomerByIdSpecification(id),
                cancellationToken);

            if (customer is not null)
            {
                await userService.UpdatePersonalInformationAsync(
                    customer,
                    request.Model,
                    cancellationToken);

                await customerRepository.CommitAsync(
                    cancellationToken);

                return;
            }

            var staff = await staffRepository.GetWithRelatedDataAsync(
                new GetStaffByIdSpecification(id),
                cancellationToken);

            if (staff is not null)
            {
                await userService.UpdatePersonalInformationAsync(
                    staff,
                    request.Model,
                    cancellationToken);

                await staffRepository.CommitAsync(
                    cancellationToken);

                return;
            }

            throw new NotFoundException("User not found.");
        }
    }
}
