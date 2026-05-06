using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Enums;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record UpdateCustomerProfileImageCommand(Guid UserId, IFormFile Image) : IRequest;

public class UpdateCustomerProfileImageCommandHandler(
    ICustomerRepository customerRepository,
    IAzureBlobService azureBlobService,
    IUnitOfWork unitOfWork,
    IOptions<ConnectionStrings> connectionStrings,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<UpdateCustomerProfileImageCommand>
{
    public async Task Handle(UpdateCustomerProfileImageCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetCustomerByIdSpecification(request.UserId);
        var account = await customerRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Account not found");

        var azureConnectionString = connectionStrings.Value.AzureConnection;
        var customerContainerName = azureBlobStorageOptions.Value.ContainerName;

        var fileName = $"customer-profile-images/{Guid.NewGuid()}-{account.ShopId}-{(int)BlobAccess.Private}{Path.GetExtension(request.Image.FileName)}";

        using var transaction = unitOfWork.BeginTransaction();

        if (fileName != null)
        {
            await using var stream = request.Image.OpenReadStream();
            var imageUrl = await azureBlobService.UploadAsync(fileName, stream, customerContainerName, BlobContentCategoryEnum.Image, true);
            account.ProfileImageUrl = imageUrl;
        }

        await customerRepository.UpdateAsync(account, cancellationToken);

        transaction.Commit();
    }
}
