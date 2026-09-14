using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands
{
    public class ChangeProfilePictureCommandHandler(
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    IAzureBlobService azureBlobService,
    IImageValidatorService imageValidatorService)
    : IRequestHandler<ChangeProfilePictureCommand>
    {
        private const string ContainerName = "dot-net";

        public async Task Handle(
            ChangeProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            await imageValidatorService.ValidateProfileImageAsync(
                request.Model.File,
                cancellationToken);

            var customer = await customerRepository.GetWithRelatedDataAsync(
                new GetCustomerByIdSpecification(request.UserId),
                cancellationToken);

            if (customer is not null)
            {
                if (string.IsNullOrWhiteSpace(customer.ProfileImageUrl))
                    throw new BadRequestException(ValidationErrorMessages.User.ProfileImageDoesNotExistErrorMessage);

                var oldProfileImageUrl = customer.ProfileImageUrl;

                var newProfileImageUrl = await UploadImageAsync(
                    request.Model.File,
                    cancellationToken);

                customer.ProfileImageUrl = newProfileImageUrl;

                await customerRepository.UpdateAsync(
                    customer,
                    cancellationToken);

                await DeleteOldImageAsync(
                    oldProfileImageUrl,
                    cancellationToken);

                return;
            }

            var staff = await staffRepository.GetWithRelatedDataAsync(
                new GetStaffByIdSpecification(request.UserId),
                cancellationToken);

            if (staff is not null)
            {
                if (string.IsNullOrWhiteSpace(staff.ProfileImageUrl))
                    throw new BadRequestException(ValidationErrorMessages.User.ProfileImageDoesNotExistErrorMessage);

                var oldProfileImageUrl = staff.ProfileImageUrl;

                var newProfileImageUrl = await UploadImageAsync(
                    request.Model.File,
                    cancellationToken);

                staff.ProfileImageUrl = newProfileImageUrl;

                await staffRepository.UpdateAsync(
                    staff,
                    cancellationToken);

                await DeleteOldImageAsync(
                    oldProfileImageUrl,
                    cancellationToken);

                return;
            }

            throw new NotFoundException(ValidationErrorMessages.User.UserNotFoundErrorMessage);
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

        private async Task DeleteOldImageAsync(
            string profileImageUrl,
            CancellationToken cancellationToken)
        {
            await azureBlobService.DeleteByUrlAsync(
                profileImageUrl,
                cancellationToken);
        }
    }
}
