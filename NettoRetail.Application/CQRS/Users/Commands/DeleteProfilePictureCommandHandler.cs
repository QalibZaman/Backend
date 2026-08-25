using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands
{
    public class DeleteProfilePictureCommandHandler(
     ICustomerRepository customerRepository,
     IStaffRepository staffRepository,
     IHttpContextAccessor httpContextAccessor,
     IAzureBlobService azureBlobService)
     : IRequestHandler<DeleteProfilePictureCommand>
    {
        public async Task Handle(
            DeleteProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var id))
                throw new UnauthorizedAccessException("User is not authenticated.");

            var customer = await customerRepository.GetWithRelatedDataAsync(
                new GetCustomerByIdSpecification(id),
                cancellationToken);

            if (customer is not null)
            {
                await DeleteProfilePictureAsync(
                    customer.ProfileImageUrl,
                    cancellationToken);

                customer.ProfileImageUrl = null;

                await customerRepository.UpdateAsync(
                    customer,
                    cancellationToken);

                return;
            }

            var staff = await staffRepository.GetWithRelatedDataAsync(
                new GetStaffByIdSpecification(id),
                cancellationToken);

            if (staff is not null)
            {
                await DeleteProfilePictureAsync(
                    staff.ProfileImageUrl,
                    cancellationToken);

                staff.ProfileImageUrl = null;

                await staffRepository.UpdateAsync(
                    staff,
                    cancellationToken);

                return;
            }

            throw new NotFoundException("User not found.");
        }

        private async Task DeleteProfilePictureAsync(
            string? profileImageUrl,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(profileImageUrl))
                throw new BadRequestException("Profile image does not exist.");

            await azureBlobService.DeleteByUrlAsync(
                profileImageUrl,
                cancellationToken);
        }
    }
}
