using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands
{
    public class UploadProfilePictureCommandHandler(
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor,
    IAzureBlobService azureBlobService,
    IImageValidatorService imageValidatorService)
    : IRequestHandler<UploadProfilePictureCommand>
    {
        private const string ContainerName = "dot-net";

        public async Task Handle(
            UploadProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var id))
                throw new UnauthorizedAccessException("User is not authenticated.");

            await imageValidatorService.ValidateProfileImageAsync(
                request.Model.File,
                cancellationToken);

            var customer = await customerRepository.GetWithRelatedDataAsync(
                new GetCustomerByIdSpecification(id),
                cancellationToken);

            if (customer is not null)
            {
                customer.ProfileImageUrl = await UploadImageAsync(
                    request.Model.File,
                    cancellationToken);

                await customerRepository.UpdateAsync(customer, cancellationToken);
                await customerRepository.CommitAsync(cancellationToken);

                return;
            }

            var staff = await staffRepository.GetWithRelatedDataAsync(
                new GetStaffByIdSpecification(id),
                cancellationToken);

            if (staff is not null)
            {
                staff.ProfileImageUrl = await UploadImageAsync(
                    request.Model.File,
                    cancellationToken);

                await staffRepository.UpdateAsync(staff, cancellationToken);
                await staffRepository.CommitAsync(cancellationToken);

                return;
            }

            throw new NotFoundException("User not found.");
        }

        private async Task<string> UploadImageAsync(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(file.FileName);

            var fileName = $"{Guid.NewGuid()}{extension}";

            await using var stream = file.OpenReadStream();

            return await azureBlobService.UploadAsync(
                fileName,
                stream,
                ContainerName,
                BlobContentCategoryEnum.Image,
                overwrite: true,
                contentType: file.ContentType);
        }
    }
}
